using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;
using GuiaBakio.Views;

namespace GuiaBakio
{
    public partial class MainPage : ContentPage
    {
        private readonly DataBaseService? _dbService;
        private ListaLocalidadesViewModel _myViewModel;
        private readonly AddLocalidadPopup _addLocalidadPopup = new();    
        public MainPage()
        {
            InitializeComponent();
            _dbService =  App.Services.GetService<DataBaseService>();

            _myViewModel = new ListaLocalidadesViewModel(_dbService);
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

            {
                try
                {
                    bool añadido = await _myViewModel.AñadirLocalidadAsync(nuevaLocalidad);
                    if (!añadido)
                    {
                        await DisplayAlert("Localidad existente", "La localidad ya está en la lista o no es válida.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo añadir la localidad.\n{ex.Message}", "OK");
                }
            }
        }

        private async Task CargarListaLocalidadesAsync()
        {
                            try
                            {
                                await _myViewModel.ActualizarVistaLocalidadesAsync();
                            }
                            catch (Exception ex)
                            {
                                await DisplayAlert("Error", $"No se pudo obtener la lista de localidades.\n{ex.Message}", "OK");
                            }
         }
    }
}

