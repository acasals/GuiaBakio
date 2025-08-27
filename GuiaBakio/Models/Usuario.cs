using SQLite;

namespace GuiaBakio.Models
{
    public class Usuario
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public Usuario(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.", nameof(nombre));
            Nombre = nombre;
        }

    }
}
