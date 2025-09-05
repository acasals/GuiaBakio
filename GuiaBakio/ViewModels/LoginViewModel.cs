using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDialogOKService _dialogService;
        private readonly DataBaseService _dbService;

        [ObservableProperty]
        private string nombreUsuario;

        public IRelayCommand AddUsuarioAsyncCommand { get; }
        public LoginViewModel(DataBaseService dataBaseService, IDialogOKService dialogOKService)
        {
            _dbService = dataBaseService ?? throw new ArgumentNullException(nameof(dataBaseService));
            _dialogService = dialogOKService ?? throw new ArgumentNullException(nameof(dialogOKService));

            AddUsuarioAsyncCommand = new AsyncRelayCommand(AddUsuarioAsync);
        }

        [RelayCommand]
        private async Task AddUsuarioAsync()
        {
            if (string.IsNullOrWhiteSpace(NombreUsuario))
            {
                await _dialogService.ShowAlertAsync("Error", "El nombre de usuario no puede estar vacío. Por favor, introduce un nombre.", "OK");
                return;
            }
            var existe = await _dbService.ExisteUsuarioAsync(NombreUsuario);
            if (existe == true)
            {
                await _dialogService.ShowAlertAsync("Error", "El nombre de usuario ya existe. Por favor, elige otro.", "OK");
                return;
            }

            try
            {
                int usuarioId = await _dbService.InsertarUsuarioAsync(NombreUsuario);
                Preferences.Set("UsuarioId", usuarioId);
                await Shell.Current.GoToAsync("mainPage");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudo guardar el usuario.", ex);
            }
        }
    }
}
