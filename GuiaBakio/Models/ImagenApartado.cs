using SQLite;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GuiaBakio.Models
{
    public class ImagenApartado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int ApartadoId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public bool EsMapa { get; set; } = false;

        public ImagenApartado(int apartadoId, byte[] byteArray, string nombre = "",bool esmapa=false)
        {
            if (apartadoId <= 0)
                throw new ArgumentException("El ID de la apartado debe ser mayor que cero.", nameof(apartadoId));
            ApartadoId = apartadoId;
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            Foto = byteArray;
            Nombre = nombre;
            EsMapa = esmapa;
        }
        public ImagenApartado() { }
    }
}
