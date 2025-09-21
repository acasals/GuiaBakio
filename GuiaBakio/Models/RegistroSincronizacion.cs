using SQLite;

namespace GuiaBakio.Models
{
    public class RegistroSincronizacion
    {
        [PrimaryKey]
        public TipoEntidad Entidad { get; set; }
        public DateTime FechaModificacion { get; set; }

    }
}
