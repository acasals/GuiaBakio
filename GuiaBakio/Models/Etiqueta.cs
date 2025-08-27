using SQLite;
namespace GuiaBakio.Models
{
    public class Etiqueta
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

        [Ignore]
        public bool IsSelected { get; set; }
    }
}
