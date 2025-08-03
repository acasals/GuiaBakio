using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;



namespace GuiaBakio.Views
{
    internal class AddLocalidadPopup
    {
        public async Task<string?> MostrarAsync(Page hostPage)
        {
            var tcs = new TaskCompletionSource<string?>();
            var popup = new CommunityToolkit.Maui.Views.Popup { BackgroundColor = Colors.White };
              
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
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = 12
                },
                BackgroundColor = Colors.White
            };

            popup.Content = border;
            await hostPage.ShowPopupAsync(popup);

            return await tcs.Task;
        }
    }
}
