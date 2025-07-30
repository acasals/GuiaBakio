using SQLite;

namespace GuiaBakio.Models
{
    public class ImagenApartado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int LocalidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public ImagenApartado(int localidadId)
        {
            if (localidadId <= 0)
                throw new ArgumentException("El ID de la localidad debe ser mayor que cero.", nameof(localidadId));
            LocalidadId = localidadId;
        }
        public ImagenApartado() { }
    }
}
