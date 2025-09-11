using SQLite;

namespace GuiaBakio.Models
{
    public class Foto
    {

        [PrimaryKey]
        public string Id { get; set; }
        public string EntidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Blob { get; set; }
        public bool EsMapa { get; set; }
        public string? UrlMapa { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;
        public TipoEntidad TipoDeEntidad { get; set; }
        public Foto(string entidadId, TipoEntidad tipoDeEntidad, byte[] byteArray, string usuarioId, string nombre = "", bool esmapa = false, string urlMapa = "")
        {
            if (byteArray == null)
                throw new ArgumentNullException(nameof(byteArray), "La imagen no puede ser nula.");
            if (string.IsNullOrWhiteSpace(entidadId))
                throw new ArgumentNullException(nameof(entidadId), "El Id de la localidad o nota no puede estar vacío.");
            Id = Guid.NewGuid().ToString();
            EntidadId = entidadId;
            Blob = byteArray;
            Nombre = nombre;
            EsMapa = esmapa;
            UrlMapa = urlMapa;
            CreadorId = usuarioId;
            TipoDeEntidad = tipoDeEntidad;
            FechaModificacion = DateTime.UtcNow;
        }
        public Foto()
        {
            Id = Guid.NewGuid().ToString();
            EntidadId = string.Empty;
            CreadorId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }

        [Ignore]
        public ImageSource? ImagenSource
        {
            get
            {
                if (Blob == null)
                    return null;

                return ImageSource.FromStream(() => new MemoryStream(Blob));
            }
        }
    }
}
