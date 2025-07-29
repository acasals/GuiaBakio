using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;
using GuiaBakio.Views;
using SQLite;
using System.Diagnostics;

namespace GuiaBakio
{

    public partial class MainPage : ContentPage
    {
        private readonly DataBaseService? _dbService;
        private MainViewModel _myViewModel;
        private readonly AddLocalidadPopup _addLocalidadPopup = new();    
        public MainPage()
        {
            InitializeComponent();
            _dbService =  App.Services.GetService<DataBaseService>();

            _myViewModel = new MainViewModel(_dbService);
            BindingContext = _myViewModel;
            AppForegroundNotifier.AppResumed += OnAppResumed;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_dbService != null)
            {
                await _dbService.InitTablesAsync();
                await CargarListaLocalidadesAsync();
            }
        }

        private void OnAppResumed()
        {
            if (_dbService != null)
            {
                _ = CargarListaLocalidadesAsync();
            }
        }

        private async void OnLocalidadSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            var localidadSeleccionada = e.CurrentSelection.FirstOrDefault() as Localidad;
           
            if (localidadSeleccionada != null)
            {
                try
                {
                    await Shell.Current.GoToAsync($"localidadPage?Id={localidadSeleccionada.Id}");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo navegar a la página de la localidad.\n{ex.Message}", "OK");
                }
            }
        }

        private async void BtnAñadir_Clicked(object sender, EventArgs e)
        {
            string? nuevaLocalidad = await _addLocalidadPopup.MostrarYEsperarAsync(this);

            if (string.IsNullOrEmpty(nuevaLocalidad))
            {
                return;
            }

            if (_dbService == null)
            {
                return;
            }

            bool yaExiste = await _dbService.ExisteLocalidadAsync(nuevaLocalidad);
            if (yaExiste)
            {
                _ = DisplayAlert("Localidad existente", "La localidad ya está en la lista.", "OK");
                return;
            }
            try
            {
                await _dbService.InsertarLocalidadAsync(nuevaLocalidad);
                await CargarListaLocalidadesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo añadir la localidad.\n{ex.Message}", "OK");
            }

        }

        private async Task CargarListaLocalidadesAsync()
        {
            try
            {
                await _myViewModel.VistaLocalidades!.ActualizarVistaLocalidadesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo obtener la lista de localidades.\n{ex.Message}", "OK");
            }
        }

    }

}

