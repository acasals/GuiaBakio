using GuiaBakio.Models;
using GuiaBakio.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GuiaBakio.ViewModels
{
    public class LocalidadDetalleViewModel(int localidadId, DataBaseService dbService) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public int LocalidadId { get; set; } = localidadId;
        public Localidad? LocalidadActual { get; set; }
        public ObservableCollection<Apartado> Apartados { get; set; } = new ObservableCollection<Apartado>();
        public ObservableCollection<ImagenLocalidad> Imagenes { get; set; } = new ObservableCollection<ImagenLocalidad>();

        public async Task CargarDatosAsync()
        {
            LocalidadActual = await dbService.ObtenerLocalidadAsync(LocalidadId);
            Apartados = new ObservableCollection<Apartado>(
                await dbService.ObtenerApartadosAsync(LocalidadId));
            Imagenes = new ObservableCollection<ImagenLocalidad>(
                await dbService.ObtenerImagenesPorLocalidadAsync(LocalidadId));
        }
    }
}
