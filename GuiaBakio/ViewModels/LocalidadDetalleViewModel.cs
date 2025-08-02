using GuiaBakio.Models;
using GuiaBakio.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace GuiaBakio.ViewModels
{
    public class LocalidadDetalleViewModel : INotifyPropertyChanged
    {
        private readonly DataBaseService _dbService;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalidadDetalleViewModel(int localidadId, DataBaseService dbService)
        {
            _dbService = dbService;
            LocalidadId = localidadId;
        }

        private int _localidadId;
        public int LocalidadId
        {
            get => _localidadId;
            set
            {
                if (_localidadId != value)
                {
                    _localidadId = value;
                    OnPropertyChanged(nameof(LocalidadId));
                }
            }
        }

        private Localidad? _localidad;
        public Localidad? Localidad
        {
            get => _localidad;
            set
            {
                if (_localidad != value)
                {
                    _localidad = value;
                    OnPropertyChanged(nameof(Localidad));
                    OnPropertyChanged(nameof(NoHayTexto));

                }
            }
        }

        private ObservableCollection<Apartado> _apartados = new();
        public ObservableCollection<Apartado> Apartados
        {
            get => _apartados;
            set
            {
                if (_apartados != value)
                {
                    _apartados = value;
                    OnPropertyChanged(nameof(Apartados));
                    OnPropertyChanged(nameof(NoHayApartados));
                }
            }
        }

        private ObservableCollection<ImagenLocalidad> _imagenes = new();
        public ObservableCollection<ImagenLocalidad> Imagenes
        {
            get => _imagenes;
            set
            {
                if (_imagenes != value)
                {
                    _imagenes = value;
                    OnPropertyChanged(nameof(Imagenes));
                    OnPropertyChanged(nameof(NoHayImagenes));
                }
            }
        }

        public bool NoHayTexto => string.IsNullOrWhiteSpace(Localidad?.Texto);
        public bool NoHayApartados => !Apartados?.Any() == true;
        public bool NoHayImagenes => !Imagenes?.Any() == true;
        public async Task CargarDatosAsync()
        {
            Localidad = await _dbService.ObtenerLocalidadAsync(LocalidadId);
            Apartados = new ObservableCollection<Apartado>(
                await _dbService.ObtenerApartadosAsync(LocalidadId));
            Imagenes = new ObservableCollection<ImagenLocalidad>(
                await _dbService.ObtenerImagenesPorLocalidadAsync(LocalidadId));
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand EditarTextoCommand => new Command(() =>
        {
            // Lógica para abrir editor de texto
            // Por ejemplo, navegar a una página de edición o mostrar un popup
        });

        public ICommand AgregarApartadoCommand => new Command(async () =>
        {
            //var nuevo = new Apartado { Titulo = "Nuevo apartado", LocalidadId = LocalidadId };
            //Apartados.Add(nuevo);
            //await _dbService.GuardarApartadoAsync(nuevo);
            OnPropertyChanged(nameof(Apartados));
        });

        public ICommand AgregarImagenCommand => new Command(async () =>
        {
            //var nueva = new ImagenLocalidad { Url = "https://ejemplo.com/imagen.jpg", LocalidadId = LocalidadId };
            //Imagenes.Add(nueva);
            //await _dbService.GuardarImagenAsync(nueva);
            OnPropertyChanged(nameof(Imagenes));
        });
    }
}
