using GuiaBakio.Models;
using GuiaBakio.Services;

namespace GuiaBakio.ViewModels
{
    internal class MainViewModel
    {
        public LocalidadesViewModel VistaLocalidades { get; set; }
      
        public MainViewModel(DataBaseService _dbService)
        {
            VistaLocalidades = new LocalidadesViewModel(_dbService);
        }
    }
}
