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
            _dialogService= dialogService ?? throw new ArgumentNullException(nameof(dialogService), "El servicio de diálogo no puede ser nulo.");
            var _ = InitTablesAsync();
        }
        public async Task InitTablesAsync()
        {
            await _db.CreateTableAsync<Localidad>();
            await _db.CreateTableAsync<Apartado>();
            await _db.CreateTableAsync<Nota>();
            await _db.CreateTableAsync<MiImagen>();
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
        public async Task ActualizarLocalidadAsync(Localidad? localidad, string nuevoNombre, string texto="")
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
        public async Task<int> EliminarLocalidadAsync(int localidadId,bool confirmarBorrado = true)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));
            var localidad = await ObtenerLocalidadAsync(localidadId) ?? throw new InvalidOperationException("No se encontró la localidad.");
                        
            var localidadApartados = await ObtenerApartadosAsync(localidadId);
            var localidadImagenes = await ObtenerImagenesPorEntidadAsync(TipoEntidad.Localidad, localidadId);
            if (confirmarBorrado)
            {
                string texto = "¿Seguro que quieres borrar esta localidad?";

                if (localidadApartados != null)
                {
                    if (localidadApartados.Count > 0)
                        texto = "Hay algún(os) apartado(s) asociado(s) a esta localidad. " + texto;
                }
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
                if (localidadApartados != null)
                    foreach (Apartado apartado in localidadApartados)
                        await EliminarApartadoAsync(apartado.Id, false);
                if (localidadImagenes != null)
                    foreach(MiImagen imagen in localidadImagenes)
                        await EliminarImagenAsync(imagen.Id, false);
                return await _db.DeleteAsync<Localidad>(localidadId);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Hubo un problema al eliminar la localidad.{ex.Message}");
                }
            }

        #endregion

        #region "Apartados"
        public async Task<int> InsertarApartadoAsync(string nombreApartado, int localidadId, string texto="")
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que cero.", nameof(localidadId));

            bool localidadExiste = await ExisteLocalidadAsync(localidadId);
            if (!localidadExiste)
                throw new InvalidOperationException($"La localidad asociada  al Id '{localidadId}' no existe.");

            var existeApartado = await ExisteApartadoAsync(nombreApartado, localidadId);
            if (existeApartado)
                throw new InvalidOperationException("Ya existe un apartado con ese nombre en esta localidad.");
       
            try
            {
                Apartado apartado = new(nombreApartado, localidadId, texto);
                await _db.InsertAsync(apartado);
                return apartado.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear el apartado. {ex.Message}");
            }
        }
        public async Task<int> InsertarApartadoAsync(string nombreApartado, string nombreLocalidad, string texto="")
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("Una localidad es obligatoria.", nameof(nombreLocalidad));

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad) ?? throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            var existeApartado = await ExisteApartadoAsync(nombreApartado, nombreLocalidad);
            if (existeApartado)
                throw new InvalidOperationException("Ya existe un apartado con ese nombre en esta localidad.");

            try
            {
                Apartado apartado = new(nombreApartado, _localidad.Id, texto);
                await _db.InsertAsync(apartado);
                return apartado.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear el apartado. {ex.Message}");
            }
        }
        public async Task<bool> ExisteApartadoAsync(int apartadoId)
        {
            if (apartadoId <= 0) throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));
            var apartado = await _db.FindAsync<Apartado>(apartadoId);
            return apartado != null;
        }
        public async Task<bool> ExisteApartadoAsync(string nombreApartado, int localidadId)
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que cero.", nameof(localidadId));

            nombreApartado = MisUtils.NormalizarTexto(nombreApartado).Trim();
            var apartado = await _db.Table<Apartado>()
                                    .Where(a => a.Nombre.ToLower() == nombreApartado.ToLower()
                                             && a.LocalidadId == localidadId)
                                    .FirstOrDefaultAsync();
            return apartado != null;
        }
        public async Task<bool> ExisteApartadoAsync(string nombreApartado, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(nombreLocalidad));

            try
            {
                var _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
                if (_localidad != null)
                {
                    return await ExisteApartadoAsync(nombreApartado, _localidad.Id);
                }
            }
            catch
            {
                return false; 
            }
            return false;
        }
        public async Task<List<Apartado>> ObtenerApartadosAsync(int localidadId)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de localidad debe ser mayor que 0.", nameof(localidadId));
            return await _db.Table<Apartado>()
                            .Where(a => a.LocalidadId == localidadId)
                            .ToListAsync();
        }
        public async Task<List<Apartado>> ObtenerApartadosAsync(string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(nombreLocalidad));

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad) ?? throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            return await ObtenerApartadosAsync(_localidad.Id);
        }
        public async Task<Apartado> ObtenerApartadoAsync(int apartadoId)
        {
            if (apartadoId <= 0) throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));

            return await _db.Table<Apartado>()
                            .Where(a => a.Id == apartadoId)
                            .FirstOrDefaultAsync();
        }
        public async Task<Apartado> ObtenerApartadoAsync(string nombreApartado, int localidadId)
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (localidadId <= 0)
                throw new ArgumentException("El Id de localidad debe ser mayor que 0.", nameof(localidadId));

            nombreApartado = MisUtils.NormalizarTexto(nombreApartado).Trim();
            return await _db.Table<Apartado>()
                            .Where(a => a.Nombre == nombreApartado && a.LocalidadId == localidadId)
                            .FirstOrDefaultAsync();
        }
        public async Task<Apartado> ObtenerApartadoAsync(string nombreApartado, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("Una localidad es obligatoria.", nameof(nombreLocalidad));

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad) ?? throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            nombreApartado = MisUtils.NormalizarTexto(nombreApartado).Trim();
            return await _db.Table<Apartado>()
                            .Where(a => a.Nombre == nombreApartado && a.LocalidadId == _localidad.Id)
                            .FirstOrDefaultAsync();
        }
        public async Task ActualizarApartadoAsync(Apartado apartado)
        {
            if (apartado == null)
                throw new ArgumentNullException(nameof(apartado), "El apartado no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(apartado.Nombre))
                throw new ArgumentException("El nuevo nombre del apartado es obligatorio.", nameof(apartado.Nombre));

            try
            {
                apartado.FechaModificacion = DateTime.UtcNow;
                await _db.UpdateAsync(apartado);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar el apartado. {ex.Message}");
            }
        }
        public async Task ActualizarApartadoAsync(Apartado apartado, string nuevoNombre, string texto="")
        {
            if (apartado == null)
                throw new ArgumentNullException(nameof(apartado), "El apartado no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nuevo nombre del apartado es obligatorio.", nameof(nuevoNombre));

            try
            {
                Apartado _apartado = await ObtenerApartadoAsync(apartado.Id) ?? throw new InvalidOperationException("No se encontró el apartado ");
                _apartado.Nombre = nuevoNombre;
                _apartado.Texto = texto;
                _apartado.FechaModificacion = DateTime.UtcNow;

                await _db.UpdateAsync(_apartado);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo actualizar el apartado. {ex.Message}");
            }
        }
        public async Task<int> EliminarApartadoAsync(int apartadoId,bool confirmarBorrado = true)
        {
            if (apartadoId <= 0) throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));
            var apartado = await ObtenerApartadoAsync(apartadoId) ?? throw new InvalidOperationException("No se encontró el apartado.");

            var apartadoImagenes = await ObtenerImagenesPorEntidadAsync(TipoEntidad.Apartado, apartadoId);
            var apartadoNotas = await ObtenerNotasAsync(apartadoId);
            if (confirmarBorrado)
            {
                string texto = "¿Seguro que quieres borrar este apartado?";

                if (apartadoNotas != null)
                {
                    if (apartadoNotas.Count > 0)
                        texto = "Hay alguna(s) nota(s) asociada(s) a este apartado. " + texto;
                }
                if (apartadoImagenes != null)
                {
                    if (apartadoImagenes.Count>0)
                    {
                        texto = "Hay alguna(s) imagen(es) asociada(s) a este apartado. " + texto;
                    }
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
                if (apartadoNotas != null)
                    foreach (Nota nota in apartadoNotas)
                        await EliminarNotaAsync(nota.Id,false);
                if (apartadoImagenes != null)
                    foreach (MiImagen imagen in apartadoImagenes)
                        await EliminarImagenAsync(imagen.Id,false);
                return await _db.DeleteAsync<Apartado>(apartadoId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar el apartado.{ex.Message}");
            }
        }

        #endregion

        #region "Notas"
        public async Task<int> InsertarNotaAsync(string titulo, string texto, int apartadoId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (apartadoId <= 0)
                throw new ArgumentException("El Id del apartado debe ser mayor que cero.", nameof(apartadoId));

            bool apartadoExiste = await ExisteApartadoAsync(apartadoId);
            if (!apartadoExiste)
                throw new InvalidOperationException($"El apartado con Id '{apartadoId}' no existe.");

            bool existeNota = await ExisteNotaAsync(titulo, apartadoId);
            if (existeNota)
                throw new InvalidOperationException("Ya existe una nota con ese título en este apartado.");

            try
            {
                Nota nota = new(titulo, texto, apartadoId);
                await _db.InsertAsync(nota);
                return nota.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo crear la nota. {ex.Message}");
            }
        }
        public async Task<int> InsertarNotaAsync(string titulo, string texto, string nombreApartado, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("La localidad es obligatoria.", nameof(nombreLocalidad));

            var _localidad = await ObtenerLocalidadAsync(nombreLocalidad) ?? throw new InvalidOperationException("No se encontró la localidad con nombre '{localidad}'.");

            var _apartado = await ObtenerApartadoAsync(nombreApartado, _localidad.Id) ?? throw new InvalidOperationException($"No se encontró el apartado '{nombreApartado}' en la localidad '{nombreLocalidad}'.");

            bool existeNota = await ExisteNotaAsync(titulo, _apartado.Id);
            if (existeNota)
                throw new InvalidOperationException("Ya existe una nota con ese título en este apartado.");

            try
            {
                Nota nota = new(titulo, texto, _apartado.Id); 
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
        public async Task<bool> ExisteNotaAsync(string titulo, int apartadoId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (apartadoId <= 0)
                throw new ArgumentException("El Id del apartado debe ser mayor que cero.", nameof(apartadoId));

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            var nota = await _db.Table<Nota>()
                                .Where(n => n.Titulo.ToLower() == titulo.ToLower()
                                         && n.ApartadoId == apartadoId)
                                .FirstOrDefaultAsync();
            return nota != null;
        }
        public async Task<bool> ExisteNotaAsync(string titulo, string nombreApartado, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("La localidad es obligatoria.", nameof(nombreLocalidad));

            var _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad == null)
                throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            var _apartado = await ObtenerApartadoAsync(nombreApartado, _localidad.Nombre);
            if (_apartado == null)
                throw new InvalidOperationException($"No se encontró el apartado '{nombreApartado}'.");

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            var existeNota = await _db.Table<Nota>()
                                      .Where(n => n.Titulo.ToLower() == titulo.ToLower()
                                               && n.ApartadoId == _apartado.Id)
                                      .FirstOrDefaultAsync();
            return existeNota != null;
        }
        public async Task<List<Nota>> ObtenerNotasAsync(int apartadoId)
        {
            if (apartadoId <= 0) throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));
            return await _db.Table<Nota>()
                            .Where(a => a.ApartadoId == apartadoId)
                            .ToListAsync();
        }
        public async Task<Nota> ObtenerNotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));

            return await _db.Table<Nota>()
                            .Where(a => a.Id == notaId)
                            .FirstOrDefaultAsync();
        }
        public async Task<Nota> ObtenerNotaAsync(string titulo, int apartadoId)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El nombre del titulo es obligatorio.", nameof(titulo));
            if (apartadoId <= 0)
                throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));

            titulo = MisUtils.NormalizarTexto(titulo).Trim();
            return await _db.Table<Nota>()
                            .Where(a => a.Titulo.ToLower() == titulo.ToLower() && a.ApartadoId == apartadoId)
                            .FirstOrDefaultAsync();
        }
        public async Task<Nota?> ObtenerNotaAsync(string titulo, string nombreApartado, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("Una localidad es obligatoria.", nameof(nombreLocalidad));

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad != null)
            {
                var _apartado = await ObtenerApartadoAsync(nombreApartado, _localidad.Id);
                if (_apartado != null)
                {
                    titulo = MisUtils.NormalizarTexto(titulo).Trim();
                    return await _db.Table<Nota>().Where(a => a.Titulo.ToLower() == titulo.ToLower() && a.ApartadoId == _apartado.Id).FirstOrDefaultAsync();
                }
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
                _nota.Contenido = nuevoContenido;
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

            var notaImagenes = await ObtenerImagenesPorEntidadAsync(TipoEntidad.Nota,notaId);
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
                    foreach (MiImagen imagenNota in notaImagenes)
                        await EliminarImagenAsync(imagenNota.Id,false);
                return await _db.DeleteAsync<Nota>(notaId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la nota.{ex.Message}");
            }
        }

        #endregion

        #region "Imagenes"
        public async Task<int> InsertarImagensync(int entidadId, TipoEntidad tipoEntidad, byte[] byteArray,string nombre="",bool esMapa=false,string urlMapa="" )
        {
            if (entidadId <= 0)
                throw new ArgumentException("El Id de la localidad, apartado o nota debe ser mayor que cero.", nameof(entidadId));
            if (byteArray is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(byteArray));

            switch (tipoEntidad)
            {
                case TipoEntidad.Localidad:
                    bool localidadExiste = await ExisteLocalidadAsync(entidadId);
                    if (!localidadExiste)
                        throw new InvalidOperationException($"La localidad con Id '{entidadId}' no existe.");
                    break;
                case TipoEntidad.Apartado:
                    bool apartadoExiste = await ExisteApartadoAsync(entidadId);
                    if (!apartadoExiste)
                        throw new InvalidOperationException($"El apartado con Id '{entidadId}' no existe.");
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
                MiImagen imagen = new(entidadId, tipoEntidad, byteArray, nombre,esMapa,urlMapa); 
                await _db.InsertAsync(imagen);
                return imagen.Id;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"No se pudo insertar la imagen. {ex.Message}");
            }
        }

        public async Task<int> InsertarImagensync(MiImagen imagen)
        {
            if (imagen.EntidadId <= 0)
                throw new ArgumentException("El Id de la localidad, apartado o nota debe ser mayor que cero.", nameof(imagen.EntidadId));
            if (imagen.Foto is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(imagen.Foto));

            switch (imagen.TipoDeEntidad)
            {
                case TipoEntidad.Localidad:
                    bool localidadExiste = await ExisteLocalidadAsync(imagen.EntidadId);
                    if (!localidadExiste)
                        throw new InvalidOperationException($"La localidad con Id '{imagen.EntidadId}' no existe.");
                    break;
                case TipoEntidad.Apartado:
                    bool apartadoExiste = await ExisteApartadoAsync(imagen.EntidadId);
                    if (!apartadoExiste)
                        throw new InvalidOperationException($"El apartado con Id '{imagen.EntidadId}' no existe.");
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
        public async Task<MiImagen> ObtenerImagenAsync(int imagenId)
        {
            if (imagenId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenId));

            return await _db.Table<MiImagen>()
                            .Where(a => a.Id == imagenId)
                            .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteImagenAsync(int imagenId)
        {
            if (imagenId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenId));
            var imagen = await _db.FindAsync<MiImagen>(imagenId);
            return imagen != null;
        }
        public async Task<List<MiImagen>> ObtenerImagenesPorEntidadAsync(TipoEntidad tipoEntidad, int entidadId)
        {
            if (entidadId <= 0) throw new ArgumentException("El Id de la localidad, apartado o nota debe ser mayor que 0.", nameof(entidadId));
            return await _db.Table<MiImagen>()
                            .Where(a => a.TipoDeEntidad == tipoEntidad && a.EntidadId == entidadId)
                            .ToListAsync();
        }
        public async Task<int> EliminarImagenAsync(int imagenId,bool confirmarBorrado = true)
        {
            if (imagenId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenId));

            _ = await _db.FindAsync<MiImagen>(imagenId) ?? throw new InvalidOperationException("No se encontró la imagen.");

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
                return await _db.DeleteAsync<MiImagen>(imagenId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Hubo un problema al eliminar la imagen. {ex.Message}");
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
