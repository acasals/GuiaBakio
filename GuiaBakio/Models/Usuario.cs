using SQLite;

namespace GuiaBakio.Models
{
    public class Usuario
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string Nombre { get; set; }

        public Usuario(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentNullException(nameof(nombre), "El Id de la nota no puede estar vacío.");
            Id = Guid.NewGuid().ToString();
            Nombre = nombre;
        }
        public Usuario()
        {
            Id = Guid.NewGuid().ToString();
            Nombre = string.Empty;
        }
    }
}
