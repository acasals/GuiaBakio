namespace GuiaBakio.Models
{
    public class FotoDto
    {
        public string Id { get; set; }
        public string EntidadId { get; set; }
        public string? Nombre { get; set; }
        public byte[]? Blob { get; set; }
        public bool EsMapa { get; set; }
        public string? UrlMapa { get; set; }
        public string CreadorId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int TipoDeEntidad { get; set; } // Enum como entero
    }
}
