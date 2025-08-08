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
        private readonly IAddItemPopupService _addItemPopupService;
        private readonly IDialogOKService _dialogService;

        public IRelayCommand AddLocalidadAsyncCommand { get; }

        [ObservableProperty]
        private ObservableCollection<Localidad> listaLocalidades = [];
               
        public ListaLocalidadesViewModel(DataBaseService dbService,IAddItemPopupService addItemPopupService,IDialogOKService dialogService)
        {
            AddLocalidadAsyncCommand = new AsyncRelayCommand(AddLocalidadAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _addItemPopupService =addItemPopupService ?? throw new ArgumentNullException(nameof(_addItemPopupService));
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
            var nuevaLocalidad = await _addItemPopupService.MostrarAsync("Añade una localidad");
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
                var id= await _dbService.InsertarLocalidadAsync(nuevaLocalidad);
                if (id <= 0)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo añadir la localidad.", "OK");
                    return;
                }
                try
                {
                    await Shell.Current.GoToAsync($"localidadPage?Id={id}");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Error", $"No se pudo navegar a la página de la localidad.{Environment.NewLine}{ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir la localidad.{Environment.NewLine}{ex.Message}", "OK");
            }
            
            await CargarListaLocalidadesAsync();
        }


    }
}
