using SQLite;

namespace GuiaBakio.Models
{
    public class Nota
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string LocalidadId { get; set; }
        public string Titulo { get; set; }
        public string? Texto { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;
        public string CreadorId { get; set; }

        public Nota(string titulo, string texto, string localidadId, string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentNullException(nameof(titulo), "El Id de la nota no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la nota no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id de la nota no puede estar vacío.");
            Id = Guid.NewGuid().ToString();
            Titulo = titulo;
            Texto = texto;
            LocalidadId = localidadId;
            CreadorId = usuarioId;
            FechaModificacion = DateTime.UtcNow;
        }

        public Nota()
        {
            Id = Guid.NewGuid().ToString();
            Titulo = string.Empty;
            LocalidadId = string.Empty;
            CreadorId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }

    }
}

