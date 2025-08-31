using SQLite;

namespace GuiaBakio.Models
{
    public class NotaEtiqueta
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int NotaId { get; set; }
        public int EtiquetaId { get; set; }
        public NotaEtiqueta() { }
        public NotaEtiqueta(int notaId, int etiquetaId)
        {
            NotaId = notaId;
            EtiquetaId = etiquetaId;
        }

    }
}
