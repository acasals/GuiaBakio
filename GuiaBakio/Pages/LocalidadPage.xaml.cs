using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using GuiaBakio.ViewModels;
using System.Diagnostics;


namespace GuiaBakio.Pages;
public partial class LocalidadPage : ContentPage, IQueryAttributable
{
    private readonly LocalidadDetalleViewModel _myViewModel;
    private readonly IDialogOKService _dialogService;

    private int localidadId;

    public LocalidadPage(LocalidadDetalleViewModel viewModel,IDialogOKService dialogService)
    {
        InitializeComponent();
        _myViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel)); ;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
        AppForegroundNotifier.AppResumed += OnAppResumed;
    }
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var value) && int.TryParse(value?.ToString(), out int id))
        {
            localidadId = id;
            try
            {
                BindingContext = _myViewModel;
                await _myViewModel.CargarDatosAsync(id);
            }
            catch
            {
                await _dialogService.ShowAlertAsync("Error al cargar datos de localidad", $"Hubo un error al obtener los datos de la localidad con Id: ;{Id}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        else
        {
            await Shell.Current.GoToAsync("..");
        }
     }
 
    private async void OnAppResumed()
    {
       await _myViewModel.CargarDatosAsync(localidadId);
    }

    private async void OnApartadoSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        var apartadoSeleccionado = e.CurrentSelection.FirstOrDefault() as Apartado;

        if (apartadoSeleccionado != null)
        {
            try
            {
                await Shell.Current.GoToAsync($"apartadoPage?Id={apartadoSeleccionado.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo navegar a la página del apartado.{Environment.NewLine}{ex.Message}", "OK");
            }
        }
    }
    private async void OnVolverButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
 
}

