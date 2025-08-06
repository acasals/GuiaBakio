using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services
{
    public class DialogYesNoService: IDialogYesNoService
    {
        public Task<bool> ShowAlertAsync(string title, string message, string yes, string no)
        {
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage != null)
                return currentPage.DisplayAlert(title, message, yes, no);
            return Task.FromResult(false);
        }
    }
}
