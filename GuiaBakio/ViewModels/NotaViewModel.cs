using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;

namespace GuiaBakio.ViewModels
{
    public partial class NotaViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly ITextEditorPopupService _textEditorPopupService;
        private readonly IAddImagenPopupService _addImagenPopupService;
        private readonly IDialogOKService _dialogService;
        private readonly IEtiquetaEditorPopupService _etiquetasEditorPopupService;
        private readonly INavigationDataService _navigationDataService;

        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarNotaAsyncCommand { get; }
        public IRelayCommand EditarEtiquetasAsyncCommand { get; }
        public IRelayCommand<Foto?> ImagenTocadaAsyncCommand { get; }

        private Usuario usuario;

        public NotaViewModel(
            DataBaseService dbService,
            ITextEditorPopupService textEditorPopupService,
            IAddImagenPopupService addImagenPopupService,
            IDialogOKService dialogService,
            IEtiquetaEditorPopupService etiquetasEditorPopupService,
            INavigationDataService navigationDataService)
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            EditarEtiquetasAsyncCommand = new AsyncRelayCommand(EditarEtiquetasAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarNotaAsyncCommand = new AsyncRelayCommand(EliminarNotaAsync);
            ImagenTocadaAsyncCommand = new AsyncRelayCommand<Foto?>(ImagenTocadaAsync);

            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
            _addImagenPopupService = addImagenPopupService ?? throw new ArgumentNullException(nameof(addImagenPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _etiquetasEditorPopupService = etiquetasEditorPopupService ?? throw new ArgumentNullException(nameof(etiquetasEditorPopupService));
            _navigationDataService = navigationDataService ?? throw new ArgumentNullException(nameof(navigationDataService));

            usuario = _navigationDataService.Data as Usuario;
            if (usuario == null)
            {
                _dialogService.ShowAlertAsync("Error", "No se pudo cargar el usuario.", "OK");
            }
        }

        [ObservableProperty]
        private string titulo;

        [ObservableProperty]
        private string notaId;

        [ObservableProperty]
        private Nota? nota;

        [ObservableProperty]
        private ObservableCollection<Foto> imagenes = new();

        [ObservableProperty]
        private bool noHayTexto;

        [ObservableProperty]
        private bool noHayEtiquetas;

        [ObservableProperty]
        private bool noHayImagenes;

        [ObservableProperty]
        private bool hayImagenes;

        [ObservableProperty]
        private bool creadoPorUsuario;

        [ObservableProperty]
        private ObservableCollection<Etiqueta> etiquetas = new();

        public async Task CargarDatosAsync(string notaId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
            {
                throw new ArgumentNullException(nameof(notaId), "La Id de la nota debe ser mayor que 0.");
            }
            NotaId = notaId;
            try
            {
                Nota = await _dbService.ObtenerNotaPorIdAsync(notaId);
                var localidad = await _dbService.ObtenerLocalidadPorNombreAsync(Nota.LocalidadId);
                Titulo = Nota.Titulo + " - " + localidad?.Nombre;
                Imagenes = new ObservableCollection<Foto>(
                    await _dbService.ObtenerImagenesPorEntidadAsync(TipoEntidad.Nota, notaId));
                NoHayTexto = string.IsNullOrWhiteSpace(Nota.Texto) && Nota.CreadorId == usuario?.Id;
                Etiquetas = new ObservableCollection<Etiqueta>(
                    await _dbService.ObtenerEtiquetasDeNotaAsync(Nota.Id));
                NoHayEtiquetas = Etiquetas is null || !Etiquetas.Any();
                NoHayImagenes = !Imagenes?.Any() == true;
                HayImagenes = Imagenes?.Any() == true;
                CreadoPorUsuario = Nota.CreadorId == usuario?.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cargar datos de la nota. {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task EditarTextoAsync()
        {
            try
            {
                var resultado = await _textEditorPopupService.MostrarEditorAsync(Nota?.Texto);

                if (resultado is null)
                {
                    return;
                }
                string? v = Nota?.Texto = resultado;
                await _dbService.ActualizarNotaAsync(Nota);
                await CargarDatosAsync(NotaId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualizar el texto de la nota. El texto no fue guardado. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task EditarEtiquetasAsync()
        {
            try
            {
                var resultado = await _etiquetasEditorPopupService.MostrarEditorAsync(Etiquetas);

                if (resultado is null)
                {
                    return;
                }
                List<Etiqueta>? nuevalistaEtiquetasDeNota = resultado;
                await _dbService.DesasignarEtiquetasANotaAsync(Nota.Id);
                if (nuevalistaEtiquetasDeNota != null && nuevalistaEtiquetasDeNota.Count > 0)
                {
                    await _dbService.AsignarEtiquetasANotaAsync(Nota.Id, nuevalistaEtiquetasDeNota);
                }
                await CargarDatosAsync(NotaId);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualizar las etiquetas de la nota. Las etiquetas no fueron guardadas. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task EliminarNotaAsync()
        {
            try
            {
                int eliminado = await _dbService.EliminarNotaAsync(NotaId);
                if (eliminado > 0)
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error al eliminar", $"Hubo un error al eliminar la nota. {Environment.NewLine}{ex.Message}", "OK");
                return;
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
                miImagen.EntidadId = NotaId;
                miImagen.TipoDeEntidad = TipoEntidad.Nota;
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
