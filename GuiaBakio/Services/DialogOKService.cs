using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services
{
    public class DialogOKService : IDialogOKService
    {
        public Task ShowAlertAsync(string title, string message, string cancel)
        {
            var currentPage = Shell.Current?.CurrentPage;

            if (currentPage != null)
                return currentPage.DisplayAlert(title, message, cancel);

            return Task.CompletedTask;
        }
    }
}
