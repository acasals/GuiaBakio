using GuiaBakio.Models;

namespace GuiaBakio.Helpers
{
    public static class ApiMapper
    {
        public static LocalidadDto LocalidadToDto(Localidad localidad)
        {
            return new LocalidadDto
            {
                Id = localidad.Id,
                Nombre = localidad.Nombre,
                Texto = localidad.Texto,
                CreadorId = localidad.CreadorId,
                FechaModificacion = localidad.FechaModificacion
            };
        }

        public static Localidad DtoToLocalidad(LocalidadDto dto)
        {
            return new Localidad
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Texto = dto.Texto,
                CreadorId = dto.CreadorId,
                FechaModificacion = dto.FechaModificacion,
                Sincronizado = true
            };
        }

        public static NotaDto NotaToDto(Nota nota)
        {
            return new NotaDto
            {
                Id = nota.Id,
                LocalidadId = nota.LocalidadId,
                Titulo = nota.Titulo,
                Texto = nota.Texto,
                CreadorId = nota.CreadorId,
                FechaModificacion = nota.FechaModificacion
            };
        }

        public static Nota DtoToNota(NotaDto dto)
        {
            return new Nota
            {
                Id = dto.Id,
                LocalidadId = dto.LocalidadId,
                Titulo = dto.Titulo,
                Texto = dto.Texto,
                CreadorId = dto.CreadorId,
                FechaModificacion = dto.FechaModificacion,
                Sincronizado = true
            };
        }

        public static EtiquetaDto EtiquetaToDto(Etiqueta etiqueta)
        {
            return new EtiquetaDto
            {
                Id = etiqueta.Id,
                Nombre = etiqueta.Nombre,
                Icono = etiqueta.Icono,
                CreadorId = etiqueta.CreadorId,
                FechaModificacion = etiqueta.FechaModificacion
            };
        }

        public static Etiqueta DtoToEtiqueta(EtiquetaDto dto)
        {
            return new Etiqueta
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Icono = dto.Icono,
                CreadorId = dto.CreadorId,
                FechaModificacion = dto.FechaModificacion,
                Sincronizado = true
            };
        }

        public static FotoDto FotoToDto(Foto foto)
        {
            return new FotoDto
            {
                Id = foto.Id,
                EntidadId = foto.EntidadId,
                Nombre = foto.Nombre,
                Blob = foto.Blob,
                EsMapa = foto.EsMapa,
                UrlMapa = foto.UrlMapa,
                CreadorId = foto.CreadorId,
                FechaModificacion = foto.FechaModificacion,
                TipoDeEntidad = (int)foto.TipoDeEntidad
            };
        }

        public static Foto DtoToFoto(FotoDto dto)
        {
            return new Foto
            {
                Id = dto.Id,
                EntidadId = dto.EntidadId,
                Nombre = dto.Nombre,
                Blob = dto.Blob,
                EsMapa = dto.EsMapa,
                UrlMapa = dto.UrlMapa,
                CreadorId = dto.CreadorId,
                FechaModificacion = dto.FechaModificacion,
                TipoDeEntidad = (TipoEntidad)dto.TipoDeEntidad,
                Sincronizado = true
            };
        }

        public static UsuarioDto UsuarioToDto(Usuario usuario)
        {
            return new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                FechaModificacion = usuario.FechaModificacion
            };
        }

        public static Usuario DtoToUsuario(UsuarioDto dto)
        {
            return new Usuario
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                FechaModificacion = dto.FechaModificacion,
                Sincronizado = true
            };
        }

        public static NotaEtiquetaDto NotaEtiquetaToDto(NotaEtiqueta relacion)
        {
            return new NotaEtiquetaDto
            {
                Id = relacion.Id,
                NotaId = relacion.NotaId,
                EtiquetaId = relacion.EtiquetaId,
                FechaModificacion = relacion.FechaModificacion
            };
        }

        public static NotaEtiqueta DtoToEtiqueta(NotaEtiquetaDto dto)
        {
            return new NotaEtiqueta
            {
                Id = dto.Id,
                NotaId = dto.NotaId,
                EtiquetaId = dto.EtiquetaId,
                FechaModificacion = dto.FechaModificacion,
                Sincronizado = true
            };
        }

    }

}