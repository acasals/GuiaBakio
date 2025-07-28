using System.Globalization;
using System.Text;

namespace GuiaBakio.Helpers
{
    internal static class MisTextUtils
    {
        public static string Normalizar(string texto)
        {
            var form = texto.Normalize(NormalizationForm.FormD);
            var sinAcentos = new string(form
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
            return sinAcentos.Trim();
        }

    }
}
