using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GuiaBakio.ViewModels
{
    public partial class LocalidadDetalleViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly ITextEditorPopupService _textEditorPopupService;
        private readonly IDialogOKService _dialogService;
        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarApartadoAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public IRelayCommand EliminarLocalidadAsyncCommand { get; }
        public LocalidadDetalleViewModel(DataBaseService dbService, ITextEditorPopupService textEditorPopupService, IDialogOKService dialogService )
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            AgregarApartadoAsyncCommand = new AsyncRelayCommand(AgregarApartadoAsync);
            AgregarImagenAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            EliminarLocalidadAsyncCommand = new AsyncRelayCommand(EliminarLocalidadAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
        }

        [ObservableProperty]
        private int localidadId;

        [ObservableProperty] 
        private Localidad? localidad;
       

        [ObservableProperty] 
        private ObservableCollection<Apartado> apartados = new();
        
        [ObservableProperty] 
        private ObservableCollection<ImagenLocalidad> imagenes = new();

        [ObservableProperty]
        private bool noHayTexto;
        [ObservableProperty] 
        private bool noHayApartados;
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
                Apartados = new ObservableCollection<Apartado>(
                    await _dbService.ObtenerApartadosAsync(LocalidadId));
                Imagenes = new ObservableCollection<ImagenLocalidad>(
                    await _dbService.ObtenerImagenesPorLocalidadAsync(LocalidadId));
                NoHayTexto = string.IsNullOrWhiteSpace(Localidad?.Texto);
                NoHayApartados = !Apartados?.Any() == true;
                NoHayImagenes = !Imagenes?.Any() == true;
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
                int eliminado = await _dbService.EliminarLocalidadAsync(LocalidadId);
                if (eliminado>0)
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
                await _dialogService.ShowAlertAsync("Error al actualizar.", $"Hubo un error al actualziar la localidad. El texto no fue guardado. {Environment.NewLine}{ex.Message}", "OK");
                return;
            }
        }

        [RelayCommand]
        public async Task AgregarApartadoAsync() 
        {
            //var nuevo = new Apartado { Titulo = "Nuevo apartado", LocalidadId = LocalidadId };
            //Apartados.Add(nuevo);
            //await _dbService.GuardarApartadoAsync(nuevo);
            //OnPropertyChanged(nameof(Apartados));
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
