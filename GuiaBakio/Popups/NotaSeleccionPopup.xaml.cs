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

    private Border? _ultimoSeleccionado;

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is Border border && border.BindingContext is Nota nota)
        {
            NotasView.SelectedItem = nota;

            // 2. Restaurar el color del anterior seleccionado
            if (_ultimoSeleccionado != null)
            {
                _ultimoSeleccionado.BackgroundColor = Color.FromArgb("#fafafa");
                _ultimoSeleccionado.Stroke = Color.FromArgb("#dddddd");
            }

            // 3. Aplicar color de selección al nuevo
            border.BackgroundColor = Color.FromArgb("#cce5ff"); // azul suave
            border.Stroke = Color.FromArgb("#3399ff");

            // 4. Guardar referencia
            _ultimoSeleccionado = border;


        }
    }

    private void OnAceptarClicked(object sender, EventArgs e)
    {
        Aceptado?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        Cancelado?.Invoke(this, EventArgs.Empty);
    }
}