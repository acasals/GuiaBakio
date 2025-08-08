using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using GuiaBakio.ViewModels;

namespace GuiaBakio.Pages;

public partial class NotaPage : ContentPage, IQueryAttributable
{
    private readonly NotaViewModel _myViewModel;
    private readonly IDialogOKService _dialogService;

    private int notaId;
    public NotaPage(NotaViewModel viewModel, IDialogOKService dialogService)
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
            notaId = id;
            try
            {
                BindingContext = _myViewModel;
                await _myViewModel.CargarDatosAsync(id);
            }
            catch
            {
                await _dialogService.ShowAlertAsync("Error al cargar datos de la nota", $"Hubo un error al obtener los datos de la nota con Id: ;{Id}", "OK");
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
        await _myViewModel.CargarDatosAsync(notaId);
    }

    private async void OnVolverButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

}