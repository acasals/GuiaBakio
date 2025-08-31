using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Helpers;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services
{
    public class TextEditorPopupService : ITextEditorPopupService
    {
        public async Task<string?> MostrarEditorAsync(string? initialText)
        {
            var currentPage = Shell.Current?.CurrentPage;

            if (currentPage is null)
                throw new InvalidOperationException("No se pudo obtener la página actual.");

            var popup = new CommunityToolkit.Maui.Views.Popup
            {
                BackgroundColor = Colors.White
            };

            var tcs = new TaskCompletionSource<string?>();

            var editor = new Editor
            {
                Text = initialText,
                HeightRequest = 200,
                WidthRequest = 300,
                AutoSize = EditorAutoSizeOption.TextChanges
            };

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
                tcs.TrySetResult(editor.Text);
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

            popup.Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label { Text = "Edita el texto:", FontAttributes = FontAttributes.Bold },
                    editor,
                    botonesRow
                }
            };

            await currentPage.ShowPopupAsync(popup, new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
            });
            return await tcs.Task;
        }
    }
}
