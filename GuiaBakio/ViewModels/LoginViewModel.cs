using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GuiaBakio.Services;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IDialogOKService _dialogService;
        private readonly IDialogYesNoService _dialogYesNoService;
        private readonly DataBaseService _dbService;

        [ObservableProperty]
        private string nombreUsuario;

        public IRelayCommand AddUsuarioAsyncCommand { get; }
        public LoginViewModel(
            DataBaseService dataBaseService,
            IDialogOKService dialogOKService,
            IDialogYesNoService dialogYesNoService)
        {
            _dbService = dataBaseService ?? throw new ArgumentNullException(nameof(dataBaseService));
            _dialogService = dialogOKService ?? throw new ArgumentNullException(nameof(dialogOKService));
            _dialogYesNoService = dialogYesNoService ?? throw new ArgumentNullException(nameof(_dialogYesNoService));

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
            var usuario = await _dbService.ObtenerUsuarioPorNombreAsync(NombreUsuario);
            if (usuario != null)
            {
                var response = await _dialogYesNoService.ShowAlertAsync("Error", $"El nombre del usuario {NombreUsuario} ya existe en otro dispositivo. Por favor, elige otro, a menos de que seas tú seguro.", "Aceptar", "Cancelar");
                if (response == true)
                {
                    try
                    {
                        Preferences.Set("UsuarioId", usuario.Id);
                        await Shell.Current.GoToAsync("mainPage");
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("No se pudo guardar el usuario.", ex);
                    }
                }
            }
            else
            {
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
}
