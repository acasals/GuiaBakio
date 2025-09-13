namespace GuiaBakio.Models
{
    public class NotaDto
    {
        public string Id { get; set; }
        public string LocalidadId { get; set; }
        public string Titulo { get; set; }
        public string? Texto { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
    }
}
