using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GuiaBakio.ViewModels
{
    public partial class NotaViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly ITextEditorPopupService _textEditorPopupService;
        private readonly IAddImagenPopupService _addImagenPopupService;
        private readonly IDialogOKService _dialogService;
        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarNotaAsyncCommand { get; }

        public NotaViewModel(DataBaseService dbService, ITextEditorPopupService textEditorPopupService, IAddImagenPopupService addImagenPopupService, IDialogOKService dialogService)
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarNotaAsyncCommand = new AsyncRelayCommand(EliminarNotaAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
            _addImagenPopupService = addImagenPopupService ?? throw new ArgumentNullException(nameof(addImagenPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
        }

        [ObservableProperty]
        private string titulo;

        [ObservableProperty]
        private int notaId;

        [ObservableProperty]
        private Nota? nota;

        [ObservableProperty]
        private ObservableCollection<MiImagen> imagenes = new();

        [ObservableProperty]
        private bool noHayTexto;
        [ObservableProperty]
        private bool noHayImagenes;

        public async Task CargarDatosAsync(int notaId)
        {
            if (notaId <= 0)
            {
                throw new ArgumentNullException(nameof(notaId), "La Id de la nota debe ser mayor que 0.");
            }
            NotaId = notaId;
            try
            {
                Nota = await _dbService.ObtenerNotaAsync(notaId);
                var apartado = await _dbService.ObtenerApartadoAsync(Nota.ApartadoId);
                var localidad = await _dbService.ObtenerLocalidadAsync(apartado.LocalidadId);
                Titulo = localidad?.Nombre + " - " + apartado.Nombre + " - " + Nota.Titulo;
                Imagenes = new ObservableCollection<MiImagen>(
                    await _dbService.ObtenerImagenesPorEntidadAsync(TipoEntidad.Nota, notaId));
                await AsignarImagenSourceAsync();
                NoHayTexto = string.IsNullOrWhiteSpace(Nota.Contenido);
                NoHayImagenes = !Imagenes?.Any() == true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al cargar datos de la nota. {ex.Message}");
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
        public async Task EditarTextoAsync()
        {
            try
            {
                var resultado = await _textEditorPopupService.MostrarEditorAsync(Nota?.Contenido);

                if (resultado is null)
                {
                    return;
                }
                string? v = Nota?.Contenido = resultado;
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
            //var nueva = new ImagenLocalidad { Url = "https://ejemplo.com/imagen.jpg", LocalidadId = LocalidadId };
            //Imagenes.Add(nueva);
            //await _dbService.GuardarImagenAsync(nueva);
            OnPropertyChanged(nameof(Imagenes));
        }
    }
}
