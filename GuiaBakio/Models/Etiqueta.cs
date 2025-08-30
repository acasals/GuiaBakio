using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
namespace GuiaBakio.Models
{
    public partial class Etiqueta : ObservableObject
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public string? Icono { get; set; }
        public int CreadorId { get; set; }
        public Etiqueta(string nombre, string? icono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la etiqueta es obligatorio.", nameof(nombre));
            Nombre = nombre;
            Icono = icono;
        }
        public Etiqueta()
        {
            Nombre = string.Empty;
        }

        [ObservableProperty]
        private bool isSelected;
    }
}
