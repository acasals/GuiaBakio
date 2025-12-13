using GuiaBakio.Helpers;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;

namespace GuiaBakio
{
    public partial class MainPage : ContentPage
    {
        private ListaNotasViewModel _myViewModel;

        public MainPage(
            ListaNotasViewModel viewmodel)
        {
            InitializeComponent();
            _myViewModel = viewmodel;
            BindingContext = _myViewModel;
            AppForegroundNotifier.AppResumed += OnAppResumed;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _myViewModel.OnAppearedOrResumed();
            await MisUtils.MostrarBotonAnimado(BtnAñadirNota);
        }

        private async void OnAppResumed()
        {
            await _myViewModel.OnAppearedOrResumed();
            await MisUtils.MostrarBotonAnimado(BtnAñadirNota);
        }

        private async void OnNotaSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Nota notaSeleccionada)
            {
                try
                {
                    await Shell.Current.GoToAsync($"notaPage?Id={notaSeleccionada.Id}");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo navegar a la página de la nota.{Environment.NewLine}{ex.Message}", "OK");
                }
            }
        }
    }
}

