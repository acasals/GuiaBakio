using GuiaBakio.Models;
using GuiaBakio.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GuiaBakio.ViewModels
{
    internal class LocalidadesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<Localidad> _ListaLocalidades = [];
        public ObservableCollection<Localidad> ListaLocalidades
        {
            get => _ListaLocalidades;
            set
            {
                _ListaLocalidades = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ListaLocalidades)));
            }
        }
        private readonly DataBaseService _dbService;
        public LocalidadesViewModel(DataBaseService? dbService)
        {
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        public async Task ActualizarVistaLocalidadesAsync()
        {
            var lista = await _dbService.ObtenerLocalidadesAsync();
            ListaLocalidades = new ObservableCollection<Localidad>(lista);
        }
    }
}
