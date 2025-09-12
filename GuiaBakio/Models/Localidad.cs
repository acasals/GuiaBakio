using SQLite;

namespace GuiaBakio.Models
{
    public class Localidad
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Nombre { get; set; }
        public string? Texto { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;



        public Localidad(string localidad, string usuarioId, string texto = "")
        {
            if (string.IsNullOrWhiteSpace(localidad))
                throw new ArgumentNullException(nameof(localidad), "El Id de la localidad no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");
            Id = Guid.NewGuid().ToString();
            Nombre = localidad;
            CreadorId = usuarioId;
            Texto = texto;
            FechaModificacion = DateTime.UtcNow;
        }
        public Localidad()
        {
            Id = Guid.NewGuid().ToString();
            Nombre = string.Empty;
            CreadorId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }
    }
}
