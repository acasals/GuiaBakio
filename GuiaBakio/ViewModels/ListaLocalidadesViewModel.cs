using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Models;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;
using SQLite;
using System.Collections.ObjectModel;

namespace GuiaBakio.ViewModels
{
    public partial class ListaLocalidadesViewModel : ObservableObject
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

        public IRelayCommand AddLocalidadAsyncCommand { get; }

        [ObservableProperty]
        private ObservableCollection<Localidad> listaLocalidades = [];

        public ListaLocalidadesViewModel(
            DataBaseService dbService,
            IAddItemPopupService addItemPopupService,
            IDialogOKService dialogService,
            INavigationDataService navigationDataService,
            SQLiteAsyncConnection db,
            ApiService apiService)
        {
            AddLocalidadAsyncCommand = new AsyncRelayCommand(AddLocalidadAsync);
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
                await CargarListaLocalidadesAsync();
                await SincronizarSiNoRecienteAsync();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", $"No se pudo obtener la lista de localidades: {Environment.NewLine}{ex.Message}", "OK");
            }
        }

        private async Task SincronizarSiNoRecienteAsync()
        {
            if (ultimaSincronizacion == null ||
                DateTime.UtcNow - ultimaSincronizacion > TimeSpan.FromMinutes(30))
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
                    await _dialogService.ShowAlertAsync("Error", $"No se pudieron sincronizar los datos con el servidor: {Environment.NewLine}{resultado}", "OK");
            }
        }

        public async Task CargarListaLocalidadesAsync()
        {
            try
            {
                var lista = await _dbService.ObtenerLocalidadesAsync();
                ListaLocalidades = new ObservableCollection<Localidad>(lista);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo cargar la lista de localidades.", ex);
            }
        }

        [RelayCommand]
        public async Task AddLocalidadAsync()
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
