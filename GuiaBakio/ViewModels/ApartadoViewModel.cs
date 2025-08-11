using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GuiaBakio.ViewModels
{
    public partial class ApartadoViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly ITextEditorPopupService _textEditorPopupService;
        private readonly IAddItemPopupService _addItemPopupService;
        private readonly IAddImagenPopupService _addImagenPopupService;
        private readonly IDialogOKService _dialogService;
        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarNotaAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarApartadoAsyncCommand { get; }

        public ApartadoViewModel(DataBaseService dbService, IAddItemPopupService addItemPopupService, IAddImagenPopupService addImagenPopupService, ITextEditorPopupService textEditorPopupService, IDialogOKService dialogService)
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            AgregarNotaAsyncCommand = new AsyncRelayCommand(AgregarNotaAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarApartadoAsyncCommand = new AsyncRelayCommand(EliminarApartadoAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
            _addItemPopupService = addItemPopupService ?? throw new ArgumentNullException(nameof(_addItemPopupService));
            _addImagenPopupService = addImagenPopupService ?? throw new ArgumentNullException(nameof(addImagenPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
        }

        [ObservableProperty]
        private string titulo;

        [ObservableProperty]
        private int apartadoId;

        [ObservableProperty]
        private Apartado? apartado;


        [ObservableProperty]
        private ObservableCollection<Nota> notas = new();

        [ObservableProperty]
        private ObservableCollection<MiImagen> imagenes = new();

        [ObservableProperty]
        private bool noHayTexto;
        [ObservableProperty]
        private bool noHayNotas;
        [ObservableProperty]
        private bool noHayImagenes;

        public async Task CargarDatosAsync(int apartadoId)
        {
            if (apartadoId <= 0)
            {
                throw new ArgumentNullException(nameof(apartadoId), "La Id del apartado debe ser mayor que 0.");
            }
            ApartadoId = apartadoId;
            try
            {
                Apartado = await _dbService.ObtenerApartadoAsync(ApartadoId);
                var localidad = await _dbService.ObtenerLocalidadAsync(Apartado.LocalidadId);
                Titulo = localidad?.Nombre + " - " + Apartado.Nombre;
                Notas = new ObservableCollection<Nota>(
                    await _dbService.ObtenerNotasAsync(ApartadoId));
                Imagenes = new ObservableCollection<MiImagen>(
                    await _dbService.ObtenerImagenesPorEntidadAsync(TipoEntidad.Apartado, ApartadoId));
                await AsignarImagenSourceAsync();
                NoHayTexto = string.IsNullOrWhiteSpace(Apartado?.Texto);
                NoHayNotas= !Notas?.Any() == true;
                NoHayImagenes = !Imagenes?.Any() == true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cargar datos del apartado. {ex.Message}");
            }
        }

        private async Task AsignarImagenSourceAsync()
        {
            foreach (var imagen in Imagenes)
            {
                imagen.ImagenSource = await DataBaseService.ConvertirBytesAImageSourceAsync(imagen.Foto);
            }
        }

        [RelayCommand]
        public async Task EliminarApartadoAsync()
        {
            try
            {
                int eliminado = await _dbService.EliminarApartadoAsync(ApartadoId);
                if (eliminado > 0)
                {
                    await Shell.Current.GoToAsync("mainPage");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al eliminar", $"Hubo un error al eliminar el apartado. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
            //
        }

        [RelayCommand]
        public async Task EditarTextoAsync()
        {
            try
            {
                var resultado = await _textEditorPopupService.MostrarEditorAsync(Apartado?.Texto);

                if (resultado is null)
                {
                    return;
                }
                string? v = Apartado?.Texto = resultado;
                await _dbService.ActualizarApartadoAsync(Apartado);
                await CargarDatosAsync(ApartadoId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualizar el texto del apartado. El texto no fue guardado. {Environment.NewLine}{ex.Message}", "OK");
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
                await _dialogService.ShowAlertAsync("Error", "El nombre de la nota no puede estar vacío.", "OK");
                return;
            }
            try
            {
                bool yaExiste = await _dbService.ExisteNotaAsync(nuevaNota, Apartado.Id);
                if (yaExiste)
                {
                    await _dialogService.ShowAlertAsync("Error", "Nota existente.", "OK");
                    return;
                }
                var id = await _dbService.InsertarNotaAsync(nuevaNota,"", Apartado.Id);
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
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir la nota.{Environment.NewLine}{ex.Message}", "OK");
            }

        }

        [RelayCommand]
        public async Task AgregarImagenAsync()
        {
            //var nueva = new ImagenLocalidad { Url = "https://ejemplo.com/imagen.jpg", LocalidadId = LocalidadId };
            //Imagenes.Add(nueva);
            //await _dbService.GuardarImagenAsync(nueva);
            OnPropertyChanged(nameof(Imagenes));
        }
    }
}
