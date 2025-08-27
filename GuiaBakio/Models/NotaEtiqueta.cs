using SQLite;

namespace GuiaBakio.Models
{
    public class NotaEtiqueta
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NotaId { get; set; }
        public int EtiquetaId { get; set; }

    }
}
