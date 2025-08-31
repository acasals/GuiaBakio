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

            // Crear etiquetas predeterminadas si no existen
            if (await _db.Table<Etiqueta>().CountAsync() == 0)
            {
                try
                {
                    List<Etiqueta> etiquetasPredeterminadas =
                    [
                        new Etiqueta("Comer", "restaurant"),
                        new Etiqueta("Cafetería", "emoji_food_beverage"),
                        new Etiqueta("Pintxos", "tapas"),
                        new Etiqueta("Pasear", "hiking"),
                        new Etiqueta("Paisaje", "landscape"),
                        new Etiqueta("Aparcar", "local_parking"),
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
        public async Task<int> InsertarLocalidadAsync(string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(nombreLocalidad));

            bool localidadExiste = await ExisteLocalidadAsync(nombreLocalidad);
            if (localidadExiste)
                throw new InvalidOperationException("Ya existe una localidad con ese nombre.");

            try
            {
                Localidad localidad = new(nombreLocalidad);
                await _db.InsertAsync(localidad);
                return localidad.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la localidad. {ex.Message}");
            }
        }
        public async Task<bool> ExisteLocalidadAsync(int localidadId)
        {
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));

            var localidad = await _db.FindAsync<Localidad>(localidadId);
            return localidad != null;
        }
        public async Task<bool> ExisteLocalidadAsync(string nombreLocalidad)
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
        public async Task<Localidad?> ObtenerLocalidadAsync(int localidadId)
        {
            if (localidadId <= 0)
                throw new ArgumentException("El Id de localidad debe ser mayor que 0.", nameof(localidadId));

            return await _db.Table<Localidad>()
                            .Where(l => l.Id == localidadId)
                                     .FirstOrDefaultAsync();
        }
        public async Task<Localidad?> ObtenerLocalidadAsync(string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("El nombre de la localidad no puede estar vacío.", nameof(nombreLocalidad));

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
                throw new ArgumentException("El nuevo nombre de la localidad es obligatorio.", nameof(localidad.Nombre));

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
        public async Task ActualizarLocalidadAsync(Localidad? localidad, string nuevoNombre, string texto = "")
        {
            if (localidad == null)
                throw new ArgumentNullException(nameof(localidad), "La localidad no puede ser nula.");
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nuevo nombre de la localidad es obligatorio.", nameof(nuevoNombre));

            try
            {
                Localidad? _localidad = await ObtenerLocalidadAsync(localidad.Id) ?? throw new InvalidOperationException("No se encontró la localidad.");

                _localidad.Nombre = nuevoNombre;
                _localidad.Texto = texto;
                _localidad.FechaModificacion = DateTime.UtcNow;

                await _db.UpdateAsync(_localidad);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar la localidad. {ex.Message}");
            }
        }
        public async Task<int> EliminarLocalidadAsync(int localidadId, bool confirmarBorrado = true)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));
            var localidad = await ObtenerLocalidadAsync(localidadId) ?? throw new InvalidOperationException("No se encontró la localidad.");

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
            try
            {
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
        public async Task<int> InsertarNotaAsync(string titulo, int localidadId, string texto = "")
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que cero.", nameof(localidadId));

            bool localidadExiste = await ExisteLocalidadAsync(localidadId);
            if (!localidadExiste)
                throw new InvalidOperationException($"El localidad con Id '{localidadId}' no existe.");

            bool existeNota = await ExisteNotaAsync(titulo, localidadId);
            if (existeNota)
                throw new InvalidOperationException("Ya existe una nota con ese título en este localidad.");

            try
            {
                Nota nota = new(titulo, texto, localidadId);
                await _db.InsertAsync(nota);
                return nota.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la nota. {ex.Message}");
            }
        }
        public async Task<int> InsertarNotaAsync(string titulo, string nombreLocalidad, string texto = "")
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("La localidad es obligatoria.", nameof(nombreLocalidad));

            var _localidad = await ObtenerLocalidadAsync(nombreLocalidad) ?? throw new InvalidOperationException("No se encontró la localidad con nombre '{localidad}'.");

            bool existeNota = await ExisteNotaAsync(titulo, _localidad.Id);

            if (existeNota)
                throw new InvalidOperationException("Ya existe una nota con ese título en esta localidad.");

            try
            {
                Nota nota = new(titulo, texto, _localidad.Id);
                await _db.InsertAsync(nota);
                return nota.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la nota. {ex.Message}");
            }
        }
        public async Task<bool> ExisteNotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            var nota = await _db.FindAsync<Nota>(notaId);
            return nota != null;
        }
        public async Task<bool> ExisteNotaAsync(string titulo, int localidadId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que cero.", nameof(localidadId));

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            var nota = await _db.Table<Nota>()
                                .Where(n => n.Titulo.ToLower() == titulo.ToLower()
                                         && n.LocalidadId == localidadId)
                                .FirstOrDefaultAsync();
            return nota != null;
        }
        public async Task<bool> ExisteNotaAsync(string titulo, string nombrelocalidad, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("La localidad es obligatoria.", nameof(nombreLocalidad));

            var _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad == null)
                throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            var existeNota = await _db.Table<Nota>()
                                      .Where(n => n.Titulo.ToLower() == titulo.ToLower()
                                               && n.LocalidadId == _localidad.Id)
                                      .FirstOrDefaultAsync();
            return existeNota != null;
        }
        public async Task<List<Nota>> ObtenerNotasAsync(int localidadId)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));
            return await _db.Table<Nota>()
                            .Where(a => a.LocalidadId == localidadId)
                            .ToListAsync();
        }

        public async Task<List<Nota>> ObtenerNotasPorEtiquetasAsync(int localidadId, List<Etiqueta> listaEtiquetas)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));
            if (listaEtiquetas == null || listaEtiquetas.Count == 0)
                return await _db.Table<Nota>().ToListAsync(); // O tu método personalizado

            var etiquetaIds = listaEtiquetas.Select(e => e.Id).ToList();

            // Obtener relaciones que coincidan con alguna etiqueta
            var relaciones = await _db.Table<NotaEtiqueta>()
                                      .Where(ne => etiquetaIds.Contains(ne.EtiquetaId))
                                      .ToListAsync();

            // Obtener los IDs únicos de notas relacionadas
            var notaIds = relaciones.Select(r => r.NotaId).Distinct().ToList();

            // Obtener las notas correspondientes
            var notas = await _db.Table<Nota>()
                                 .Where(n => n.LocalidadId == localidadId && notaIds.Contains(n.Id))
                                 .ToListAsync();

            return notas;
        }


        public async Task<Nota> ObtenerNotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));

            return await _db.Table<Nota>()
                            .Where(a => a.Id == notaId)
                            .FirstOrDefaultAsync();
        }
        public async Task<Nota> ObtenerNotaAsync(string titulo, int localidadId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El nombre del titulo es obligatorio.", nameof(titulo));
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            return await _db.Table<Nota>()
                            .Where(a => a.Titulo.ToLower() == titulo.ToLower() && a.LocalidadId == localidadId)
                            .FirstOrDefaultAsync();
        }
        public async Task<Nota?> ObtenerNotaAsync(string titulo, string nombrelocalidad, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("Una localidad es obligatoria.", nameof(nombreLocalidad));

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad != null)
            {
                titulo = MisUtils.NormalizarTexto(titulo).Trim();
                return await _db.Table<Nota>().Where(a => a.Titulo.ToLower() == titulo.ToLower() && a.LocalidadId == _localidad.Id).FirstOrDefaultAsync();
            }
            return null;
        }
        public async Task ActualizarNotaAsync(Nota nota)
        {
            if (nota == null)
                throw new ArgumentNullException(nameof(nota), "La nota no puede ser nula.");
            if (string.IsNullOrWhiteSpace(nota.Titulo))
                throw new ArgumentException("El nuevo título de la nota es obligatorio.", nameof(nota.Titulo));

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
        public async Task ActualizarNotaAsync(Nota nota, string nuevoTitulo, string nuevoContenido)
        {
            if (nota == null)
                throw new ArgumentNullException(nameof(nota), "La nota no puede ser nula.");
            if (string.IsNullOrWhiteSpace(nuevoTitulo))
                throw new ArgumentException("El nuevo título de la nota es obligatorio.", nameof(nuevoTitulo));

            try
            {
                Nota _nota = await ObtenerNotaAsync(nota.Id) ?? throw new InvalidOperationException("No se encontró la nota.");
                _nota.Titulo = nuevoTitulo;
                _nota.Texto = nuevoContenido;
                _nota.FechaModificacion = DateTime.UtcNow;
                await _db.UpdateAsync(_nota);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar la nota. {ex.Message}");
            }
        }
        public async Task<int> EliminarNotaAsync(int notaId, bool confirmarBorrado = true)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            _ = await ObtenerNotaAsync(notaId) ?? throw new InvalidOperationException("No se encontró la nota.");

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
            try
            {
                if (notaImagenes != null)
                    foreach (Foto imagenNota in notaImagenes)
                        await EliminarImagenAsync(imagenNota.Id, false);
                return await _db.DeleteAsync<Nota>(notaId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la nota.{ex.Message}");
            }
        }

        #endregion

        #region "Imagenes"
        public async Task<int> InsertarImagensync(int entidadId, TipoEntidad tipoEntidad, byte[] byteArray, string nombre = "", bool esMapa = false, string urlMapa = "")
        {
            if (entidadId <= 0)
                throw new ArgumentException("El Id de la localidad o nota debe ser mayor que cero.", nameof(entidadId));
            if (byteArray is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(byteArray));

            switch (tipoEntidad)
            {
                case TipoEntidad.Localidad:
                    bool localidadExiste = await ExisteLocalidadAsync(entidadId);
                    if (!localidadExiste)
                        throw new InvalidOperationException($"La localidad con Id '{entidadId}' no existe.");
                    break;
                case TipoEntidad.Nota:
                    bool notaExiste = await ExisteNotaAsync(entidadId);
                    if (!notaExiste)
                        throw new InvalidOperationException($"La nota con Id '{entidadId}' no existe.");
                    break;
                default:
                    throw new ArgumentException("El tipo de entidad (localidad, aparatado o nota) es incorrecto.", nameof(tipoEntidad));
            }

            try
            {
                Foto imagen = new(entidadId, tipoEntidad, byteArray, nombre, esMapa, urlMapa);
                await _db.InsertAsync(imagen);
                return imagen.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo insertar la imagen. {ex.Message}");
            }
        }

        public async Task<int> InsertarImagensync(Foto imagen)
        {
            if (imagen.EntidadId <= 0)
                throw new ArgumentException("El Id de la localidad o nota debe ser mayor que cero.", nameof(imagen.EntidadId));
            if (imagen.Blob is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(imagen.Blob));

            switch (imagen.TipoDeEntidad)
            {
                case TipoEntidad.Localidad:
                    bool localidadExiste = await ExisteLocalidadAsync(imagen.EntidadId);
                    if (!localidadExiste)
                        throw new InvalidOperationException($"La localidad con Id '{imagen.EntidadId}' no existe.");
                    break;
                case TipoEntidad.Nota:
                    bool notaExiste = await ExisteNotaAsync(imagen.EntidadId);
                    if (!notaExiste)
                        throw new InvalidOperationException($"La nota con Id '{imagen.EntidadId}' no existe.");
                    break;
                default:
                    throw new ArgumentException("El tipo de entidad (localidad, aparatado o nota) es incorrecto.", nameof(imagen.TipoDeEntidad));
            }

            try
            {
                await _db.InsertAsync(imagen);
                return imagen.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo insertar la imagen. {ex.Message}");
            }
        }
        public async Task<Foto> ObtenerImagenAsync(int imagenId)
        {
            if (imagenId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenId));

            return await _db.Table<Foto>()
                            .Where(a => a.Id == imagenId)
                            .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteImagenAsync(int imagenId)
        {
            if (imagenId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenId));
            var imagen = await _db.FindAsync<Foto>(imagenId);
            return imagen != null;
        }
        public async Task<List<Foto>> ObtenerImagenesPorEntidadAsync(TipoEntidad tipoEntidad, int entidadId)
        {
            if (entidadId <= 0) throw new ArgumentException("El Id de la localidad o nota debe ser mayor que 0.", nameof(entidadId));
            return await _db.Table<Foto>()
                            .Where(a => a.TipoDeEntidad == tipoEntidad && a.EntidadId == entidadId)
                            .ToListAsync();
        }
        public async Task<int> EliminarImagenAsync(int imagenId, bool confirmarBorrado = true)
        {
            if (imagenId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenId));

            _ = await _db.FindAsync<Foto>(imagenId) ?? throw new InvalidOperationException("No se encontró la imagen.");

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
            try
            {
                return await _db.DeleteAsync<Foto>(imagenId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la imagen. {ex.Message}");
            }
        }

        #endregion

        #region "Etiquetas"
        public async Task<int> InsertarEtiquetaAsync(string nombre, string icono)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la etiqueta es obligatorio.", nameof(nombre));
            if (string.IsNullOrWhiteSpace(icono))
                throw new ArgumentException("El icono de la etiqueta es obligatorio.", nameof(icono));
            bool existeEtiqueta = await ExisteEtiquetaAsync(nombre);
            if (existeEtiqueta)
                throw new InvalidOperationException("Ya existe una etiqueta con ese nombre.");
            try
            {
                Etiqueta etiqueta = new(nombre, icono);
                await _db.InsertAsync(etiqueta);
                return etiqueta.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la etiqueta. {ex.Message}");
            }
        }
        public async Task<bool> ExisteEtiquetaAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la etiqueta es obligatorio.", nameof(nombre));
            nombre = MisUtils.NormalizarTexto(nombre).Trim();
            var etiqueta = await _db.Table<Etiqueta>()
                                    .Where(a => a.Nombre.ToLower() == nombre.ToLower())
                                    .FirstOrDefaultAsync();
            return etiqueta != null;
        }

        public async Task<List<Etiqueta>> ObtenerEtiquetasAsync()
        {
            return await _db.Table<Etiqueta>().ToListAsync();
        }

        public async Task<List<Etiqueta>> ObtenerEtiquetasDeNotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            var nota = await _db.FindAsync<Nota>(notaId) ?? throw new InvalidOperationException("No se encontró la nota.");

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
        public async Task<Etiqueta> ObtenerEtiquetaAsync(int etiquetaId)
        {
            if (etiquetaId <= 0) throw new ArgumentException("El Id de la etiqueta debe ser mayor que 0.", nameof(etiquetaId));
            return await _db.Table<Etiqueta>()
                            .Where(a => a.Id == etiquetaId)
                            .FirstOrDefaultAsync();
        }

        public async Task ActualizarEtiquetaAsync(Etiqueta etiqueta)
        {
            if (etiqueta == null)
                throw new ArgumentNullException(nameof(etiqueta), "La etiqueta no puede ser nula.");
            if (string.IsNullOrWhiteSpace(etiqueta.Nombre))
                throw new ArgumentException("El nuevo nombre de la etiqueta es obligatorio.", nameof(etiqueta.Nombre));
            try
            {
                await _db.UpdateAsync(etiqueta);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar la etiqueta. {ex.Message}");
            }
        }

        public async Task<int> EliminarEtiquetaAsync(int etiquetaId, bool confirmarBorrado = true)
        {
            if (etiquetaId <= 0) throw new ArgumentException("El Id de la etiqueta debe ser mayor que 0.", nameof(etiquetaId));
            _ = await ObtenerEtiquetaAsync(etiquetaId) ?? throw new InvalidOperationException("No se encontró la etiqueta.");
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
            try
            {
                return await _db.DeleteAsync<Etiqueta>(etiquetaId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la etiqueta. {ex.Message}");
            }
        }

        public async Task DesasignarEtiquetasANotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            var nota = await _db.FindAsync<Nota>(notaId) ?? throw new InvalidOperationException("No se encontró la nota.");
            try
            {
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

        public async Task AsignarEtiquetasANotaAsync(int notaId, List<Etiqueta> listaEtiquetas)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            var nota = await _db.FindAsync<Nota>(notaId) ?? throw new InvalidOperationException("No se encontró la nota.");
            if (listaEtiquetas == null || listaEtiquetas.Count == 0)
                return; // No hay etiquetas para asignar, salir del método
            try
            {
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
        public static async Task<ImageSource?> ConvertirBytesAImageSourceAsync(byte[]? foto)
        {
            if (foto == null)
                return null;

            MemoryStream stream = await Task.Run(() => new MemoryStream(foto));
            return ImageSource.FromStream(() => stream);
        }
    }
}
