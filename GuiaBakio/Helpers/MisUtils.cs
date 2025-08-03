using System.Globalization;
using System.Text;

namespace GuiaBakio.Helpers
{
    public static class MisUtils
    {
        public static string NormalizarTexto(string texto)
        {
            var form = texto.Normalize(NormalizationForm.FormD);
            var sinAcentos = new string(form
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
            return sinAcentos.Trim();
        }

        public static async Task MostrarBotonAnimado(Button boton)
        {
            boton.Opacity = 0;
            boton.Scale = 0.5;

            await Task.Delay(300);

            await Task.WhenAll(
                boton.FadeTo(1, 800),
                boton.ScaleTo(1, 800, Easing.SpringOut)
            );
        }

    }
}
