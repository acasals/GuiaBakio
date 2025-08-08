using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Services.Interfaces;
using Microsoft.Maui.Controls.Shapes;



namespace GuiaBakio.Services
{
    public class AddItemPopupService : IAddItemPopupService
    {
        public async Task<string?> MostrarAsync(string texto)
        {
            var currentPage = Shell.Current?.CurrentPage;

            if (currentPage is null)
                throw new InvalidOperationException("No se pudo obtener la página actual.");

            var popup = new CommunityToolkit.Maui.Views.Popup
            {
                BackgroundColor = Colors.White
            };

            var tcs = new TaskCompletionSource<string?>();

            var entry = new Entry
            {
                Placeholder = texto,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            var button = new Button
            {
                Text = "Añadir",
                HorizontalOptions = LayoutOptions.Fill,
                Command = new Command(async () =>
                {
                    tcs.TrySetResult(entry.Text);
                    await popup.CloseAsync();
                })
            };

            var grid = new Grid
            {
                ColumnSpacing=10,
                WidthRequest = 300,
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2,GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
            };

            grid.Add(entry, 0, 0);
            grid.Add(button, 1, 0);

            popup.Content = grid;
            await currentPage.ShowPopupAsync(popup, new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
            });
            return await tcs.Task;
        }
    }
}
