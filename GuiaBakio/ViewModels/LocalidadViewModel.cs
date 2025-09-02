using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GuiaBakio.ViewModels
{
    public partial class LocalidadViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly ITextEditorPopupService _textEditorPopupService;
        private readonly IAddImagenPopupService _addImagenPopupService;
        private readonly IAddItemPopupService _addItemPopupService;
        private readonly IDialogOKService _dialogService;
        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarNotaAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarLocalidadAsyncCommand { get; }
        public IRelayCommand ToggleEtiquetaCommand => new RelayCommand<Etiqueta>(async etiqueta =>
        {
            etiqueta?.IsSelected = !etiqueta.IsSelected;
            Debug.WriteLine($"Etiqueta {etiqueta?.Nombre} seleccionada: {etiqueta?.IsSelected}");
            EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
            NotasFiltradas = (await _dbService.ObtenerNotasPorEtiquetasAsync(LocalidadId, EtiquetasSeleccionadas.ToList())).ToObservableCollection();
        });

        public LocalidadViewModel(DataBaseService dbService, IAddItemPopupService addItemPopupService, IAddImagenPopupService addImagenPopupService, ITextEditorPopupService textEditorPopupService, IDialogOKService dialogService)
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            AgregarNotaAsyncCommand = new AsyncRelayCommand(AgregarNotaAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarLocalidadAsyncCommand = new AsyncRelayCommand(EliminarLocalidadAsync);

            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
            _addItemPopupService = addItemPopupService ?? throw new ArgumentNullException(nameof(_addItemPopupService));
            _addImagenPopupService = addImagenPopupService ?? throw new ArgumentNullException(nameof(addImagenPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        [ObservableProperty]
        private int localidadId;

        [ObservableProperty]
        private Localidad? localidad;


        [ObservableProperty]
        private ObservableCollection<Nota> notas = new();

        [ObservableProperty]
        private ObservableCollection<Etiqueta> etiquetas = new();

        [ObservableProperty]
        private ObservableCollection<Etiqueta> etiquetasSeleccionadas = new();

        [ObservableProperty]
        private ObservableCollection<Nota> notasFiltradas = new();

        [ObservableProperty]
        private ObservableCollection<Foto> imagenes = new();

        [ObservableProperty]
        private bool noHayTexto;

        [ObservableProperty]
        private bool noHayNotas;

        [ObservableProperty]
        private bool noHayImagenes;

        public async Task CargarDatosAsync(int localidadId)
        {
            if (localidadId <= 0)
            {
                throw new ArgumentNullException(nameof(localidadId), "La Id de localidad debe ser mayor que 0.");
            }
            LocalidadId = localidadId;
            try
            {
                Localidad = await _dbService.ObtenerLocalidadAsync(LocalidadId);
                Notas = new ObservableCollection<Nota>(
                    await _dbService.ObtenerNotasAsync(LocalidadId));
                Etiquetas = new ObservableCollection<Etiqueta>(
                    await _dbService.ObtenerEtiquetasAsync());
                Imagenes = new ObservableCollection<Foto>(
                await _dbService.ObtenerImagenesPorEntidadAsync(TipoEntidad.Localidad, LocalidadId));
                await AsignarImagenSourceAsync();
                NoHayTexto = string.IsNullOrWhiteSpace(Localidad?.Texto);
                NoHayNotas = !Notas?.Any() == true;
                NoHayImagenes = !Imagenes?.Any() == true;
                EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
                NotasFiltradas = (await _dbService.ObtenerNotasPorEtiquetasAsync(LocalidadId, EtiquetasSeleccionadas.ToList())).ToObservableCollection();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cargar datos de localidad. {ex.Message}");
            }
        }

        private async Task AsignarImagenSourceAsync()
        {
            foreach (var imagen in Imagenes)
            {
                imagen.ImagenSource = await DataBaseService.ConvertirBytesAImageSourceAsync(imagen.Blob);
            }
        }

        [RelayCommand]
        public async Task EliminarLocalidadAsync()
        {
            try
            {
                int eliminado = await _dbService.EliminarLocalidadAsync(LocalidadId);
                if (eliminado > 0)
                {
                    await Shell.Current.GoToAsync("mainPage");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al eliminar", $"Hubo un error al eliminar la localidad. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
            //
        }

        [RelayCommand]
        public async Task EditarTextoAsync()
        {
            try
            {
                var resultado = await _textEditorPopupService.MostrarEditorAsync(Localidad?.Texto);

                if (resultado is null)
                {
                    return;
                }
                string? v = Localidad?.Texto = resultado;
                await _dbService.ActualizarLocalidadAsync(Localidad);
                await CargarDatosAsync(LocalidadId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualizar la localidad. El texto no fue guardado. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task AgregarNotaAsync()
        {
            var nuevaNota = await _addItemPopupService.MostrarAsync("Añade una nota");
            if (nuevaNota is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(nuevaNota))
            {
                await _dialogService.ShowAlertAsync("Error", "El nombre del nota no puede estar vacío.", "OK");
                return;
            }
            try
            {
                bool yaExiste = await _dbService.ExisteNotaAsync(nuevaNota, Localidad.Id);
                if (yaExiste)
                {
                    await _dialogService.ShowAlertAsync("Error", "Nota existente.", "OK");
                    return;
                }
                var id = await _dbService.InsertarNotaAsync(nuevaNota, Localidad.Id);
                if (id <= 0)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo añadir la nota.", "OK");
                    return;
                }
                try
                {
                    await Shell.Current.GoToAsync($"notaPage?Id={id}");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Error", $"No se pudo navegar a la página de la nota.{Environment.NewLine}{ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir el nota.{Environment.NewLine}{ex.Message}", "OK");
            }

        }

        [RelayCommand]
        public async Task AgregarImagenAsync()
        {
            try
            {
                var miImagen = await _addImagenPopupService.MostrarAsync();
                if (miImagen is null)
                {
                    return;
                }
                if (miImagen.Blob is null || miImagen.Blob.Length == 0)
                {
                    await _dialogService.ShowAlertAsync("Error", "La imagen no puede estar vacía.", "OK");
                    return;
                }
                miImagen.EntidadId = LocalidadId;
                miImagen.TipoDeEntidad = TipoEntidad.Localidad;
                await _dbService.InsertarImagensync(miImagen);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir la imagen.{Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }
    }
}
