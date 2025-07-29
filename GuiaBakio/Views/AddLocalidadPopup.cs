using CommunityToolkit.Maui.Views;

namespace GuiaBakio.Views
{
    internal class AddLocalidadPopup
    {
       
        private Entry? _entry;
        private readonly TaskCompletionSource<string?> _tcs = new();

        private CommunityToolkit.Maui.Views.Popup popup = new() { Color=Colors.White};
        public Task<string?> MostrarYEsperarAsync(Page parent)
        {
            MostrarPopup(parent); 

            return _tcs.Task;
        }

        public void Cerrar(string nombreLocalidad)
        {
           _tcs?.TrySetResult(nombreLocalidad);
            popup?.Close();
        }


        public void MostrarPopup(Page hostPage)
        {
         
            var miTabla = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            _entry = new Entry
            {
                Placeholder = "Introduce una localidad",
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
            };

            var button = new Button
            {
                Text = "Añadir",
                HorizontalOptions = LayoutOptions.Fill,
                Command = new Command(() =>
                {
                    Cerrar(_entry.Text);
                })
            };

            miTabla.Add(_entry, 0, 0);
            miTabla.Add(button, 1, 0);

            popup.Content = new Frame
            {
                Content = miTabla,
                WidthRequest = 300,
                Padding = 10,
                CornerRadius = 12,
                BackgroundColor = Colors.White
            };
            hostPage.ShowPopup(popup);
        }
    }
}
