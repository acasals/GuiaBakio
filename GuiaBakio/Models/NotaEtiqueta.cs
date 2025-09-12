using SQLite;

namespace GuiaBakio.Models
{
    public class NotaEtiqueta
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string NotaId { get; set; }
        public string EtiquetaId { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;

        public NotaEtiqueta()
        {
            Id = Guid.NewGuid().ToString();
            NotaId = string.Empty;
            EtiquetaId = string.Empty;
            CreadorId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }
        public NotaEtiqueta(string notaId, string etiquetaId, string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
                throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(etiquetaId))
                throw new ArgumentNullException(nameof(etiquetaId), "El Id de la etiqueta no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");

            Id = Guid.NewGuid().ToString();
            NotaId = notaId;
            EtiquetaId = etiquetaId;
            CreadorId = usuarioId;
            FechaModificacion = DateTime.UtcNow;
        }

    }
}
