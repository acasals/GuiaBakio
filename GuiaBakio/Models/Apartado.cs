﻿using SQLite;

namespace GuiaBakio.Models
{
    public class Apartado
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [NotNull]
        public string Nombre { get; set; }
        public string? Texto { get; set; }
        public int LocalidadId { get; set; }
        public DateTime FechaModificacion { get; set; }
        public bool Sincronizado { get; set; } = false;
        public Apartado(string nombre, int localidadId,string texto="")
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombre));
            if (localidadId <= 0)
                throw new ArgumentException("El ID de la localidad debe ser mayor que cero.", nameof(localidadId));
            Nombre = nombre;
            Texto = texto;
            LocalidadId = localidadId;
            FechaModificacion = DateTime.UtcNow;
        }
        public Apartado()
        {
            Nombre = string.Empty;
        }

    }
}
