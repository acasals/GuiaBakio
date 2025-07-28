using SQLite;

namespace GuiaBakio.Models
{
    internal class Nota
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ApartadoId { get; set; }

        [NotNull]
        public string Titulo { get; set; }

        public string? Contenido { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;

        public Nota(string titulo, string texto, int apartadoId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (apartadoId <= 0)
                throw new ArgumentException("El ID del apartado ser mayor que cero.", nameof(apartadoId));
            Titulo = titulo;
            Contenido = texto;
            ApartadoId = apartadoId;
            FechaModificacion = DateTime.UtcNow;
        }

        public Nota()
        {
            Titulo = string.Empty;
        }
    }
}

