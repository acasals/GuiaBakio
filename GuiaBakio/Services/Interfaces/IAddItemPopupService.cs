namespace GuiaBakio.Services.Interfaces
{
    public interface IAddItemPopupService
    {
        Task<string?> MostrarAsync(string texto);
    }
}