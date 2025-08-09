using SQLite;

namespace GuiaBakio.Models
{
    public class ImagenLocalidad
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int LocalidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public bool? EsMapa { get; set; }
        public string? UrlMapa { get; set; }
        public ImagenLocalidad(int localidadId, byte[] byteArray, string nombre,bool esMapa,string urlMapa)
        {
            if (localidadId <= 0)
                throw new ArgumentException("El ID de la localidad debe ser mayor que cero.", nameof(localidadId));
            LocalidadId = localidadId;
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            Foto = byteArray;
            Nombre = nombre;
            EsMapa = esMapa;
            UrlMapa = urlMapa;
        }
        public ImagenLocalidad() { }    
    }
}