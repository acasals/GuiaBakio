using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace GuiaBakio.Models
{
    public partial class Localidad : ObservableObject
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;



        public Localidad(string localidad, string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(localidad))
                throw new ArgumentNullException(nameof(localidad), "El nombre de la localidad es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");
            Id = Guid.NewGuid().ToString();
            Nombre = localidad;
            CreadorId = usuarioId;
            FechaModificacion = DateTime.UtcNow;
        }
        public Localidad()
        {
            Id = Guid.NewGuid().ToString();
            Nombre = string.Empty;
            CreadorId = string.Empty;
            FechaModificacion = DateTime.UtcNow;
        }

        // Propiedades ignoradas por SQLite
        private bool isSelected;
        [Ignore]
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        private bool isButton;
        [Ignore]
        public bool IsButton
        {
            get => isButton;
            set => SetProperty(ref isButton, value);
        }
    }
}
