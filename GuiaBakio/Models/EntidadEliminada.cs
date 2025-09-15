using SQLite;

namespace GuiaBakio.Models
{
    public class EntidadEliminada
    {
        [PrimaryKey]
        public string Id { get; set; }

        public TipoEntidad Tipo { get; set; }

        public DateTime FechaEliminacion { get; set; }

        public EntidadEliminada(TipoEntidad tipo)
        {
            Id = Guid.NewGuid().ToString();
            Tipo = tipo;
            FechaEliminacion = DateTime.UtcNow;
        }

        public EntidadEliminada()
        {
            Id = Guid.NewGuid().ToString();
            FechaEliminacion = DateTime.UtcNow;
        }
    }

}
