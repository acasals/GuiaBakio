using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaBakio.Models
{
    public class ImagenNota
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int LocalidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public ImagenNota(int localidadId, byte[] byteArray, string nombre="")
        {
            if (localidadId <= 0)
                throw new ArgumentException("El ID de la localidad debe ser mayor que cero.", nameof(localidadId));
            LocalidadId = localidadId;
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            Foto = byteArray;
            Nombre = nombre;
        }
        public ImagenNota() { }
    }
}
