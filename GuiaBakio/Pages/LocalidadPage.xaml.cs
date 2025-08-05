using GuiaBakio.Services;
using GuiaBakio.ViewModels;
using System.Diagnostics;


namespace GuiaBakio.Pages;
public partial class LocalidadPage : ContentPage, IQueryAttributable
{
    private readonly LocalidadDetalleViewModel _myViewModel;
    private int localidadId;

    public LocalidadPage(LocalidadDetalleViewModel viewModel)
    {
        InitializeComponent();
        _myViewModel = viewModel;
        AppForegroundNotifier.AppResumed += OnAppResumed;
    }
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var value) && int.TryParse(value?.ToString(), out int id))
        {
            localidadId = id;
            BindingContext = _myViewModel;
            await _myViewModel.CargarDatosAsync(id);
        }
        else
        {
            Debug.WriteLine("No se pudo obtener el servicio DataBaseService.");
            await Shell.Current.GoToAsync("..");
        }
     }
 
    private async void OnAppResumed()
    {
       await _myViewModel.CargarDatosAsync(localidadId);
    }

    private async void OnVolverButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
 
}
