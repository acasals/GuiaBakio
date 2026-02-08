using GuiaBakio.Helpers;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;
#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif


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

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

#if WINDOWS
            var nativeView = this.Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement;
            if (nativeView != null)
            {
                nativeView.AllowDrop = true;
                nativeView.DragOver += OnDragOver;
                nativeView.Drop += OnDrop;
                System.Diagnostics.Debug.WriteLine("Drop y DragOver registrados en MainPage.");
            }
#endif
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

#if WINDOWS
        private async void OnDrop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            if (!e.DataView.Contains(StandardDataFormats.StorageItems))
                return;
                

            var items = await e.DataView.GetStorageItemsAsync();
            var files = items
                .OfType<StorageFile>()
                .Select(f => f.Path)
                .ToList();

            if (files.Count == 0)
                return;

            await _myViewModel.HandleDroppedFilesAsync(files);
        }

        private void OnDragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
        }
#endif

    }
}

