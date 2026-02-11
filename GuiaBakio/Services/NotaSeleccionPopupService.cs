using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Models;
using GuiaBakio.Popups;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services;

public class NotaSeleccionPopupService : INotaSeleccionPopupService
{
    private readonly IServiceProvider _services;

    public NotaSeleccionPopupService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task<Nota?> MostrarAsync(IEnumerable<Nota> notas)
    {
        var currentPage = Shell.Current?.CurrentPage
            ?? throw new InvalidOperationException("No se pudo obtener la página actual.");

        var popup = _services.GetRequiredService<NotaSeleccionPopup>();
        popup.CargarNotas(notas);

        var tcs = new TaskCompletionSource<Nota?>();

        popup.Aceptado += async (_, __) =>
        {
            tcs.TrySetResult(popup.NotaSeleccionada);
            await popup.CloseAsync();
        };

        popup.Cancelado += async (_, __) =>
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
