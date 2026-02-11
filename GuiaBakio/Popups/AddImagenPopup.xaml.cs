using CommunityToolkit.Maui.Views;
using GuiaBakio.Models;

namespace GuiaBakio.Popups;

public partial class AddImagenPopup : Popup
{
    public AddImagenPopup()
    {
        InitializeComponent();
    }

    public string UrlTexto => EntryUrl.Text?.Trim() ?? "";

    public event Func<string, Task>? CargarUrlSolicitado;
    public event Func<Task>? SeleccionarArchivoSolicitado;
    public event EventHandler? GuardarSolicitado;

    public void MostrarPreview(byte[] bytes)
    {
        PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        PreviewImage.IsVisible = true;
        BtnGuardar.IsEnabled = true;
    }

    private async void OnCargarUrlClicked(object sender, EventArgs e)
    {
        if (CargarUrlSolicitado != null)
            await CargarUrlSolicitado.Invoke(UrlTexto);
    }

    private async void OnSeleccionarArchivoClicked(object sender, EventArgs e)
    {
        if (SeleccionarArchivoSolicitado != null)
            await SeleccionarArchivoSolicitado.Invoke();
    }

    private void OnGuardarClicked(object sender, EventArgs e)
    {
        GuardarSolicitado?.Invoke(this, EventArgs.Empty);
    }
}