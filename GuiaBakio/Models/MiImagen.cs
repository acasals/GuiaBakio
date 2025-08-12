using SQLite;

namespace GuiaBakio.Models
{
    public class MiImagen
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int EntidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public bool EsMapa { get; set; } 
        public string? UrlMapa { get; set; }
        public TipoEntidad TipoDeEntidad { get; set; }
        public MiImagen( int entidadId, TipoEntidad tipoDeEntidad, byte[] byteArray, string nombre = "", bool esmapa = false, string urlMapa= "")
        {
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            if (entidadId <= 0)
                throw new ArgumentException("El Id de la localidad, apartado o nota debe ser mayor que cero.", nameof(entidadId));
            Foto = byteArray;
            Nombre = nombre;
            EsMapa = esmapa;
            UrlMapa = urlMapa;
            TipoDeEntidad = tipoDeEntidad;
        }
        public MiImagen() { }

        [Ignore]
        public ImageSource? ImagenSource { get; set; }
    }
}
