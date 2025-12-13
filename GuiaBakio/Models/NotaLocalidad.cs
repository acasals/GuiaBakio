using SQLite;

namespace GuiaBakio.Models
{
    public class NotaLocalidad
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string NotaId { get; set; }
        public string LocalidadId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;

        public NotaLocalidad()
        {
            Id = Guid.NewGuid().ToString();
            NotaId = string.Empty;
            LocalidadId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }
        public NotaLocalidad(string notaId, string localidadId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
                throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad no puede estar vacío.");

            Id = Guid.NewGuid().ToString();
            NotaId = notaId;
            LocalidadId = localidadId;
            FechaModificacion = DateTime.UtcNow;
        }

    }
}
