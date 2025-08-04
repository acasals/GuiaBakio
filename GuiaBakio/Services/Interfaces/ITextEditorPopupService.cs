namespace GuiaBakio.Services.Interfaces
{
    public interface ITextEditorPopupService
    {
        Task<string?> MostrarEditorAsync(string? initialText);
    }
}
