using GuiaBakio.Helpers;
using GuiaBakio.Models;
using GuiaBakio.Services.Interfaces;
using SQLite;

namespace GuiaBakio.Services
{
    public class DataBaseService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly IDialogYesNoService _dialogService;
        public DataBaseService(string dbPath, IDialogYesNoService dialogService)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
            var _ = InitTablesAsync();
        }
        public async Task InitTablesAsync()
        {
            await _db.CreateTableAsync<Localidad>();
            await _db.CreateTableAsync<Nota>();
            await _db.CreateTableAsync<Foto>();
            await _db.CreateTableAsync<Etiqueta>();
            await _db.CreateTableAsync<NotaEtiqueta>();
            await _db.CreateTableAsync<Usuario>();

            // Crear etiquetas predeterminadas si no existen
            if (await _db.Table<Etiqueta>().CountAsync() == 0)
            {
                try
                {
                    List<Etiqueta> etiquetasPredeterminadas =
                    [
                        new Etiqueta("Comer", "restaurant","aabbccdd"),
                        new Etiqueta("Cafetería", "emoji_food_beverage","aabbccdd"),
                        new Etiqueta("Pintxos", "tapas", "aabbccdd"),
                        new Etiqueta("Pasear", "hiking", "aabbccdd"),
                        new Etiqueta("Paisaje", "landscape", "aabbccdd"),
                        new Etiqueta("Aparcar", "local_parking", "aabbccdd"),
                    ];
                    await _db.InsertAllAsync(etiquetasPredeterminadas);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"No se pudieron crear las etiquetas predeterminadas. {ex.Message}");
                }
            }

        }

        #region "Localidades"
        public async Task<string> InsertarLocalidadAsync(string nombreLocalidad, string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentNullException(nameof(nombreLocalidad), "El Id de la nota no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");

            bool localidadExiste = await ExisteLocalidadConNombreAsync(nombreLocalidad);
            if (localidadExiste)
                throw new InvalidOperationException("Ya existe una localidad con ese nombre.");

            try
            {
                Localidad localidad = new(nombreLocalidad, usuarioId);
                await _db.InsertAsync(localidad);
                return localidad.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la localidad. {ex.Message}");
            }
        }
        public async Task<bool> ExisteLocalidadConIdAsync(string localidadId)
        {
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de localidad no puede estar vacío.");

            var localidad = await _db.FindAsync<Localidad>(localidadId);
            return localidad != null;
        }
        public async Task<bool> ExisteLocalidadConNombreAsync(string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(nombreLocalidad));

            nombreLocalidad = MisUtils.NormalizarTexto(nombreLocalidad).Trim();

            var _localidad = await _db.Table<Localidad>()
                                    .Where(a => a.Nombre.ToLower() == nombreLocalidad.ToLower())
                                    .FirstOrDefaultAsync();
            return _localidad != null;
        }
        public async Task<List<Localidad>> ObtenerLocalidadesAsync()
        {
            return await _db.Table<Localidad>().ToListAsync();
        }
        public async Task<Localidad> ObtenerLocalidadPorIdAsync(string localidadId)
        {
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad no puede estar vacío.");
            return await _db.FindAsync<Localidad>(localidadId);
        }
        public async Task<Localidad?> ObtenerLocalidadPorNombreAsync(string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentNullException(nameof(nombreLocalidad), "El nombre de la localidad no puede estar vacío.");

            nombreLocalidad = MisUtils.NormalizarTexto(nombreLocalidad).Trim();
            return await _db.Table<Localidad>()
                            .Where(l => l.Nombre.ToLower() == nombreLocalidad.ToLower())
                                     .FirstOrDefaultAsync();
        }
        public async Task ActualizarLocalidadAsync(Localidad? localidad)
        {
            if (localidad == null)
                throw new ArgumentNullException(nameof(localidad), "La localidad no puede ser nula.");
            if (string.IsNullOrWhiteSpace(localidad.Nombre))
                throw new ArgumentException($"El nuevo nombre de la localidad es obligatorio. {localidad.Nombre}");
            if (string.IsNullOrWhiteSpace(localidad.Id))
                throw new ArgumentException($"El Id de la localidad no puede estar vacío. {localidad.Id}");
            bool existeLocalidad = await ExisteLocalidadConNombreAsync(localidad.Nombre);
            if (!existeLocalidad)
                throw new InvalidOperationException($"No se encontró la localidad con Id: {localidad.Id}");

            try
            {
                localidad.FechaModificacion = DateTime.UtcNow;
                await _db.UpdateAsync(localidad);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar la localidad. {ex.Message}");
            }
        }
        public async Task<int> EliminarLocalidadAsync(string localidadId, bool confirmarBorrado = true)
        {
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad no puede estar vacío.");
            bool existeLocalidad = await ExisteLocalidadConIdAsync(localidadId);
            if (!existeLocalidad)
                throw new InvalidOperationException($"No se encontró la localidad con Id: {localidadId}");

            try
            {
                var localidadImagenes = await ObtenerImagenesPorEntidadAsync(TipoEntidad.Localidad, localidadId);
                if (confirmarBorrado)
                {
                    string texto = "¿Seguro que quieres borrar esta localidad?";

                    if (localidadImagenes != null)
                    {
                        if (localidadImagenes.Count > 0)
                            texto = "Hay alguna(s) imagen(es) asociada(s) a esta localidad. " + texto;
                    }
                    bool confirmacion = await _dialogService.ShowAlertAsync(
                        "Confirmar borrado",
                        texto,
                        "Sí",
                        "No");
                    if (!confirmacion)
                        return 0; // Cancelar el borrado si el usuario no confirma      
                }

                if (localidadImagenes != null)
                    foreach (Foto imagen in localidadImagenes)
                        await EliminarImagenAsync(imagen.Id, false);
                return await _db.DeleteAsync<Localidad>(localidadId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la localidad.{ex.Message}");
            }
        }

        #endregion

        #region "Notas"
        public async Task<string> InsertarNotaAsync(string titulo, string localidadId, string usuarioId, string texto = "")
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentNullException(nameof(titulo), "El título de la nota es obligatorio.");
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");

            bool existeLocalidad = await ExisteLocalidadConIdAsync(localidadId);
            if (!existeLocalidad)
                throw new InvalidOperationException($"No se encontró la localidad con Id: {localidadId}");

            bool existeNota = await ExisteNotaPorTituloYLocalidadAsync(titulo, localidadId);
            if (existeNota)
                throw new InvalidOperationException("Ya existe una nota con ese título en este localidad.");

            try
            {
                Nota nota = new(titulo, localidadId, usuarioId, texto);
                await _db.InsertAsync(nota);
                return nota.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la nota. {ex.Message}");
            }
        }
        public async Task<bool> ExisteNotaConIdAsync(string notaId)
        {
            if (string.IsNullOrWhiteSpace(notaId)) throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");

            var nota = await _db.FindAsync<Nota>(notaId);
            return nota != null;
        }
        public async Task<bool> ExisteNotaPorTituloYLocalidadAsync(string titulo, string localidadId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentNullException(nameof(titulo), "El título de la nota es obligatorio.");
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad debe ser mayor que cero.");

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            var nota = await _db.Table<Nota>()
                                .Where(n => n.Titulo.ToLower() == titulo.ToLower()
                                         && n.LocalidadId == localidadId)
                                .FirstOrDefaultAsync();
            return nota != null;
        }
        public async Task<List<Nota>> ObtenerNotasPorLocalidadAsync(string localidadId)
        {
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad no puede estar vacío.");

            return await _db.Table<Nota>()
                            .Where(a => a.LocalidadId == localidadId)
                            .ToListAsync();
        }
        public async Task<List<Nota>> ObtenerNotasPorEtiquetasAsync(string localidadId, List<Etiqueta> listaEtiquetas)
        {
            if (string.IsNullOrWhiteSpace(localidadId))
                throw new ArgumentNullException(nameof(localidadId), "El Id de la localidad no puede estar vacío.");

            if (listaEtiquetas == null || listaEtiquetas.Count == 0)
                return await _db.Table<Nota>()
                            .Where(a => a.LocalidadId == localidadId)
                            .ToListAsync();

            var etiquetaIds = listaEtiquetas.Select(e => e.Id).ToList();

            // Obtener relaciones que coincidan con alguna etiqueta
            var relaciones = await _db.Table<NotaEtiqueta>()
                                      .Where(ne => etiquetaIds.Contains(ne.EtiquetaId))
                                      .ToListAsync();

            // Obtener los IDs únicos de notas relacionadas
            var notaIds = relaciones.Select(r => r.NotaId).Distinct().ToList();

            // Obtener las notas correspondientes
            var notas = await _db.Table<Nota>()
                                 .Where(n => (n.LocalidadId == localidadId) && notaIds.Contains(n.Id))
                                 .ToListAsync();

            return notas;
        }
        public async Task<Nota> ObtenerNotaPorIdAsync(string notaId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
                throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");

            return await _db.FindAsync<Nota>(notaId);
        }
        public async Task ActualizarNotaAsync(Nota nota)
        {
            if (nota == null)
                throw new ArgumentNullException(nameof(nota), "La nota no puede ser nula.");
            if (string.IsNullOrWhiteSpace(nota.Titulo))
                throw new ArgumentException($"El nuevo título de la nota es obligatorio. {nameof(nota.Titulo)}");

            try
            {
                nota.FechaModificacion = DateTime.UtcNow;
                await _db.UpdateAsync(nota);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar la nota. {ex.Message}");
            }
        }
        public async Task<int> EliminarNotaAsync(string notaId, bool confirmarBorrado = true)
        {
            if (string.IsNullOrWhiteSpace(notaId)) throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");

            try
            {
                bool existeNota = await ExisteLocalidadConIdAsync(notaId);
                if (!existeNota)
                    throw new InvalidOperationException($"No se encontró la nota con Id: {notaId}");

                var notaImagenes = await ObtenerImagenesPorEntidadAsync(TipoEntidad.Nota, notaId);

                if (confirmarBorrado)
                {
                    string texto = "¿Seguro que quieres borrar esta nota?";

                    if (notaImagenes != null)
                    {
                        if (notaImagenes.Count > 0)
                            texto = "Hay algunas imágenes asociadas a esta nota. " + texto;
                    }

                    bool confirmacion = await _dialogService.ShowAlertAsync(
                        "Confirmar borrado",
                        texto,
                        "Sí",
                        "No");
                    if (!confirmacion)
                        return 0; // Cancelar el borrado si el usuario no confirma      
                }

                if (notaImagenes != null)
                    foreach (Foto imagenNota in notaImagenes)
                        await EliminarImagenAsync(imagenNota.Id, false);

                await DesasignarEtiquetasANotaAsync(notaId);

                return await _db.DeleteAsync<Nota>(notaId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la nota.{ex.Message}");
            }
        }

        #endregion

        #region "Imagenes"
        public async Task<string> InsertarImagensync(Foto imagen)
        {
            if (string.IsNullOrWhiteSpace(imagen.EntidadId))
                throw new ArgumentException($"El Id de la localidad o nota no puede estar vacío. {nameof(imagen.EntidadId)}");
            if (imagen.Blob is null)
                throw new ArgumentException($"La imagen no puede ser null. {nameof(imagen.Blob)}");
            if (imagen.Blob.Length == 0)
                throw new ArgumentException($"La imagen no puede estar vacía. {nameof(imagen.Blob)}");
            if (string.IsNullOrWhiteSpace(imagen.CreadorId))
                throw new ArgumentException($"El Id del usuario no puede estar vacío. {nameof(imagen.CreadorId)}");
            if ((imagen.TipoDeEntidad != TipoEntidad.Localidad && imagen.TipoDeEntidad != TipoEntidad.Nota))
                throw new ArgumentException($"El tipo de entidad (localidad, aparatado o nota) es incorrecto. {nameof(imagen.TipoDeEntidad)}");


            try
            {
                switch (imagen.TipoDeEntidad)
                {
                    case TipoEntidad.Localidad:
                        bool localidadExiste = await ExisteLocalidadConIdAsync(imagen.EntidadId);
                        if (!localidadExiste)
                            throw new InvalidOperationException($"La localidad con Id '{imagen.EntidadId}' no existe.");
                        break;
                    case TipoEntidad.Nota:
                        bool notaExiste = await ExisteNotaConIdAsync(imagen.EntidadId);
                        if (!notaExiste)
                            throw new InvalidOperationException($"La nota con Id '{imagen.EntidadId}' no existe.");
                        break;
                    default:
                        throw new ArgumentException($"El tipo de entidad (localidad, aparatado o nota) es incorrecto. {nameof(imagen.TipoDeEntidad)}");
                }


                await _db.InsertAsync(imagen);
                return imagen.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo insertar la imagen. {ex.Message}");
            }
        }
        public async Task<Foto> ObtenerImagenAsync(string imagenId)
        {
            if (string.IsNullOrWhiteSpace(imagenId)) throw new ArgumentNullException(nameof(imagenId), "El Id de la imagen no puede estar vacío.");

            return await _db.FindAsync<Foto>(imagenId);
        }
        public async Task<List<Foto>> ObtenerImagenesPorEntidadAsync(TipoEntidad tipoEntidad, string entidadId)
        {
            if (string.IsNullOrWhiteSpace(entidadId))
                throw new ArgumentNullException(nameof(entidadId), "El Id de la localidad o nota no puede estar vacío.");
            if (tipoEntidad != TipoEntidad.Localidad && tipoEntidad != TipoEntidad.Nota)
                throw new ArgumentException("El tipo de entidad (localidad o nota) es incorrecto.", nameof(tipoEntidad));

            return await _db.Table<Foto>()
                            .Where(a => a.TipoDeEntidad == tipoEntidad && a.EntidadId == entidadId)
                            .ToListAsync();
        }
        public async Task<int> EliminarImagenAsync(string imagenId, bool confirmarBorrado = true)
        {
            if (string.IsNullOrWhiteSpace(imagenId)) throw new ArgumentNullException(nameof(imagenId), "El Id de la imagen no puede estar vacío.");

            try
            {
                _ = await _db.FindAsync<Foto>(imagenId) ?? throw new InvalidOperationException($"No se encontró la imagen con Id: {imagenId}");

                if (confirmarBorrado)
                {
                    bool confirmacion = await _dialogService.ShowAlertAsync(
                        "Confirmar borrado",
                        "¿Seguro que quieres borrar esta imagen?",
                        "Sí",
                        "No");
                    if (!confirmacion)
                        return 0; // Cancelar el borrado si el usuario no confirma      
                }

                return await _db.DeleteAsync<Foto>(imagenId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la imagen. {ex.Message}");
            }
        }

        #endregion

        #region "Etiquetas"
        public async Task<string> InsertarEtiquetaAsync(string nombre, string icono, string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentNullException(nameof(nombre), "El nombre de la etiqueta es obligatorio.");
            if (string.IsNullOrWhiteSpace(icono))
                throw new ArgumentNullException(nameof(icono), "El icono de la etiqueta es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuarioId))
                throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");

            try
            {
                bool existeEtiqueta = await ExisteEtiquetaConNombreAsync(nombre);
                if (existeEtiqueta)
                    throw new InvalidOperationException($"Ya existe una etiqueta con el nombre: {nombre}");

                Etiqueta etiqueta = new(nombre, icono, usuarioId);
                await _db.InsertAsync(etiqueta);
                return etiqueta.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la etiqueta. {ex.Message}");
            }
        }
        public async Task<bool> ExisteEtiquetaConNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la etiqueta es obligatorio.", nameof(nombre));

            nombre = MisUtils.NormalizarTexto(nombre).Trim();
            var etiqueta = await _db.Table<Etiqueta>()
                                    .Where(a => a.Nombre.ToLower() == nombre.ToLower())
                                    .FirstOrDefaultAsync();
            return etiqueta != null;
        }
        public async Task<List<Etiqueta>> ObtenerTodasLasEtiquetasAsync()
        {
            return await _db.Table<Etiqueta>().ToListAsync();
        }
        public async Task<List<Etiqueta>> ObtenerEtiquetasDeNotaAsync(string notaId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
                throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");

            try
            {
                bool existeNota = await ExisteNotaConIdAsync(notaId);
                if (!existeNota)
                    throw new InvalidOperationException($"No se encontró la nota con Id: {notaId}");

                // Obtener los IDs de etiquetas asociadas a la nota
                var relaciones = await _db.Table<NotaEtiqueta>()
                                         .Where(ne => ne.NotaId == notaId)
                                         .ToListAsync();

                var etiquetaIds = relaciones.Select(r => r.EtiquetaId).ToList();

                // Obtener las etiquetas correspondientes
                var etiquetas = await _db.Table<Etiqueta>()
                                        .Where(e => etiquetaIds.Contains(e.Id))
                                        .ToListAsync();

                return etiquetas;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al obtener las etiquetas de la nota. {ex.Message}");
            }
        }
        public async Task<Etiqueta> ObtenerEtiquetaPorIdAsync(string etiquetaId)
        {
            if (string.IsNullOrWhiteSpace(etiquetaId))
                throw new ArgumentNullException(nameof(etiquetaId), "El Id de la etiqueta no puede estar vacío.");

            return await _db.FindAsync<Etiqueta>(etiquetaId);
        }
        public async Task ActualizarEtiquetaAsync(Etiqueta etiqueta)
        {
            if (etiqueta == null)
                throw new ArgumentNullException(nameof(etiqueta), "La etiqueta no puede ser nula.");
            if (string.IsNullOrWhiteSpace(etiqueta.Nombre))
                throw new ArgumentException($"El nuevo nombre de la etiqueta es obligatorio. {nameof(etiqueta.Nombre)}");
            if (string.IsNullOrWhiteSpace(etiqueta.Id))
                throw new ArgumentException($"El Id de la etiqueta no puede estar vacío. {nameof(etiqueta.Id)}");

            try
            {
                await _db.UpdateAsync(etiqueta);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar la etiqueta. {ex.Message}");
            }
        }
        public async Task<int> EliminarEtiquetaAsync(string etiquetaId, bool confirmarBorrado = true)
        {
            if (string.IsNullOrWhiteSpace(etiquetaId))
                throw new ArgumentNullException(nameof(etiquetaId), "El Id de la etiqueta no puede estar vacío.");

            try
            {
                _ = await ObtenerEtiquetaPorIdAsync(etiquetaId) ?? throw new InvalidOperationException("No se encontró la etiqueta.");

                if (confirmarBorrado)
                {
                    bool confirmacion = await _dialogService.ShowAlertAsync(
                        "Confirmar borrado",
                        "¿Seguro que quieres borrar esta etiqueta?",
                        "Sí",
                        "No");
                    if (!confirmacion)
                        return 0; // Cancelar el borrado si el usuario no confirma      
                }

                return await _db.DeleteAsync<Etiqueta>(etiquetaId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la etiqueta. {ex.Message}");
            }
        }
        public async Task DesasignarEtiquetasANotaAsync(string notaId)
        {
            if (string.IsNullOrWhiteSpace(notaId))
                throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");

            try
            {
                bool existeNota = await ExisteNotaConIdAsync(notaId);
                if (!existeNota)
                    throw new InvalidOperationException($"No se encontró la nota con Id: {notaId}");

                var relaciones = await _db.Table<NotaEtiqueta>()
                                         .Where(ne => ne.NotaId == notaId)
                                         .ToListAsync();
                foreach (var relacion in relaciones)
                {
                    await _db.DeleteAsync(relacion);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar las etiquetas de la nota. {ex.Message}");
            }
        }
        public async Task AsignarEtiquetasANotaAsync(string notaId, List<Etiqueta> listaEtiquetas)
        {
            if (string.IsNullOrWhiteSpace(notaId))
                throw new ArgumentNullException(nameof(notaId), "El Id de la nota no puede estar vacío.");
            if (listaEtiquetas == null || listaEtiquetas.Count == 0)
                return; // No hay etiquetas para asignar, salir del método

            try
            {
                _ = await _db.FindAsync<Nota>(notaId) ?? throw new InvalidOperationException($"No se encontró la nota con Id: {notaId}");

                foreach (var etiqueta in listaEtiquetas)
                {
                    var relacionExistente = await _db.Table<NotaEtiqueta>()
                                                     .Where(ne => ne.NotaId == notaId && ne.EtiquetaId == etiqueta.Id)
                                                     .FirstOrDefaultAsync();
                    if (relacionExistente == null)
                    {
                        NotaEtiqueta nuevaRelacion = new(notaId, etiqueta.Id);
                        await _db.InsertAsync(nuevaRelacion);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al asignar las etiquetas a la nota. {ex.Message}");
            }

        }

        #endregion

        #region "Usuarios"
        public async Task<string> InsertarUsuarioAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentNullException(nameof(nombre), "El nombre del usuario es obligatorio.");

            try
            {
                bool existeUsuario = (await ObtenerUsuarioPorNombreAsync(nombre)) != null;
                if (existeUsuario)
                    throw new InvalidOperationException("Ya existe un usuario con ese nombre.");

                Usuario usuario = new(nombre);
                await _db.InsertAsync(usuario);
                return usuario.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear el usuario. {ex.Message}");
            }
        }
        public async Task<Usuario> ObtenerUsuarioPorNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.", nameof(nombre));

            try
            {
                nombre = MisUtils.NormalizarTexto(nombre).Trim();
                var usuario = await _db.Table<Usuario>()
                                        .Where(a => a.Nombre.ToLower() == nombre.ToLower())
                                        .FirstOrDefaultAsync();
                return usuario;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al obtener el usuario. {ex.Message}");
            }
        }
        public async Task<Usuario> ObtenerUsuarioPorIdAsync(string usuarioId)
        {
            if (string.IsNullOrWhiteSpace(usuarioId)) throw new ArgumentNullException(nameof(usuarioId), "El Id del usuario no puede estar vacío.");
            return await _db.FindAsync<Usuario>(usuarioId);
        }

        #endregion

        // Métodos para convertir ImageSource a byte[] y viceversa
        public static async Task<byte[]?> ConvertirImageSourceABytesAsync(ImageSource? imagen)
        {
            if (imagen == null)
                return null;

            if (imagen is StreamImageSource streamImage)
            {
                var stream = await streamImage.Stream.Invoke(CancellationToken.None);
                using MemoryStream memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }

            // Si el tipo de ImageSource no es StreamImageSource, no se puede convertir fácilmente
            return null;
        }

        public static ImageSource? ConvertirBytesAImageSourceAsync(byte[]? foto)
        {
            if (foto == null)
                return null;

            // MemoryStream stream = await Task.Run(() => new MemoryStream(foto));
            return ImageSource.FromStream(() => new MemoryStream(foto));
        }
    }
}
