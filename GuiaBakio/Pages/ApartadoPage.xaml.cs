using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using GuiaBakio.ViewModels;

namespace GuiaBakio.Pages;
public partial class ApartadoPage : ContentPage, IQueryAttributable
{
    private readonly ApartadoViewModel _myViewModel;
    private readonly IDialogOKService _dialogService;

    private int apartadoId;
    public ApartadoPage(ApartadoViewModel viewModel, IDialogOKService dialogService)
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
            apartadoId = id;
            try
            {
                BindingContext = _myViewModel;
                await _myViewModel.CargarDatosAsync(id);
            }
            catch
            {
                await _dialogService.ShowAlertAsync("Error al cargar datos del apartado", $"Hubo un error al obtener los datos del apartado con Id: ;{Id}", "OK");
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
        await _myViewModel.CargarDatosAsync(apartadoId);
    }

    private async void OnVolverButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
