using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
namespace GuiaBakio.Models
{
    public partial class Etiqueta : ObservableObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Nombre { get; set; }

        public string? Icono { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;


        public Etiqueta(string nombre, string icono, string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentNullException(nameof(nombre), "El nombre de la etiqueta es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El ID del usuario no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(icono))
                throw new ArgumentNullException(nameof(icono), "El icono de la etiqueta no puede estar vacío.");

            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
            Icono = icono;
            CreadorId = usuarioId;
            FechaModificacion = DateTime.UtcNow;
        }
        public Etiqueta()
        {
            Id = Guid.NewGuid().ToString();
            Nombre = string.Empty;
            CreadorId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }

        [ObservableProperty]
        private bool isSelected;
    }
}
