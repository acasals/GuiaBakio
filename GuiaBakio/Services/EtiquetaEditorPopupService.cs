using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Helpers;
using GuiaBakio.Models;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services
{
    public class EtiquetaEditorPopupService : IEtiquetaEditorPopupService
    {
        public async Task<List<Etiqueta>?> MostrarEditorAsync(int? notaId)
        {
            var currentPage = Shell.Current?.CurrentPage;
            if (currentPage is null)
                throw new InvalidOperationException("No se pudo obtener la página actual.");
            var popup = new CommunityToolkit.Maui.Views.Popup
            {
                BackgroundColor = Colors.White
            };
            var tcs = new TaskCompletionSource<List<Etiqueta>?>();
            //var entry = new Entry
            //{
            //    Text = initialText,
            //    HeightRequest = 40,
            //    WidthRequest = 300,
            //    Placeholder = "Nombre de la etiqueta"
            //};
            var cancelButton = new Button
            {
                Text = "Cancelar",
                HorizontalOptions = LayoutOptions.Start
            };
            StyleHelper.ApplyStyle(cancelButton, "BotonCancelarStyle");
            cancelButton.Clicked += async (_, _) =>
            {
                tcs.TrySetResult(null);
                await popup.CloseAsync();
            };
            var saveButton = new Button
            {
                Text = "Guardar",
                HorizontalOptions = LayoutOptions.End
            };
            saveButton.Clicked += async (_, _) =>
            {
                //tcs.TrySetResult(entry.Text);
                await popup.CloseAsync();
            };
            var botonesRow = new Grid
            {
                RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto }
                    },
                ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    }
            };
            botonesRow.Add(cancelButton, 0, 0);
            botonesRow.Add(saveButton, 1, 0);
            var layout = new StackLayout
            {
                Padding = new Thickness(20),
                Spacing = 10,
                Children =
                {
                    new Label { Text = "Editar Etiqueta", FontAttributes = FontAttributes.Bold, FontSize=18, HorizontalOptions=LayoutOptions.Center },
                    //entry,
                    botonesRow
                }
            };
            popup.Content = layout;
            await currentPage.ShowPopupAsync(popup, new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
            });
            return await tcs.Task;
        }
    }
}
