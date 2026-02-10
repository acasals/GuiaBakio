using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Popups;
using GuiaBakio.Services.Interfaces;



namespace GuiaBakio.Services
{
    public class AddItemPopupService : IAddItemPopupService
    {
        private readonly IServiceProvider _services;

        public AddItemPopupService(IServiceProvider services)
        {
            _services = services;
        }

        public async Task<string?> MostrarAsync(string texto)
        {
            var currentPage = (Shell.Current?.CurrentPage) ?? throw new InvalidOperationException("No se pudo obtener la página actual.");
            var popup = _services.GetRequiredService<AddItemPopup>();
            popup.EntryControl.Placeholder = texto;

            var tcs = new TaskCompletionSource<string?>();

            popup.Aceptado += async (_, __) =>
            {
                tcs.TrySetResult(popup.Texto);
                await popup.CloseAsync();
            };

            await currentPage.ShowPopupAsync(popup, new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
            });

            return await tcs.Task;
        }
    }
}
