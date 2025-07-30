namespace GuiaBakio.Models
{
    using SQLite;

    public class Localidad
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Nombre { get; set; }

        public string? Texto { get; set; }

        public DateTime FechaModificacion { get; set; }

        public bool Sincronizado { get; set; } = false;

        public Localidad(string localidad)
        {
            if (string.IsNullOrWhiteSpace(localidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(localidad));
            Nombre = localidad;
            FechaModificacion = DateTime.UtcNow;
        }
        public Localidad()
        {
            Nombre = string.Empty;
        }
    }
}
