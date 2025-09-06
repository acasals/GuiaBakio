using SQLite;

namespace GuiaBakio.Models
{
    public class Localidad
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string? Texto { get; set; }

        public int CreadorId { get; set; }

        public DateTime FechaModificacion { get; set; }

        public bool Sincronizado { get; set; } = false;

        public Localidad(string localidad, int usuarioId, string texto = "")
        {
            if (string.IsNullOrWhiteSpace(localidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(localidad));
            if (usuarioId <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor que cero.", nameof(usuarioId));
            Nombre = localidad;
            CreadorId = usuarioId;
            Texto = texto;
            FechaModificacion = DateTime.UtcNow;
        }
        public Localidad()
        {
            Nombre = string.Empty;
        }
    }
}
