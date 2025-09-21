using GuiaBakio.Helpers;
using GuiaBakio.Models;
using SQLite;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GuiaBakio.Services
{
    public class ApiService(HttpClient httpClient, SQLiteAsyncConnection db)
    {
        private const string BaseUrl = "https://glidercm.net/api/";

        private async Task<bool> PostAsync<T>(string endpoint, List<T> payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }

        private async Task GuardarEstadoSincronizacionAsync(TipoSincronizacion tipoSincronizacion, bool fueExitosa, string error = "")
        {
            try
            {
                var estado = new EstadoSincronizacion
                {
                    Tipo = tipoSincronizacion,
                    UltimoIntento = DateTime.UtcNow,
                    FueExitosa = fueExitosa,
                    Error = error
                };
                await db.InsertOrReplaceAsync(estado);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al guardar el estado de sincronización. {ex.Message}");
            }
        }

        public async Task<string> SincronizarDescendenteAsync()
        {
            var resultados = new Dictionary<string, bool>();

            resultados["Usuarios"] = await BajarUsuariosAsync();
            resultados["Localidades"] = await BajarLocalidadesAsync();
            resultados["Notas"] = await BajarNotasAsync();
            resultados["Etiquetas"] = await BajarEtiquetasAsync();
            resultados["NotaEtiquetas"] = await BajarNotaEtiquetasAsync();
            resultados["Fotos"] = await BajarFotosAsync();

            var todoOk = resultados.Values.All(r => r);
            await GuardarEstadoSincronizacionAsync(TipoSincronizacion.Descendente, todoOk);

            if (!todoOk)
            {
                var fallos = resultados.Where(kvp => !kvp.Value).Select(kvp => kvp.Key);
                var mensaje = $"Falló la sincronización descendente de: {string.Join(", ", fallos)}";
                Debug.WriteLine(mensaje);
                return mensaje;
            }
            return "OK";

        }

        public async Task<string> SincronizarAscendenteAsync()
        {
            var resultados = new Dictionary<string, bool>();

            resultados["Usuarios"] = await SubirUsuariosAsync();
            resultados["Localidades"] = await SubirLocalidadesAsync();
            resultados["Notas"] = await SubirNotasAsync();
            resultados["Etiquetas"] = await SubirEtiquetasAsync();
            resultados["NotaEtiquetas"] = await SubirNotaEtiquetasAsync();
            resultados["Fotos"] = await SubirFotosAsync();
            resultados["LocalidadesEliminadas"] = await MandarLocalidadesEliminadas();
            resultados["NotasEliminadas"] = await MandarNotasEliminadas();
            resultados["EtiquetasEliminadas"] = await MandarEtiquetasEliminadas();
            resultados["NotaEtiquetasEliminadas"] = await MandarNotaEtiquetasEliminadas();
            resultados["FotosEliminadas"] = await MandarFotosEliminadas();

            var todoOk = resultados.Values.All(r => r);
            await GuardarEstadoSincronizacionAsync(TipoSincronizacion.Ascendente, todoOk);


            if (!todoOk)
            {
                var fallos = resultados.Where(kvp => !kvp.Value).Select(kvp => kvp.Key);
                var mensaje = $"Falló la sincronización ascendente de: {string.Join(", ", fallos)}";
                Debug.WriteLine(mensaje);
                return mensaje;
            }
            return "OK";
        }

        #region "Subir"
        private async Task<bool> SubirEtiquetasAsync()
        {
            try
            {
                var etiquetas = await db.Table<Etiqueta>()
                    .Where(e => !e.Sincronizado)
                    .ToListAsync();

                if (etiquetas.Count == 0)
                    return true; // Nada que subir, pero no es un error

                var dtoList = etiquetas.Select(ApiMapper.EtiquetaToDto).ToList();
                var success = await PostAsync($"{BaseUrl}etiquetas/sincronizar", dtoList);

                if (!success)
                    return false;

                foreach (var etiqueta in etiquetas)
                {
                    etiqueta.Sincronizado = true;
                    await db.UpdateAsync(etiqueta);
                }
                Debug.WriteLine("Etiquetas subidas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al subir etiquetas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SubirLocalidadesAsync()
        {
            try
            {
                var localidades = await db.Table<Localidad>()
                    .Where(e => !e.Sincronizado)
                    .ToListAsync();

                if (localidades.Count == 0)
                    return true; // Nada que subir, pero no es un error

                var dtoList = localidades.Select(ApiMapper.LocalidadToDto).ToList();
                var success = await PostAsync($"{BaseUrl}localidades/sincronizar", dtoList);

                if (!success)
                    return false;

                foreach (var localidad in localidades)
                {
                    localidad.Sincronizado = true;
                    await db.UpdateAsync(localidad);
                }
                Debug.WriteLine("Localidades subidas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al subir localidades. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SubirNotasAsync()
        {
            try
            {
                var notas = await db.Table<Nota>()
                    .Where(e => !e.Sincronizado)
                    .ToListAsync();

                if (notas.Count == 0)
                    return true; // Nada que subir, pero no es un error

                var dtoList = notas.Select(ApiMapper.NotaToDto).ToList();
                var success = await PostAsync($"{BaseUrl}notas/sincronizar", dtoList);

                if (!success)
                    return false;

                foreach (var nota in notas)
                {
                    nota.Sincronizado = true;
                    await db.UpdateAsync(nota);
                }
                Debug.WriteLine("Notas subidas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al subir notas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SubirNotaEtiquetasAsync()
        {
            try
            {
                var notaEtiquetas = await db.Table<NotaEtiqueta>()
                    .Where(e => !e.Sincronizado)
                    .ToListAsync();

                if (notaEtiquetas.Count == 0)
                    return true; // Nada que subir, pero no es un error

                var dtoList = notaEtiquetas.Select(ApiMapper.NotaEtiquetaToDto).ToList();
                var success = await PostAsync($"{BaseUrl}api/notaetiquetas/sincronizar", dtoList);

                if (!success)
                    return false;

                foreach (var notaEtiqueta in notaEtiquetas)
                {
                    notaEtiqueta.Sincronizado = true;
                    await db.UpdateAsync(notaEtiqueta);
                }
                Debug.WriteLine("NotaEtiquetas subidas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al subir notaEtiquetas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SubirFotosAsync()
        {
            try
            {
                var fotos = await db.Table<Foto>()
                    .Where(e => !e.Sincronizado)
                    .ToListAsync();

                if (fotos.Count == 0)
                    return true; // Nada que subir, pero no es un error

                var dtoList = fotos.Select(ApiMapper.FotoToDto).ToList();
                var success = await PostAsync($"{BaseUrl}fotos/sincronizar", dtoList);

                if (!success)
                    return false;

                foreach (var foto in fotos)
                {
                    foto.Sincronizado = true;
                    await db.UpdateAsync(foto);
                }
                Debug.WriteLine("Fotos subidas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al subir fotos. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> SubirUsuariosAsync()
        {
            try
            {
                var usuarios = await db.Table<Usuario>()
                    .Where(e => !e.Sincronizado)
                    .ToListAsync();

                if (usuarios.Count == 0)
                    return true; // Nada que subir, pero no es un error

                var dtoList = usuarios.Select(ApiMapper.UsuarioToDto).ToList();
                var success = await PostAsync($"{BaseUrl}usuarios/sincronizar", dtoList);

                if (!success)

                    foreach (var usuario in usuarios)
                    {
                        usuario.Sincronizado = true;
                        await db.UpdateAsync(usuario);
                    }
                Debug.WriteLine("Usuarios subidos correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al subir usuarios. {ex.Message}");
                return false;
            }
        }

        #endregion

        #region "Eliminadas"

        private async Task<bool> MandarLocalidadesEliminadas()
        {
            try
            {
                var entidadesEliminadas = await db.Table<EntidadEliminada>()
                    .Where(n => n.Tipo == TipoEntidad.Localidad)
                    .ToListAsync();

                if (entidadesEliminadas.Count == 0)
                    return true; // Nada que eliminar, pero no es un error

                var idsEliminados = entidadesEliminadas.Select(e => e.Id).ToList();

                var success = await PostAsync($"{BaseUrl}localidades/eliminar", idsEliminados);
                if (!success)
                    return false;

                foreach (var item in entidadesEliminadas)
                    await db.DeleteAsync(item);
                Debug.WriteLine("Subida lista de localidades eliminadas correctamente.");
                return true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar localidades eliminadas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> MandarNotasEliminadas()
        {
            try
            {
                var entidadesEliminadas = await db.Table<EntidadEliminada>()
                    .Where(n => n.Tipo == TipoEntidad.Nota)
                    .ToListAsync();

                if (entidadesEliminadas.Count == 0)
                    return true; // Nada que eliminar

                var idsEliminados = entidadesEliminadas.Select(e => e.Id).ToList();

                var success = await PostAsync($"{BaseUrl}notas/eliminar", idsEliminados);
                if (!success)
                    return false;

                foreach (var item in entidadesEliminadas)
                    await db.DeleteAsync(item);
                Debug.WriteLine("Subida lista de notas eliminadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar notas eliminadas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> MandarFotosEliminadas()
        {
            try
            {
                var entidadesEliminadas = await db.Table<EntidadEliminada>()
                    .Where(n => n.Tipo == TipoEntidad.Foto)
                    .ToListAsync();

                if (entidadesEliminadas.Count == 0)
                    return true; // Nada que eliminar

                var idsEliminados = entidadesEliminadas.Select(e => e.Id).ToList();

                var success = await PostAsync($"{BaseUrl}fotos/eliminar", idsEliminados);
                if (!success)
                    return false;

                foreach (var item in entidadesEliminadas)
                    await db.DeleteAsync(item);
                Debug.WriteLine("Subida lista de fotos eliminadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar fotos eliminadas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> MandarEtiquetasEliminadas()
        {
            try
            {
                var entidadesEliminadas = await db.Table<EntidadEliminada>()
                    .Where(n => n.Tipo == TipoEntidad.Etiqueta)
                    .ToListAsync();

                if (entidadesEliminadas.Count == 0)
                    return true; // Nada que eliminar

                var idsEliminados = entidadesEliminadas.Select(e => e.Id).ToList();

                var success = await PostAsync($"{BaseUrl}etiquetas/eliminar", idsEliminados);
                if (!success)
                    return false;

                foreach (var item in entidadesEliminadas)
                    await db.DeleteAsync(item);
                Debug.WriteLine("Subida lista de etiquetas eliminadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar etiquetas eliminadas. {ex.Message}");
                return false;
            }
        }

        private async Task<bool> MandarNotaEtiquetasEliminadas()
        {
            try
            {
                var entidadesEliminadas = await db.Table<EntidadEliminada>()
                    .Where(n => n.Tipo == TipoEntidad.NotaEtiqueta)
                    .ToListAsync();

                if (entidadesEliminadas.Count == 0)
                    return true; // Nada que eliminar

                var idsEliminados = entidadesEliminadas.Select(e => e.Id).ToList();

                var success = await PostAsync($"{BaseUrl}notaetiquetas/eliminar", idsEliminados);
                if (!success)
                    return false;
                foreach (var item in entidadesEliminadas)
                    await db.DeleteAsync(item);
                Debug.WriteLine("Subida lista de notaEtiquetas eliminadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar notaEtiquetas eliminadas. {ex.Message}");
                return false;
            }
        }

        #endregion

        #region "Bajar"

        private async Task<bool> BajarUsuariosAsync()
        {
            try
            {
                var registro = await db.FindAsync<RegistroSincronizacion>(TipoEntidad.Usuario);
                var fechaSync = registro?.FechaModificacion ?? DateTime.MinValue;
                var url = $"{BaseUrl}usuarios/actualizadas-desde/{fechaSync:o}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false; ;

                var json = await response.Content.ReadAsStringAsync();
                var nuevosUsuarios = JsonSerializer.Deserialize<List<UsuarioDto>>(json);

                if (nuevosUsuarios != null)
                {
                    foreach (var usuario in nuevosUsuarios)
                    {
                        var usuarioLocal = await db.FindAsync<Usuario>(usuario.Id);
                        if (usuarioLocal == null)
                            await db.InsertAsync(ApiMapper.DtoToUsuario(usuario));
                        else if (usuario.FechaModificacion > usuarioLocal.FechaModificacion)
                        {
                            usuarioLocal.Nombre = usuario.Nombre;
                            usuarioLocal.FechaModificacion = usuario.FechaModificacion;
                            await db.UpdateAsync(usuarioLocal);
                        }
                    }

                    // Actualizar fecha de sincronización
                    await db.InsertOrReplaceAsync(new RegistroSincronizacion
                    {
                        Entidad = TipoEntidad.Usuario,
                        FechaModificacion = DateTime.UtcNow
                    });
                }
                Debug.WriteLine("Usuarios sincronizados correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar usuarios. {ex.Message}");
                return false;
            }
        }
        private async Task<bool> BajarLocalidadesAsync()
        {
            try
            {
                var registro = await db.FindAsync<RegistroSincronizacion>(TipoEntidad.Localidad);
                var fechaSync = registro?.FechaModificacion ?? DateTime.MinValue;
                var url = $"{BaseUrl}localidades/actualizadas-desde/{fechaSync:o}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var nuevasLocalidades = JsonSerializer.Deserialize<List<LocalidadDto>>(json);

                if (nuevasLocalidades != null)
                {
                    foreach (var localidad in nuevasLocalidades)
                    {
                        var localidadLocal = await db.FindAsync<Localidad>(localidad.Id);
                        if (localidadLocal == null)
                            await db.InsertAsync(ApiMapper.DtoToLocalidad(localidad));
                        else if (localidad.FechaModificacion > localidadLocal.FechaModificacion)
                        {
                            localidadLocal.Texto = localidad.Texto;
                            localidadLocal.FechaModificacion = localidad.FechaModificacion;
                            await db.UpdateAsync(localidadLocal);
                        }
                    }

                    // Actualizar fecha de sincronización
                    await db.InsertOrReplaceAsync(new RegistroSincronizacion
                    {
                        Entidad = TipoEntidad.Localidad,
                        FechaModificacion = DateTime.UtcNow
                    });
                }
                Debug.WriteLine("Localidades sincronizadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar localidades. {ex.Message}");
                return false;
            }
        }
        private async Task<bool> BajarNotasAsync()
        {
            try
            {
                var registro = await db.FindAsync<RegistroSincronizacion>(TipoEntidad.Nota);
                var fechaSync = registro?.FechaModificacion ?? DateTime.MinValue;
                var url = $"{BaseUrl}notas/actualizadas-desde/{fechaSync:o}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var nuevasNotas = JsonSerializer.Deserialize<List<NotaDto>>(json);

                if (nuevasNotas != null)
                {
                    foreach (var nota in nuevasNotas)
                    {
                        var notaLocal = await db.FindAsync<Nota>(nota.Id);
                        if (notaLocal == null)
                            await db.InsertAsync(ApiMapper.DtoToNota(nota));
                        else if (nota.FechaModificacion > notaLocal.FechaModificacion)
                        {
                            notaLocal.Texto = nota.Texto;
                            notaLocal.FechaModificacion = nota.FechaModificacion;
                            await db.UpdateAsync(notaLocal);
                        }
                    }

                    // Actualizar fecha de sincronización
                    await db.InsertOrReplaceAsync(new RegistroSincronizacion
                    {
                        Entidad = TipoEntidad.Nota,
                        FechaModificacion = DateTime.UtcNow
                    });
                }
                Debug.WriteLine("Notas sincronizadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar notas. {ex.Message}");
                return false;
            }
        }
        private async Task<bool> BajarEtiquetasAsync()
        {
            try
            {
                var registro = await db.FindAsync<RegistroSincronizacion>(TipoEntidad.Etiqueta);
                var fechaSync = registro?.FechaModificacion ?? DateTime.MinValue;
                var url = $"{BaseUrl}etiquetas/actualizadas-desde/{fechaSync:o}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var nuevasEtiquetas = JsonSerializer.Deserialize<List<EtiquetaDto>>(json);

                if (nuevasEtiquetas != null)
                {
                    foreach (var etiqueta in nuevasEtiquetas)
                    {
                        var etiquetaLocal = await db.FindAsync<Etiqueta>(etiqueta.Id);
                        if (etiquetaLocal == null)
                            await db.InsertAsync(ApiMapper.DtoToEtiqueta(etiqueta));
                        else if (etiqueta.FechaModificacion > etiquetaLocal.FechaModificacion)
                        {
                            etiquetaLocal.Nombre = etiqueta.Nombre;
                            etiquetaLocal.Icono = etiqueta.Icono;
                            etiquetaLocal.FechaModificacion = etiqueta.FechaModificacion;
                            await db.UpdateAsync(etiquetaLocal);
                        }
                    }

                    // Actualizar fecha de sincronización
                    await db.InsertOrReplaceAsync(new RegistroSincronizacion
                    {
                        Entidad = TipoEntidad.Etiqueta,
                        FechaModificacion = DateTime.UtcNow
                    });
                }
                Debug.WriteLine("Etiquetas sincronizadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar etiquetas. {ex.Message}");
                return false;
            }
        }
        private async Task<bool> BajarFotosAsync()
        {
            try
            {
                var registro = await db.FindAsync<RegistroSincronizacion>(TipoEntidad.Foto);
                var fechaSync = registro?.FechaModificacion ?? DateTime.MinValue;
                var url = $"{BaseUrl}fotos/actualizadas-desde/{fechaSync:o}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var nuevasFotos = JsonSerializer.Deserialize<List<FotoDto>>(json);

                if (nuevasFotos != null)
                {
                    foreach (var foto in nuevasFotos)
                    {
                        var fotoLocal = await db.FindAsync<Foto>(foto.Id);
                        if (fotoLocal == null)
                            await db.InsertAsync(ApiMapper.DtoToFoto(foto));
                        else if (foto.FechaModificacion > fotoLocal.FechaModificacion)
                        {
                            fotoLocal.Nombre = foto.Nombre;
                            fotoLocal.Blob = foto.Blob;
                            fotoLocal.EsMapa = foto.EsMapa;
                            fotoLocal.UrlMapa = foto.UrlMapa;
                            fotoLocal.FechaModificacion = foto.FechaModificacion;
                            await db.UpdateAsync(fotoLocal);
                        }
                    }

                    // Actualizar fecha de sincronización
                    await db.InsertOrReplaceAsync(new RegistroSincronizacion
                    {
                        Entidad = TipoEntidad.Foto,
                        FechaModificacion = DateTime.UtcNow
                    });
                }
                Debug.WriteLine("Fotos sincronizadas correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al sincronizar fotos. {ex.Message}");
                return false;
            }
        }
        private async Task<bool> BajarNotaEtiquetasAsync()
        {
            try
            {
                var registro = await db.FindAsync<RegistroSincronizacion>(TipoEntidad.NotaEtiqueta);
                var fechaSync = registro?.FechaModificacion ?? DateTime.MinValue;
                var url = $"{BaseUrl}notaetiquetas/actualizadas-desde/{fechaSync:o}";

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                var nuevasNotaEtiquetas = JsonSerializer.Deserialize<List<NotaEtiquetaDto>>(json);

                if (nuevasNotaEtiquetas != null)
                {
                    foreach (var notaEtiqueta in nuevasNotaEtiquetas)
                    {
                        var etiquetaLocal = await db.FindAsync<NotaEtiqueta>(notaEtiqueta.Id);
                        if (etiquetaLocal == null)
                            await db.InsertAsync(ApiMapper.DtoToEtiqueta(notaEtiqueta));
                        else if (notaEtiqueta.FechaModificacion > etiquetaLocal.FechaModificacion)
                        {
                            etiquetaLocal.NotaId = notaEtiqueta.NotaId;
                            etiquetaLocal.EtiquetaId = notaEtiqueta.EtiquetaId;
                            etiquetaLocal.FechaModificacion = notaEtiqueta.FechaModificacion;
                            await db.UpdateAsync(etiquetaLocal);
                        }
                    }

                    // Actualizar fecha de sincronización
                    await db.InsertOrReplaceAsync(new RegistroSincronizacion
                    {
                        Entidad = TipoEntidad.NotaEtiqueta,
                        FechaModificacion = DateTime.UtcNow
                    });
                }
                Debug.WriteLine("NotaEtiquetas sincronizadas correctamente.");
                return false;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al sincronizar notaEtiquetas. {ex.Message}");
            }
        }

        #endregion
    }
}
