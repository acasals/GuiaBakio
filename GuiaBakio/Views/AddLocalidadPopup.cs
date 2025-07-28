using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaBakio.Views
{
    internal class AddLocalidadPopup
    {
        public event EventHandler<string>? CityAdded;

        private Entry? _entry;

        public void MostrarPopup(Page hostPage)
        {
            var popup = new Popup
            {
                Color = Colors.White
            };

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
                    CityAdded?.Invoke(this, _entry.Text);
                    popup.Close();
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
