namespace GuiaBakio.Services.Interfaces
{
    public interface IDialogYesNoService
    {
        Task<bool> ShowAlertAsync(string title, string message, string yes, string no);
    }
}
