using CommunityToolkit.Maui.Views;

namespace GuiaBakio.Popups;

public partial class AddItemPopup : Popup
{
    public AddItemPopup()
    {
        InitializeComponent();
    }

    public Entry EntryControl => EntryTexto;

    public string Texto => EntryTexto.Text;

    public event EventHandler? Aceptado;

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        Aceptado?.Invoke(this, EventArgs.Empty);
    }
}

