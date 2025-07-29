using GuiaBakio.Models;
using GuiaBakio.Services;
using System.Diagnostics;

namespace GuiaBakio.Pages;
public partial class LocalidadPage : ContentPage, IQueryAttributable
{
    private int localidadId;
    private readonly DataBaseService? _dbService = App.Services.GetService<DataBaseService>();
    private Localidad? localidad;
    public LocalidadPage(string nombre)
    {
        InitializeComponent();
    }

    // Constructor por defecto (requerido por Shell)
    public LocalidadPage() : this("") { }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            if (!query.TryGetValue("Id", out object? value))
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            var IdString = value?.ToString();
            if (string.IsNullOrWhiteSpace(IdString))
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (!int.TryParse(IdString, out localidadId) || localidadId <= 0)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_dbService == null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            localidad = await _dbService.ObtenerLocalidadAsync(localidadId);

            if (localidad == null)
            {
                await Shell.Current.GoToAsync("..");
                return;
            }

            this.Title = localidad.Nombre;

            // Mostrar detalles de la localidad aquí...
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error al aplicar atributos de consulta: {ex.Message}");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnVolverButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }


}
