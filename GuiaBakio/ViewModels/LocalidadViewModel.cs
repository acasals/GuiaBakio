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
        private readonly INavigationDataService _navigationDataService;
        private readonly Usuario usuario;

        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarNotaAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarLocalidadAsyncCommand { get; }
        public IRelayCommand<Foto?> ImagenTocadaAsyncCommand { get; }
        public IRelayCommand ToggleEtiquetaCommand => new RelayCommand<Etiqueta>(async etiqueta =>
        {
            etiqueta?.IsSelected = !etiqueta.IsSelected;
            Debug.WriteLine($"Etiqueta {etiqueta?.Nombre} seleccionada: {etiqueta?.IsSelected}");
            EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
            NotasFiltradas = (await _dbService.ObtenerNotasPorEtiquetasAsync(LocalidadId, EtiquetasSeleccionadas.ToList())).ToObservableCollection();
        });

        public LocalidadViewModel(
            DataBaseService dbService,
            IAddItemPopupService addItemPopupService,
            IAddImagenPopupService addImagenPopupService,
            ITextEditorPopupService textEditorPopupService,
            IDialogOKService dialogService,
            INavigationDataService navigationDataService)
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            AgregarNotaAsyncCommand = new AsyncRelayCommand(AgregarNotaAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarLocalidadAsyncCommand = new AsyncRelayCommand(EliminarLocalidadAsync);
            ImagenTocadaAsyncCommand = new AsyncRelayCommand<Foto?>(ImagenTocadaAsync);

            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
            _addItemPopupService = addItemPopupService ?? throw new ArgumentNullException(nameof(_addItemPopupService));
            _addImagenPopupService = addImagenPopupService ?? throw new ArgumentNullException(nameof(addImagenPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationDataService = navigationDataService ?? throw new ArgumentNullException(nameof(navigationDataService));

            usuario = _navigationDataService.Data as Usuario;
            if (usuario == null)
            {
                _dialogService.ShowAlertAsync("Error", "No se pudo cargar el usuario.", "OK");
            }
        }

        [ObservableProperty]
        private string localidadId;

        [ObservableProperty]
        private Localidad? localidad;

        [ObservableProperty]
        private bool creadoPorUsuario;

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

        [ObservableProperty]
        private bool hayImagenes;

        public async Task CargarDatosAsync(string localidadId)
        {
            if (string.IsNullOrWhiteSpace(localidadId))
            {
                throw new ArgumentNullException(nameof(localidadId), "La Id de localidad no puede estar vacía.");
            }
            LocalidadId = localidadId;
            try
            {
                Localidad = await _dbService.ObtenerLocalidadPorIdAsync(LocalidadId);
                Notas = new ObservableCollection<Nota>(
                    await _dbService.ObtenerNotasPorLocalidadAsync(LocalidadId));
                Etiquetas = new ObservableCollection<Etiqueta>(
                    await _dbService.ObtenerTodasLasEtiquetasAsync());
                Imagenes = new ObservableCollection<Foto>(
                await _dbService.ObtenerImagenesPorEntidadAsync(TipoEntidad.Localidad, LocalidadId));
                NoHayTexto = string.IsNullOrWhiteSpace(Localidad?.Texto) && Localidad?.CreadorId == usuario?.Id;
                NoHayNotas = !Notas?.Any() == true;
                NoHayImagenes = !Imagenes?.Any() == true;
                HayImagenes = Imagenes?.Any() == true;
                EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
                NotasFiltradas = (await _dbService.ObtenerNotasPorEtiquetasAsync(LocalidadId, EtiquetasSeleccionadas.ToList())).ToObservableCollection();
                CreadoPorUsuario = (Localidad?.CreadorId == usuario?.Id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cargar datos de localidad. {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task EliminarLocalidadAsync()
        {
            try
            {
                if (Localidad == null || usuario == null)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo obtener la localidad o el usuario.", "OK");
                    return;
                }
                if (Localidad.CreadorId != usuario.Id)
                {
                    await _dialogService.ShowAlertAsync("Error", "Sólo el usuario que creó la localidad puede borrarla", "OK");
                    return;
                }

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
                if (Localidad == null || usuario == null)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo obtener la localidad o el usuario.", "OK");
                    return;
                }
                if (Localidad.CreadorId != usuario.Id)
                {
                    await _dialogService.ShowAlertAsync("Error", "Sólo el usuario que creó la localidad puede editar el texto", "OK");
                    return;
                }

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
                if (Localidad == null || usuario == null)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo obtener la localidad o el usuario.", "OK");
                    return;
                }
                bool yaExiste = await _dbService.ExisteNotaPorTituloYLocalidadAsync(nuevaNota, Localidad.Id);
                if (yaExiste)
                {
                    await _dialogService.ShowAlertAsync("Error", "Nota existente.", "OK");
                    return;
                }
                var id = await _dbService.InsertarNotaAsync(nuevaNota, Localidad.Id, usuario.Id);
                if (string.IsNullOrWhiteSpace(id))
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
                miImagen.CreadorId = usuario.Id;
                string imagenId = await _dbService.InsertarImagensync(miImagen);
                if (!String.IsNullOrWhiteSpace(imagenId))
                {
                    miImagen.Id = imagenId;
                    Imagenes.Add(miImagen);
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo guardar la imagen en la base de datos.", "OK");
                    return;
                }

            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir la imagen.{Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task ImagenTocadaAsync(Foto? foto)
        {
            if (foto == null)
            {
                await _dialogService.ShowAlertAsync("Error", "No se pudo obtener la foto", "Aceptar");
                return;
            }
            try
            {
                if (foto.EsMapa)
                {
                    if (string.IsNullOrWhiteSpace(foto.UrlMapa))
                    {
                        await _dialogService.ShowAlertAsync("Error", "La foto no tiene una URL de mapa válida.", "Aceptar");
                        return;
                    }
                    await Launcher.OpenAsync(new Uri(foto.UrlMapa));
                }
                else
                {
                    byte[] blob = foto.Blob ?? [];
                    if (blob.Length == 0)
                    {
                        await _dialogService.ShowAlertAsync("Error", "La foto no tiene datos de imagen válidos.", "Aceptar");
                        return;
                    }
                    string tempFilePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
                    File.WriteAllBytes(tempFilePath, blob);
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(tempFilePath)
                    });
                    await Task.Delay(5000); // Ajusta según el comportamiento observado
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"Hubo un error al gestionar la imagen: {ex.Message}", "Aceptar");
            }
        }
    }
}