using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GuiaBakio.ViewModels
{
    public partial class ListaLocalidadesViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly IAddLocalidadPopupService _addLocalidadPopupService;
        private readonly IDialogService _dialogService;

        public IRelayCommand AddLocalidadAsyncCommand { get; }

        [ObservableProperty]
        private ObservableCollection<Localidad> listaLocalidades = [];
               
        public ListaLocalidadesViewModel(DataBaseService dbService,IAddLocalidadPopupService addLocalidadPopupService,IDialogService dialogService)
        {
            AddLocalidadAsyncCommand = new AsyncRelayCommand(AddLocalidadAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _addLocalidadPopupService =addLocalidadPopupService ?? throw new ArgumentNullException(nameof(_addLocalidadPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public async Task CargarListaLocalidadesAsync()
        {
            try
            {
                var lista = await _dbService.ObtenerLocalidadesAsync();
                ListaLocalidades = new ObservableCollection<Localidad>(lista);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar la lista de localidades.", ex);
            }
        }

        [RelayCommand]
        public async Task AddLocalidadAsync()
        {
            var nuevaLocalidad = await _addLocalidadPopupService.MostrarAsync();
            if (nuevaLocalidad is null)
            {
               return;
            }

            if (string.IsNullOrWhiteSpace(nuevaLocalidad))
            {
                await _dialogService.ShowAlertAsync("Error", "El nombre de la localidad no puede estar vacío.", "OK");
                return;
            }
            try
            {
                bool yaExiste = await _dbService.ExisteLocalidadAsync(nuevaLocalidad);
                if (yaExiste)
                    {
                    await _dialogService.ShowAlertAsync("Error", "Localidad existente.", "OK");
                    return;
                    }
                await _dbService.InsertarLocalidadAsync(nuevaLocalidad);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir la localidad.\n{ex.Message}", "OK");
            }
            
            await CargarListaLocalidadesAsync();
        }


    }
}
