using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using GuiaBakio.ViewModels;
#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif

namespace GuiaBakio.Pages;

public partial class NotaPage : ContentPage, IQueryAttributable
{
    private readonly NotaViewModel _myViewModel;
    private readonly IDialogOKService _dialogService;

    private string notaId;
    public NotaPage(NotaViewModel viewModel, IDialogOKService dialogService)
    {
        InitializeComponent();
        _myViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel)); ;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
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
        }
#endif
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        string? id = query["Id"]?.ToString();

        if (!string.IsNullOrWhiteSpace(id))
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