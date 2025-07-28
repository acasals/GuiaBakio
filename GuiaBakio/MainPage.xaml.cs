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
        private const string dbName = "GuiaBakio.db";
        private readonly string dbpath = Path.Combine(FileSystem.AppDataDirectory, dbName);
        private readonly DataBaseService _dbService;
        private MainViewModel _myViewModel;
        private readonly AddLocalidadPopup _addLocalidadPopup = new();    
        public MainPage()
        {
            InitializeComponent();
            _dbService = new DataBaseService(dbpath);
            _myViewModel = new MainViewModel(_dbService);
            BindingContext = _myViewModel;
            AppForegroundNotifier.AppResumed += OnAppResumed;
            _addLocalidadPopup.CityAdded += CityAddedAsync;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _dbService.InitTablesAsync();
            await CargarListaLocalidadesAsync();

        }

        private void OnAppResumed()
        {
            if (_dbService != null)
            {
                _ = CargarListaLocalidadesAsync();
            }
        }

        private async void BtnAñadir_Clicked(object sender, EventArgs e)
        {
            _addLocalidadPopup.MostrarPopup(this);
            await Task.Delay(200); // Esperar a que se cierre el popup
        }

        private async void CityAddedAsync(object? sender, string localidad)
        {
            if (string.IsNullOrWhiteSpace(localidad)) return;
            // Verificar si la localidad ya existe
            bool yaExiste = await _dbService.ExisteLocalidadAsync(localidad);
            if (yaExiste)
            {
                _ = DisplayAlert("Localidad existente", "La localidad ya está en la lista.", "OK");
                return;
            }
            try
            {
                await _dbService.InsertarLocalidadAsync(localidad);
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

