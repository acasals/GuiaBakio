using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using SQLite;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace GuiaBakio.ViewModels
{
    public partial class ListaNotasViewModel : ObservableObject
    {
        private readonly DataBaseService _dbService;
        private readonly IAddItemPopupService _addItemPopupService;
        private readonly IDialogOKService _dialogService;
        private readonly INavigationDataService _navigationDataService;
        private readonly SQLiteAsyncConnection _db;
        private readonly ApiService _apiService;

        private DateTime? ultimaSincronizacion;
        private string _usuarioId;
        private Usuario? usuario;

        public IRelayCommand AgregarNotaAsyncCommand { get; }
        public IRelayCommand AgregarLocalidadAsyncCommand { get; }

        public IRelayCommand ToggleEtiquetaCommand => new RelayCommand<Etiqueta>(async etiqueta =>
        {
            etiqueta?.IsSelected = !etiqueta.IsSelected;
            EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
            NotasFiltradas = (await _dbService.ObtenerNotasAsync(EtiquetasSeleccionadas.ToList(), LocalidadesSeleccionadas.ToList())).ToObservableCollection();
        });

        public IRelayCommand ToggleLocalidadCommand => new RelayCommand<Localidad>(async localidad =>
        {
            if (localidad == null || localidad.IsButton)
                return; // <- ignoramos el botón especial

            localidad?.IsSelected = !localidad.IsSelected;
            LocalidadesSeleccionadas = Localidades.Where(e => e.IsSelected).ToObservableCollection();
            NotasFiltradas = (await _dbService.ObtenerNotasAsync(EtiquetasSeleccionadas.ToList(), LocalidadesSeleccionadas.ToList())).ToObservableCollection();
        });

        [ObservableProperty]
        private ObservableCollection<Nota> notas = new();

        [ObservableProperty]
        private ObservableCollection<Etiqueta> etiquetas = new();

        [ObservableProperty]
        private ObservableCollection<Etiqueta> etiquetasSeleccionadas = new();

        [ObservableProperty]
        private ObservableCollection<Localidad> localidades = new();

        [ObservableProperty]
        private ObservableCollection<Localidad> localidadesSeleccionadas = new();

        [ObservableProperty]
        private ObservableCollection<Nota> notasFiltradas = new();

        [ObservableProperty]
        private bool noHayNotas;

        public ListaNotasViewModel(
             DataBaseService dbService,
             IAddItemPopupService addItemPopupService,
             IDialogOKService dialogService,
             INavigationDataService navigationDataService,
             SQLiteAsyncConnection db,
             ApiService apiService)
        {
            AgregarNotaAsyncCommand = new AsyncRelayCommand(AgregarNotaAsync);
            AgregarLocalidadAsyncCommand = new AsyncRelayCommand(AgregarLocalidadAsync);
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _addItemPopupService = addItemPopupService ?? throw new ArgumentNullException(nameof(addItemPopupService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _navigationDataService = navigationDataService ?? throw new ArgumentNullException(nameof(navigationDataService));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));

            _ = CheckUsuario();
        }

        private async Task CheckUsuario()
        {
            try
            {
                string usuarioId = Preferences.Get("UsuarioId", "");
                if (!string.IsNullOrWhiteSpace(usuarioId))
                {
                    _usuarioId = usuarioId;
                    usuario = await _dbService.ObtenerUsuarioPorIdAsync(_usuarioId);
                    if (usuario is null)
                    {
                        await _dialogService.ShowAlertAsync("Error", "Hubo un problema al localizar al usuario. Contacta con Alex.", "OK");
                        return;
                    }
                    _navigationDataService.Data = usuario;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo obtener el usuario.", ex);
            }
        }

        public async Task OnAppearedOrResumed()
        {
            try
            {
                await CargarListaEtiquetasAsync();
                await CargarListaLocalidadesAsync();
                await CargarListaNotasAsync();
                //await SincronizarSiNoRecienteAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo obtener la lista de notas: {Environment.NewLine}{ex.Message}", "OK");
            }
        }

        public async Task CargarListaEtiquetasAsync()
        {
            try
            {
                Etiquetas = new ObservableCollection<Etiqueta>(
                     await _dbService.ObtenerEtiquetasAsync());
                EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar la lista de etiquetas.", ex);
            }
        }

        public async Task CargarListaLocalidadesAsync()
        {
            try
            {
                Localidades = new ObservableCollection<Localidad>(
                    await _dbService.ObtenerLocalidadesAsync());
                LocalidadesSeleccionadas = Localidades.Where(e => e.IsSelected).ToObservableCollection();
                // Añadimos el ítem especial al final
                Localidades.Add(new Localidad
                {
                    Nombre = "Añadir localidad",
                    IsButton = true
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar la lista de localidades.", ex);
            }
        }

        private async Task SincronizarSiNoRecienteAsync()
        {
            if (ultimaSincronizacion == null ||
                DateTime.UtcNow - ultimaSincronizacion > TimeSpan.FromMinutes(60))
            {
                string resultado = "";
                try
                {
                    var estado = _db.FindAsync<EstadoSincronizacion>("Descendente");
                    if (estado.Result == null || !estado.Result.FueExitosa)
                        resultado = await Task.Run(() => _apiService.SincronizarDescendenteAsync());
                }
                catch (Exception ex)
                {
                    resultado = ex.Message;
                }
                if (resultado != "OK")
                    await _dialogService.ShowAlertAsync("Error", $"No se pudieron recibir los datos  del servidor: {Environment.NewLine}{resultado}", "OK");
                else
                {
                    Debug.WriteLine("Sincronización descendente correcta.");
                    resultado = "";
                    try
                    {
                        var estado = _db.FindAsync<EstadoSincronizacion>("Ascendente");
                        if (estado.Result == null || !estado.Result.FueExitosa)
                            resultado = await Task.Run(() => _apiService.SincronizarAscendenteAsync());
                    }
                    catch (Exception ex)
                    {
                        resultado = ex.Message;
                    }
                    if (resultado != "OK")
                        await _dialogService.ShowAlertAsync("Error", $"No se pudieron subir los datos al servidor: {Environment.NewLine}{resultado}", "OK");
                }
                ultimaSincronizacion = DateTime.UtcNow;
            }
        }

        public async Task CargarListaNotasAsync()
        {
            try
            {
                var lista = await _dbService.ObtenerNotasAsync(EtiquetasSeleccionadas.ToList(), LocalidadesSeleccionadas.ToList());
                NotasFiltradas = new ObservableCollection<Nota>(lista);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar la lista de notas.", ex);
            }
        }

        [RelayCommand]
        public async Task AgregarNotaAsync()
        {
            var nuevaNota = await _addItemPopupService.MostrarAsync("Añade una nota");
            if (nuevaNota is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(nuevaNota))
            {
                await _dialogService.ShowAlertAsync("Error", "El nombre del nota no puede estar vacío.", "OK");
                return;
            }
            try
            {
                bool yaExiste = await _dbService.ExisteNotaPorTituloAsync(nuevaNota);
                if (yaExiste)
                {
                    await _dialogService.ShowAlertAsync("Error", "Nota existente.", "OK");
                    return;
                }
                if (usuario is null)
                {
                    await _dialogService.ShowAlertAsync("Error", "No se ha encontrado el usuario. Contacta con Alex.", "OK");
                    return;
                }
                var id = await _dbService.InsertarNotaAsync(nuevaNota, usuario.Id);
                if (string.IsNullOrWhiteSpace(id))
                {
                    await _dialogService.ShowAlertAsync("Error", "No se pudo añadir la nota.", "OK");
                    return;
                }
                try
                {
                    await Shell.Current.GoToAsync($"notaPage?Id={id}");
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Error", $"No se pudo navegar a la página de la nota.{Environment.NewLine}{ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir el nota.{Environment.NewLine}{ex.Message}", "OK");
            }

        }

        [RelayCommand]
        public async Task AgregarLocalidadAsync()
        {
            {
                var nuevaLocalidad = await _addItemPopupService.MostrarAsync("Añade una localidad");
                if (nuevaLocalidad is null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(nuevaLocalidad))
                {
                    await _dialogService.ShowAlertAsync("Error", "El nombre de la localidad no puede estar vacío.", "OK");
                    return;
                }
                try
                {
                    bool yaExiste = await _dbService.ExisteLocalidadConNombreAsync(nuevaLocalidad);
                    if (yaExiste)
                    {
                        await _dialogService.ShowAlertAsync("Error", "Localidad existente.", "OK");
                        return;
                    }
                    var id = await _dbService.InsertarLocalidadAsync(nuevaLocalidad, _usuarioId);
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        await _dialogService.ShowAlertAsync("Error", "No se pudo añadir la localidad.", "OK");
                        return;
                    }
                    try
                    {
                        await Shell.Current.GoToAsync($"localidadPage?Id={id}");
                    }
                    catch (Exception ex)
                    {
                        await _dialogService.ShowAlertAsync("Error", $"No se pudo navegar a la página de la localidad.{Environment.NewLine}{ex.Message}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowAlertAsync("Error", $"No se pudo añadir la localidad.{Environment.NewLine}{ex.Message}", "OK");
                }

                await CargarListaLocalidadesAsync();
            }
        }
    }

}