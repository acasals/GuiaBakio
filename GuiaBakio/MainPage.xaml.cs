using GuiaBakio.Helpers;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;

namespace GuiaBakio
{
    public partial class MainPage : ContentPage
    {
        private ListaLocalidadesViewModel _myViewModel;
        public MainPage(ListaLocalidadesViewModel viewmodel)
        {
            InitializeComponent();
            _myViewModel = viewmodel;
            BindingContext = _myViewModel;
            AppForegroundNotifier.AppResumed += OnAppResumed;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                await _myViewModel.CargarListaLocalidadesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo obtener la lista de localidades.\n{ex.Message}", "OK");
            }
            await MisUtils.MostrarBotonAnimado(BtnAñadir);
        }

        private async void OnAppResumed()
        {
            try
            {
                await _myViewModel.CargarListaLocalidadesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo obtener la lista de localidades.\n{ex.Message}", "OK");
            }
            await MisUtils.MostrarBotonAnimado(BtnAñadir);
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
                    await DisplayAlert("Error", $"No se pudo navegar a la página de la localidad.{Environment.NewLine}{ex.Message}", "OK");
                }
            }
        }
    }
}

