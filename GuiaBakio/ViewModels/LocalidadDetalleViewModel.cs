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
        public IRelayCommand EditarTextoAsyncCommand { get; }
        public IRelayCommand AgregarApartadoAsyncCommand { get; }
        public IRelayCommand AgregarImagenAsyncCommand { get; }
        public LocalidadDetalleViewModel(DataBaseService dbService, ITextEditorPopupService textEditorPopupService)
        {
            EditarTextoAsyncCommand = new AsyncRelayCommand(EditarTextoAsync);
            AgregarApartadoAsyncCommand = new AsyncRelayCommand(AgregarApartadoAsync);
            AgregarApartadoAsyncCommand = new AsyncRelayCommand(AgregarImagenAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _textEditorPopupService = textEditorPopupService ?? throw new ArgumentNullException(nameof(textEditorPopupService));
        }

        [ObservableProperty]
        private int localidadId;

        [ObservableProperty] 
        private Localidad? localidad;
       

        [ObservableProperty] 
        private ObservableCollection<Apartado> apartados = new();
        
        [ObservableProperty] 
        private ObservableCollection<ImagenLocalidad> imagenes = new();
       
        public bool NoHayTexto => string.IsNullOrWhiteSpace(Localidad?.Texto);
        public bool NoHayApartados => !Apartados?.Any() == true;
        public bool NoHayImagenes => !Imagenes?.Any() == true;
        public async Task CargarDatosAsync(int localidadId)
        {
            if (localidadId <= 0)
            {
                throw new ArgumentNullException(nameof(localidadId), "La Id de localidad debe ser mayor que 0.");
            }
            LocalidadId = localidadId;
            Localidad = await _dbService.ObtenerLocalidadAsync(LocalidadId);
            Apartados = new ObservableCollection<Apartado>(
                await _dbService.ObtenerApartadosAsync(LocalidadId));
            Imagenes = new ObservableCollection<ImagenLocalidad>(
                await _dbService.ObtenerImagenesPorLocalidadAsync(LocalidadId));
        }
     
        [RelayCommand]
        public async Task EditarTextoAsync()
        {
            var resultado = await _textEditorPopupService.MostrarEditorAsync("Texto inicial");

            if (resultado != null)
            {
                // Procesar el texto editado
            }
        }

        [RelayCommand]
        public async Task AgregarApartadoAsync() 
        {
            //var nuevo = new Apartado { Titulo = "Nuevo apartado", LocalidadId = LocalidadId };
            //Apartados.Add(nuevo);
            //await _dbService.GuardarApartadoAsync(nuevo);
            OnPropertyChanged(nameof(Apartados));
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
