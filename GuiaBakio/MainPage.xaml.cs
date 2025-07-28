using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;
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
        public MainPage()
        {
            InitializeComponent();
            _dbService = new DataBaseService(dbpath);
            _myViewModel = new MainViewModel(_dbService);
            BindingContext = _myViewModel;
            AppForegroundNotifier.AppResumed += OnAppResumed;
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

