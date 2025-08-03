using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;

namespace GuiaBakio.Views;

internal class TextEditorPopup 
{
    //private readonly Editor? _editor;

    public async Task<string?> MostrarAsync(Page hostPage, string? initialText)
    {
       var tcs = new TaskCompletionSource<string?>();
        var popup = new CommunityToolkit.Maui.Views.Popup { BackgroundColor = Colors.White };

        Editor _editor = new Editor
        {
            Text = initialText,
            HeightRequest = 200,
            AutoSize = EditorAutoSizeOption.TextChanges
        };

        var cancelButton = new Button
        {
            Text = "Cancelar"
        };
        cancelButton.Clicked += async (s, e) =>
        {
            tcs.TrySetResult(null);
            await popup.CloseAsync();
        };

        var saveButton = new Button
        {
            Text = "Guardar"
        };
        saveButton.Clicked += async (s, e) =>
        {
            tcs.TrySetResult(null);
            await popup.CloseAsync();
        };

        var layout = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            Children =
            {
                new Label { Text = "Edita el texto:", FontAttributes = FontAttributes.Bold },
                _editor,
                new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children = { cancelButton, saveButton }
                }
            }
        };

        popup.Content = layout;
        await hostPage.ShowPopupAsync(popup);

        return await tcs.Task;
    }
}