using CommunityToolkit.Maui.Views;

namespace GuiaBakio.Popups;

public partial class TextEditorPopup : Popup
{
    public TextEditorPopup()
    {
        InitializeComponent();
    }

    public string Texto
    {
        get => EditorTexto.Text ?? "";
        set => EditorTexto.Text = value;
    }

    public event EventHandler? GuardarSolicitado;
    public event EventHandler? CancelarSolicitado;

    private void OnGuardarClicked(object sender, EventArgs e)
    {
        GuardarSolicitado?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        CancelarSolicitado?.Invoke(this, EventArgs.Empty);
    }
}