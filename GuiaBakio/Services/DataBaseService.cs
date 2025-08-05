using GuiaBakio.Helpers;
using GuiaBakio.Models;
using SQLite;

namespace GuiaBakio.Services
{
    public class DataBaseService
    {
        private readonly SQLiteAsyncConnection _db;
        public DataBaseService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            var _ = InitTablesAsync();
        }
        public async Task InitTablesAsync()
        {
            await _db.CreateTableAsync<Localidad>();
            await _db.CreateTableAsync<Apartado>();
            await _db.CreateTableAsync<Nota>();
            await _db.CreateTableAsync<ImagenLocalidad>();
            await _db.CreateTableAsync<ImagenApartado>();
            await _db.CreateTableAsync<ImagenNota>();
        }

        #region "Localidades"
        public async Task InsertarLocalidadAsync(string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("El nombre de la localidad es obligatorio.", nameof(nombreLocalidad));

            bool localidadExiste = await ExisteLocalidadAsync(nombreLocalidad);
            if (localidadExiste)
                throw new InvalidOperationException("Ya existe una localidad con ese nombre.");

            Localidad _localidad = new(nombreLocalidad);
            await _db.InsertAsync(_localidad);
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
        public async Task ActualizarLocalidadAsync(Localidad localidad, string nuevoNombre, string texto="")
        {
            if (localidad == null)
                throw new ArgumentNullException(nameof(localidad), "La localidad no puede ser nula.");
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nuevo nombre de la localidad es obligatorio.", nameof(nuevoNombre));

            Localidad? _localidad = await ObtenerLocalidadAsync(localidad.Id) ?? throw new InvalidOperationException("No se encontró la localidad.");

            _localidad.Nombre = nuevoNombre;
            _localidad.Texto = texto;
            _localidad.FechaModificacion = DateTime.UtcNow;
            await _db.UpdateAsync(_localidad);
        }
        public async Task<int> EliminarLocalidadAsync(int localidadId)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));
            var localidad = await ObtenerLocalidadAsync(localidadId);
            if (localidad == null) throw new InvalidOperationException("No se encontró la localidad.");
            return await _db.DeleteAsync<Apartado>(localidadId);
        }

        #endregion

        #region "Apartados"
        public async Task InsertarApartadoAsync(string nombreApartado, int localidadId, string texto="")
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

            Apartado apartado = new(nombreApartado, localidadId, texto);
            await _db.InsertAsync(apartado);
        }
        public async Task InsertarApartadoAsync(string nombreApartado, string nombreLocalidad, string texto="")
        {
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El nombre del apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("Una localidad es obligatoria.", nameof(nombreLocalidad));

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad == null)
                throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            var existeApartado = await ExisteApartadoAsync(nombreApartado, nombreLocalidad);
            if (existeApartado)
                throw new InvalidOperationException("Ya existe un apartado con ese nombre en esta localidad.");

            Apartado apartado = new(nombreApartado, _localidad.Id,texto);
            await _db.InsertAsync(apartado);
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

            var _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad != null)
            {
                return await ExisteApartadoAsync(nombreApartado, _localidad.Id);
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

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad == null)
                throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");
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

            Localidad? _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad == null)
                throw new InvalidOperationException($"No se encontró la localidad '{nombreLocalidad}'.");

            nombreApartado = MisUtils.NormalizarTexto(nombreApartado).Trim();
            return await _db.Table<Apartado>()
                            .Where(a => a.Nombre == nombreApartado && a.LocalidadId == _localidad.Id)
                            .FirstOrDefaultAsync();
        }
        public async Task ActualizarApartadoAsync(Apartado apartado, string nuevoNombre, string texto="")
        {
            if (apartado == null)
                throw new ArgumentNullException(nameof(apartado), "El apartado no puede ser nulo.");
            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nuevo nombre del apartado es obligatorio.", nameof(nuevoNombre));

            Apartado _apartado = await ObtenerApartadoAsync(apartado.Id) ?? throw new InvalidOperationException("No se encontró el apartado ");
            _apartado.Nombre = nuevoNombre;
            _apartado.Texto = texto;    
            _apartado.FechaModificacion = DateTime.UtcNow;
            await _db.UpdateAsync(_apartado);
        }
        public async Task<int> EliminarApartadoAsync(int apartadoId)
        {
            if (apartadoId <= 0) throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));
            var apartado = await ObtenerApartadoAsync(apartadoId);
            if (apartado == null) throw new InvalidOperationException("No se encontró el apartado.");
            return await _db.DeleteAsync<Apartado>(apartadoId);
        }

        #endregion

        #region "Notas"
        public async Task InsertarNotaAsync(string titulo, string texto, int apartadoId)
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

            Nota nota = new(titulo, texto, apartadoId);
            await _db.InsertAsync(nota);
        }
        public async Task InsertarNotaAsync(string titulo, string texto, string nombreApartado, string nombreLocalidad)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título de la nota es obligatorio.", nameof(titulo));
            if (string.IsNullOrWhiteSpace(nombreApartado))
                throw new ArgumentException("El apartado es obligatorio.", nameof(nombreApartado));
            if (string.IsNullOrWhiteSpace(nombreLocalidad))
                throw new ArgumentException("La localidad es obligatoria.", nameof(nombreLocalidad));

            var _localidad = await ObtenerLocalidadAsync(nombreLocalidad);
            if (_localidad == null)
                throw new InvalidOperationException("No se encontró la localidad con nombre '{localidad}'.");

            var _apartado = await ObtenerApartadoAsync(nombreApartado, _localidad.Id);
            if (_apartado == null)
                throw new InvalidOperationException($"No se encontró el apartado '{nombreApartado}' en la localidad '{nombreLocalidad}'.");

            bool existeNota = await ExisteNotaAsync(titulo, _apartado.Id);
            if (existeNota)
                throw new InvalidOperationException("Ya existe una nota con ese título en este apartado.");

            Nota nota = new(titulo, texto, _apartado.Id);
            await _db.InsertAsync(nota);
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
        public async Task ActualizarNotaAsync(Nota nota, string nuevoTitulo, string nuevoContenido)
        {
            if (nota == null)
                throw new ArgumentNullException(nameof(nota), "La nota no puede ser nula.");
            if (string.IsNullOrWhiteSpace(nuevoTitulo))
                throw new ArgumentException("El nuevo título de la nota es obligatorio.", nameof(nuevoTitulo));

            Nota _nota = await ObtenerNotaAsync(nota.Id);
            if (_nota == null) throw new InvalidOperationException("No se encontró la nota.");

            _nota.Titulo = nuevoTitulo;
            _nota.Contenido = nuevoContenido;
            _nota.FechaModificacion = DateTime.UtcNow;
            await _db.UpdateAsync(_nota);
        }
        public async Task<int> EliminarNotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            var nota = await ObtenerNotaAsync(notaId);
            if (nota == null) throw new InvalidOperationException("No se encontró la nota.");
            return await _db.DeleteAsync<Nota>(notaId);
        }

        #endregion

        #region "ImagenesLocalidad"
        public async Task InsertarImagenLocalidadAsync(int localidadId, byte[] byteArray,string nombre="")
        {
            if (localidadId <= 0)
                throw new ArgumentException("El Id de la localidad debe ser mayor que cero.", nameof(localidadId));
            if (byteArray is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(byteArray));
            bool localidadExiste = await ExisteLocalidadAsync(localidadId);
            if (!localidadExiste)
                throw new InvalidOperationException($"La localidad con Id '{localidadId}' no existe.");

            ImagenLocalidad imagen = new(localidadId, byteArray, nombre);
            await _db.InsertAsync(imagen);
        }
        public async Task<ImagenLocalidad> ObtenerImagenLocalidadAsync(int imagenLocalidadId)
        {
            if (imagenLocalidadId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenLocalidadId));

            return await _db.Table<ImagenLocalidad>()
                            .Where(a => a.Id == imagenLocalidadId)
                            .FirstOrDefaultAsync();
        }
        public async Task<List<ImagenLocalidad>> ObtenerImagenesPorLocalidadAsync(int localidadId)
        {
            if (localidadId <= 0) throw new ArgumentException("El Id de la localidad debe ser mayor que 0.", nameof(localidadId));
            return await _db.Table<ImagenLocalidad>()
                            .Where(a => a.LocalidadId == localidadId)
                            .ToListAsync();
        }
        public async Task<int> EliminarImagenLocalidadAsync(int ImagenLocalidadId)
        {
            if (ImagenLocalidadId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(ImagenLocalidadId));

            var ImagenLocalidad = await ObtenerImagenLocalidadAsync(ImagenLocalidadId);
            if (ImagenLocalidad == null) throw new InvalidOperationException("No se encontró la imagen.");

            return await _db.DeleteAsync<ImagenLocalidad>(ImagenLocalidadId);
        }

        #endregion

        #region "ImagenesApartado"
        public async Task InsertarImagenApartadoAsync(int apartadoId, byte[] byteArray, string nombre = "")
        {
            if (apartadoId <= 0)
                throw new ArgumentException("El Id del apartado debe ser mayor que cero.", nameof(apartadoId));
            if (byteArray is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(byteArray));
            bool apartadoExiste = await ExisteApartadoAsync(apartadoId);
            if (!apartadoExiste)
                throw new InvalidOperationException($"El apartado con Id '{apartadoId}' no existe.");

            ImagenApartado imagen = new(apartadoId, byteArray, nombre);
            await _db.InsertAsync(imagen);
        }
        public async Task<ImagenApartado> ObtenerimagenApartadoAsync(int imagenApartadoId)
        {
            if (imagenApartadoId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenApartadoId));

            return await _db.Table<ImagenApartado>()
                            .Where(a => a.Id == imagenApartadoId)
                            .FirstOrDefaultAsync();
        }
        public async Task<List<ImagenApartado>> ObtenerImagenesPorApartadoAsync(int apartadoId)
        {
            if (apartadoId <= 0) throw new ArgumentException("El Id del apartado debe ser mayor que 0.", nameof(apartadoId));
            return await _db.Table<ImagenApartado>()
                            .Where(a => a.ApartadoId == apartadoId)
                            .ToListAsync();
        }
        public async Task<int> EliminarImagenApartadoAsync(int imagenApartadoId)
        {
            if (imagenApartadoId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenApartadoId));

            var imagenApartado = await ObtenerimagenApartadoAsync(imagenApartadoId);
            if (imagenApartado == null) throw new InvalidOperationException("No se encontró la imagen.");

            return await _db.DeleteAsync<ImagenApartado>(imagenApartadoId);
        }
        #endregion

        #region "ImagenesNota"
        public async Task InsertarImagenNotaAsync(int notaId, byte[] byteArray, string nombre = "")
        {
            if (notaId <= 0)
                throw new ArgumentException("El Id de la nota debe ser mayor que cero.", nameof(notaId));
            if (byteArray is null)
                throw new ArgumentException("La imagen no puede ser null.", nameof(byteArray));
            bool notaExiste = await ExisteNotaAsync(notaId);
            if (!notaExiste)
                throw new InvalidOperationException($"La nota con Id '{notaId}' no existe.");

            ImagenNota imagen = new(notaId, byteArray, nombre);
            await _db.InsertAsync(imagen);
        }
        public async Task<ImagenNota> ObtenerimagenNotaAsync(int imagenNotaId)
        {
            if (imagenNotaId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenNotaId));

            return await _db.Table<ImagenNota>()
                            .Where(a => a.Id == imagenNotaId)
                            .FirstOrDefaultAsync();
        }
        public async Task<List<ImagenNota>> ObtenerImagenesPorNotaAsync(int notaId)
        {
            if (notaId <= 0) throw new ArgumentException("El Id de la nota debe ser mayor que 0.", nameof(notaId));
            return await _db.Table<ImagenNota>()
                            .Where(a => a.NotaId == notaId)
                            .ToListAsync();
        }
        public async Task<int> EliminarImagenNotaAsync(int imagenNotaId)
        {
            if (imagenNotaId <= 0) throw new ArgumentException("El Id de la imagen debe ser mayor que 0.", nameof(imagenNotaId));
            var imagenNota = await ObtenerimagenNotaAsync(imagenNotaId);
            if (imagenNota == null) throw new InvalidOperationException("No se encontró la imagen.");
            return await _db.DeleteAsync<ImagenNota>(imagenNotaId);
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
