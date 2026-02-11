using CommunityToolkit.Maui.Views;
using GuiaBakio.Models;

namespace GuiaBakio.Popups;

public partial class NotaSeleccionPopup : Popup
{
    public NotaSeleccionPopup()
    {
        InitializeComponent();
    }

    public void CargarNotas(IEnumerable<Nota> notas)
    {
        NotasView.ItemsSource = notas;
    }

    public Nota? NotaSeleccionada =>
        NotasView.SelectedItem as Nota;

    public event EventHandler? Aceptado;
    public event EventHandler? Cancelado;

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        Aceptado?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        Cancelado?.Invoke(this, EventArgs.Empty);
    }
}