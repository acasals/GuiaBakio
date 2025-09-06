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

        public Nota(string titulo, string texto, int localidadId, int usuarioId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (localidadId <= 0)
                throw new ArgumentException("El ID de la localidad debe ser mayor que cero.", nameof(localidadId));
            if (usuarioId <= 0)
                throw new ArgumentException("El ID del usuario debe ser mayor que cero.", nameof(usuarioId));
            Titulo = titulo;
            Texto = texto;
            LocalidadId = localidadId;
            CreadorId = usuarioId;
            FechaModificacion = DateTime.UtcNow;
        }

        public Nota()
        {
            Titulo = string.Empty;
        }

    }
}

