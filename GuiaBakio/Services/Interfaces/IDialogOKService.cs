namespace GuiaBakio.Services.Interfaces
{
    public interface IDialogOKService
    {
        Task ShowAlertAsync(string title, string message, string cancel);
    }
}
