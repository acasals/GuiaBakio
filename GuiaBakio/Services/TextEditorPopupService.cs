using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Popups;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services;

public class TextEditorPopupService : ITextEditorPopupService
{
    private readonly IServiceProvider _services;

    public TextEditorPopupService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<string?> MostrarEditorAsync(string? initialText)
    {
        var currentPage = Shell.Current?.CurrentPage
            ?? throw new InvalidOperationException("No se pudo obtener la página actual.");

        var popup = _services.GetRequiredService<TextEditorPopup>();
        popup.Texto = initialText ?? "";

        var tcs = new TaskCompletionSource<string?>();

        popup.GuardarSolicitado += async (_, __) =>
        {
            tcs.TrySetResult(popup.Texto);
            await popup.CloseAsync();
        };

        popup.CancelarSolicitado += async (_, __) =>
        {
            tcs.TrySetResult(null);
            await popup.CloseAsync();
        };

        await currentPage.ShowPopupAsync(popup, new PopupOptions
        {
            OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
        });

        return await tcs.Task;
    }
}