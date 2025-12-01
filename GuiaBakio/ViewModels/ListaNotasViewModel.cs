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
        private string _usuarioName;

        public IRelayCommand AgregarNotaAsyncCommand { get; }
        public IRelayCommand EditarLocalidadesAsyncCommand { get; }

        public IRelayCommand ToggleEtiquetaCommand => new RelayCommand<Etiqueta>(async etiqueta =>
        {
            etiqueta?.IsSelected = !etiqueta.IsSelected;
            EtiquetasSeleccionadas = Etiquetas.Where(e => e.IsSelected).ToObservableCollection();
            NotasFiltradas = (await _dbService.ObtenerNotasAsync(EtiquetasSeleccionadas.ToList(), LocalidadesSeleccionadas.ToList())).ToObservableCollection();
        });

        public IRelayCommand ToggleLocalidadCommand => new RelayCommand<Localidad>(async Localidad =>
        {
            Localidad?.IsSelected = !Localidad.IsSelected;
            LocalidadesSeleccionadas = Localidades.Where(e => e.IsSelected).ToObservableCollection();
            NotasFiltradas = (await _dbService.ObtenerNotasAsync(EtiquetasSeleccionadas.ToList(), LocalidadesSeleccionadas.ToList())).ToObservableCollection();
        });
        [ObservableProperty]
        private ObservableCollection<Nota> listaNotas = [];

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
                    Usuario? usuario = await _dbService.ObtenerUsuarioPorIdAsync(_usuarioId);
                    if (usuario is null)
                    {
                        await _dialogService.ShowAlertAsync("Error", "Hubo un problema al localizar al usuario. Contacta con Alex.", "OK");
                        return;
                    }
                    _usuarioName = usuario.Nombre;
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
                await CargarListaNotasAsync();
                await SincronizarSiNoRecienteAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo obtener la lista de notas: {Environment.NewLine}{ex.Message}", "OK");
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
                var lista = await _dbService.ObtenerNotasAsync();
                ListaNotas = new ObservableCollection<Nota>(lista);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar la lista de notas.", ex);
            }
        }

        [RelayCommand]
        public async Task AgregarNotaAsync()
        {

        }

    }
}
