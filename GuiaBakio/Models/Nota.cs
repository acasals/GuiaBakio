using SQLite;

namespace GuiaBakio.Models
{
    public class Nota
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int LocalidadId { get; set; }
        public string Titulo { get; set; }
        public string? Texto { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;
        public int CreadorId { get; set; }

        public Nota(string titulo, string texto, int localidadId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (localidadId <= 0)
                throw new ArgumentException("El ID de la localidad debe ser mayor que cero.", nameof(localidadId));
            Titulo = titulo;
            Texto = texto;
            LocalidadId = localidadId;
            FechaModificacion = DateTime.UtcNow;
        }

        public Nota()
        {
            Titulo = string.Empty;
        }

    }
}

