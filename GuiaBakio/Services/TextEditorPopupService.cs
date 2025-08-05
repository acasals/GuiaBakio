using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using GuiaBakio.Services.Interfaces;

namespace GuiaBakio.Services
{
    public class TextEditorPopupService : ITextEditorPopupService
    {
        public async Task<string?> MostrarEditorAsync(string? initialText)
        {
            var currentPage = Shell.Current?.CurrentPage
                              ?? Application.Current?.MainPage;

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
                AutoSize = EditorAutoSizeOption.TextChanges
            };

            var cancelButton = new Button { Text = "Cancelar" };
            cancelButton.Clicked += async (_, _) =>
            {
                tcs.TrySetResult(null);
                await popup.CloseAsync();
            };

            var saveButton = new Button { Text = "Guardar" };
            saveButton.Clicked += async (_, _) =>
            {
                tcs.TrySetResult(editor.Text);
                await popup.CloseAsync();
            };

            popup.Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
            {
                new Label { Text = "Edita el texto:", FontAttributes = FontAttributes.Bold },
                editor,
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children = { cancelButton, saveButton }
                }
            }
            };

            await currentPage.ShowPopupAsync(popup,new PopupOptions
            {
                OnTappingOutsideOfPopup = () => tcs.TrySetResult(null)
            });
            return await tcs.Task;
        }
    }
}
