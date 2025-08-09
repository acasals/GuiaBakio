using SQLite;

namespace GuiaBakio.Models
{
    public class ImagenApartado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ApartadoId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public bool EsMapa { get; set; }
        public string? UrlMapa {  get; set; }


        public ImagenApartado(int apartadoId, byte[] byteArray, string nombre, bool esMapa, string urlMapa)
        {
            if (apartadoId <= 0)
                throw new ArgumentException("El ID de la apartado debe ser mayor que cero.", nameof(apartadoId));
            ApartadoId = apartadoId;
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            Foto = byteArray;
            Nombre = nombre;
            EsMapa = esMapa;
            UrlMapa = urlMapa;
        }
        public ImagenApartado() { }
    }
}
