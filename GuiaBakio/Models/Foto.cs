using SQLite;

namespace GuiaBakio.Models
{
    public class Foto
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int EntidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Blob { get; set; }
        public bool EsMapa { get; set; }
        public string? UrlMapa { get; set; }
        public int CreadorId { get; set; }

        public TipoEntidad TipoDeEntidad { get; set; }
        public Foto(int entidadId, TipoEntidad tipoDeEntidad, byte[] byteArray, string nombre = "", bool esmapa = false, string urlMapa = "")
        {
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            if (entidadId <= 0)
                throw new ArgumentException("El Id de la localidad o nota debe ser mayor que cero.", nameof(entidadId));
            Blob = byteArray;
            Nombre = nombre;
            EsMapa = esmapa;
            UrlMapa = urlMapa;
            TipoDeEntidad = tipoDeEntidad;
        }
        public Foto() { }

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
