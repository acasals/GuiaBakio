using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Services.Interfaces;
using Microsoft.Maui.Controls.Shapes;



namespace GuiaBakio.Services
{
    public class AddLocalidadPopupService : IAddLocalidadPopupService
    {
        public async Task<string?> MostrarAsync()
        {
            var currentPage = Shell.Current?.CurrentPage
                             ?? Application.Current?.MainPage;

            if (currentPage is null)
                throw new InvalidOperationException("No se pudo obtener la página actual.");

            var popup = new CommunityToolkit.Maui.Views.Popup { BackgroundColor = Colors.White };

            var tcs = new TaskCompletionSource<string?>();

            var entry = new Entry
            {
                Placeholder = "Introduce una localidad",
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
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            }
            };

            grid.Add(entry, 0, 0);
            grid.Add(button, 1, 0);

            var border  = new Border
            {
                Content = grid,
                WidthRequest = 300,
                Padding = 10,
                BackgroundColor = Colors.White
            };

            popup.Content = border;
            await currentPage.ShowPopupAsync(popup);
            return await tcs.Task;
        }
    }
}
