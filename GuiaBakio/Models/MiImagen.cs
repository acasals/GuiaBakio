using SQLite;

namespace GuiaBakio.Models
{
    public class MiImagen
    {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public byte[]? Foto { get; set; }
        public bool EsMapa { get; set; } 
        public string UrlMapa { get; set; }


        public MiImagen( byte[] byteArray, string nombre = "", bool esmapa = false, string urlMapa= "")
        {
            if (byteArray == null)
                throw new ArgumentException("La imagen no puede ser nula.", nameof(byteArray));
            Foto = byteArray;
            Nombre = nombre;
            EsMapa = esmapa;
            UrlMapa = urlMapa;
        }
        public MiImagen() { }
    }
}
