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
        public int NotaId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public bool EsMapa { get; set; } = false;
        public ImagenNota(int notaId, byte[] byteArray, string nombre="", bool esmapa = false)
        {
            if (NotaId <= 0)
                throw new ArgumentException("El ID de la notadebe ser mayor que cero.", nameof(notaId));
            NotaId =notaId;
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            Foto = byteArray;
            Nombre = nombre;
            EsMapa = esmapa;
        }
        public ImagenNota() { }
    }
}
