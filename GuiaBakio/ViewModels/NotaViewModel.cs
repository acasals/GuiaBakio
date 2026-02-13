using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Models;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;


namespace GuiaBakio.ViewModels
{
    public partial class NotaViewModel : ObservableObject
    {
        //private readonly DataBaseService _dbService;
        //private readonly ITextEditorPopupService _textEditorPopupService;
        //private readonly IAddImagenPopupService _addImagenPopupService;
        //private readonly IDialogOKService _dialogService;
        //private readonly IEtiquetaLocalidadEditorPopupService _etiquetasLocalidadesEditorPopupService;
        //private readonly INavigationDataService _navigationDataService;

        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarNotaAsyncCommand { get; }
        public IRelayCommand EditarEtiquetasLocalidadesAsyncCommand { get; }
        public IRelayCommand<Foto?> ImagenTocadaAsyncCommand { get; }

        private readonly INotaServices _s;

        private Usuario? usuario;

        public NotaViewModel(INotaServices services)
        {
            _s = services;

            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            EditarEtiquetasLocalidadesAsyncCommand = new AsyncRelayCommand(EditarEtiquetasLocalidadesAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarNotaAsyncCommand = new AsyncRelayCommand(EliminarNotaAsync);
            ImagenTocadaAsyncCommand = new AsyncRelayCommand<Foto?>(ImagenTocadaAsync);

            usuario = _s.Navigation.Data as Usuario;

            if (usuario == null)
                _ = _s.Dialog.ShowAlertAsync("Error", "No se pudo cargar el usuario.", "OK");
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

        [ObservableProperty]
        private ObservableCollection<Localidad> localidades = new();

        public async Task CargarDatosAsync(string notaId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
            {
                throw new ArgumentNullException(nameof(notaId), "La Id de la nota debe ser mayor que 0.");
            }
            NotaId = notaId;
            try
            {
                Nota = await _s.Db.ObtenerNotaPorIdAsync(notaId);
                Titulo = Nota.Titulo;
                Imagenes = new ObservableCollection<Foto>(
                    await _s.Db.ObtenerImagenesPorEntidadAsync(TipoEntidad.Nota, notaId));
                NoHayTexto = string.IsNullOrWhiteSpace(Nota.Texto) && Nota.CreadorId == usuario?.Id;
                Etiquetas = new ObservableCollection<Etiqueta>(
                    await _s.Db.ObtenerEtiquetasDeNotaAsync(Nota.Id));
                Localidades = new ObservableCollection<Localidad>(
                    await _s.Db.ObtenerLocalidadesDeNotaAsync(Nota.Id));
                NoHayEtiquetas = (Etiquetas is null || !Etiquetas.Any()) && (Localidades is null || !Localidades.Any());
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
                var resultado = await _s.TextEditor.MostrarEditorAsync(Nota?.Texto);

                if (resultado is null)
                {
                    return;
                }
                string? v = Nota?.Texto = resultado;
                await _s.Db.ActualizarNotaAsync(Nota);
                await CargarDatosAsync(NotaId);
            }
            catch (Exception ex)
            {
                await _s.Dialog.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualizar el texto de la nota. El texto no fue guardado. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task EditarEtiquetasLocalidadesAsync()
        {
            try
            {
                var resultado = await _s.Etiquetas.MostrarEditorAsync(Etiquetas, Localidades);

                if (resultado is (null, null))
                {
                    return;
                }
                List<Etiqueta>? nuevalistaEtiquetasDeNota = resultado.Item1;
                await _s.Db.DesasignarEtiquetasANotaAsync(Nota.Id);
                if (nuevalistaEtiquetasDeNota != null && nuevalistaEtiquetasDeNota.Count > 0)
                {
                    await _s.Db.AsignarEtiquetasANotaAsync(Nota.Id, nuevalistaEtiquetasDeNota);
                }
                List<Localidad>? nuevalistaLocalidadesDeNota = resultado.Item2;
                await _s.Db.DesasignarLocalidadesANotaAsync(Nota.Id);
                if (nuevalistaLocalidadesDeNota != null && nuevalistaLocalidadesDeNota.Count > 0)
                {
                    await _s.Db.AsignarLocalidadesANotaAsync(Nota.Id, nuevalistaLocalidadesDeNota);
                }
                await CargarDatosAsync(NotaId);
            }
            catch (Exception ex)
            {
                await _s.Dialog.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualizar las etiquetas y/o localidades de la nota. Las etiquetas y/o localidades no fueron guardadas. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task EliminarNotaAsync()
        {
            try
            {
                int eliminado = await _s.Db.EliminarNotaAsync(NotaId);
                if (eliminado > 0)
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await _s.Dialog.ShowAlertAsync("Error al eliminar", $"Hubo un error al eliminar la nota. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task AgregarImagenAsync()
        {
            try
            {
                var miImagen = await _s.AddImagen.MostrarAsync();
                if (miImagen is null)
                {
                    return;
                }
                if (miImagen.Blob is null || miImagen.Blob.Length == 0)
                {
                    await _s.Dialog.ShowAlertAsync("Error", "La imagen no puede estar vacía.", "OK");
                    return;
                }
                miImagen.EntidadId = NotaId;
                miImagen.TipoDeEntidad = TipoEntidad.Nota;
                miImagen.CreadorId = usuario.Id;
                string imagenId = await _s.Db.InsertarImagensync(miImagen);
                if (!String.IsNullOrWhiteSpace(imagenId))
                {
                    miImagen.Id = imagenId;
                    Imagenes.Add(miImagen);
                }
                else
                {
                    await _s.Dialog.ShowAlertAsync("Error", "No se pudo guardar la imagen en la base de datos.", "OK");
                    return;
                }
                await CargarDatosAsync(NotaId);
            }
            catch (Exception ex)
            {
                await _s.Dialog.ShowAlertAsync("Error", $"No se pudo añadir la imagen.{Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task ImagenTocadaAsync(Foto? foto)
        {
            if (foto == null)
            {
                await _s.Dialog.ShowAlertAsync("Error", "No se pudo obtener la foto", "Aceptar");
                return;
            }
            try
            {
                if (foto.EsMapa)
                {
                    if (string.IsNullOrWhiteSpace(foto.UrlMapa))
                    {
                        await _s.Dialog.ShowAlertAsync("Error", "La foto no tiene una URL de mapa válida.", "Aceptar");
                        return;
                    }
                    await Launcher.OpenAsync(new Uri(foto.UrlMapa));
                }
                else
                {
                    byte[] blob = foto.Blob ?? [];
                    if (blob.Length == 0)
                    {
                        await _s.Dialog.ShowAlertAsync("Error", "La foto no tiene datos de imagen válidos.", "Aceptar");
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
                await _s.Dialog.ShowAlertAsync("Error", $"Hubo un error al gestionar la imagen: {ex.Message}", "Aceptar");
            }
        }

        public async Task HandleDroppedFilesAsync(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                try
                {
                    if (filePath != null)
                    {
                        if (!File.Exists(filePath))
                            return;

                        byte[] imagenBytes;
                        using var stream = File.OpenRead(filePath);
                        using var ms = new MemoryStream();
                        {
                            await stream.CopyToAsync(ms);
                            imagenBytes = ms.ToArray();
                        }
                        if (imagenBytes == null || imagenBytes.Length == 0)
                            return;


                        Foto miImagen = new();
                        miImagen.EsMapa = false;
                        miImagen.UrlMapa = "";
                        miImagen.Blob = imagenBytes;
                        miImagen.EntidadId = NotaId;
                        miImagen.TipoDeEntidad = TipoEntidad.Nota;
                        miImagen.CreadorId = usuario.Id;
                        string imagenId = await _s.Db.InsertarImagensync(miImagen);
                        if (String.IsNullOrWhiteSpace(imagenId))
                        {
                            await _s.Dialog.ShowAlertAsync("Error", "No se pudo guardar la imagen en la base de datos.", "OK");
                            return;
                        }
                        await CargarDatosAsync(NotaId);
                    }
                }
                catch (Exception ex)
                {
                    await _s.Dialog.ShowAlertAsync("Error", $"No se pudo guardar la imagen en la base de datos. {Environment.NewLine}{ex.Message}", "OK");
                }
            }
        }
    }
}
