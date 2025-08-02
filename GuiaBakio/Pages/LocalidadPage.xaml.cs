using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.ViewModels;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;


namespace GuiaBakio.Pages;
public partial class LocalidadPage : ContentPage, IQueryAttributable
{
    private readonly DataBaseService? _dbService = App.Services.GetService<DataBaseService>();
    private LocalidadDetalleViewModel? _myViewModel;
    
    public LocalidadPage(string nombre)
    {
        InitializeComponent();
        
        AppForegroundNotifier.AppResumed += OnAppResumed;
    }

    // Constructor por defecto (requerido por Shell)
    public LocalidadPage() : this("") {}

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Id", out var value) && int.TryParse(value?.ToString(), out int id))
        {
            if (_dbService != null)
            {
                _myViewModel = new LocalidadDetalleViewModel(id, _dbService);
                BindingContext = _myViewModel;
                await _myViewModel.CargarDatosAsync();
            }
            else
            {
                Debug.WriteLine("No se pudo obtener el servicio DataBaseService.");
                await Shell.Current.GoToAsync("..");
            }
        }
        else
        {
            Debug.WriteLine("No se pudo obtener el Id de la localidad desde los parámetros de la consulta.");
            await Shell.Current.GoToAsync("..");
        }
     }
 
    private void OnAppResumed()
    {
        if (_dbService != null)
        {

            _ = _myViewModel?.CargarDatosAsync();
        }
    }

    private async void OnVolverButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }


}
