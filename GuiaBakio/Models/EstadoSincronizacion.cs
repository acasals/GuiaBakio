using SQLite;

namespace GuiaBakio.Models
{
    public class EstadoSincronizacion
    {
        [PrimaryKey]
        public TipoSincronizacion Tipo { get; set; }
        public DateTime UltimoIntento { get; set; }
        public bool FueExitosa { get; set; }
        public string? Error { get; set; }
    }
}
