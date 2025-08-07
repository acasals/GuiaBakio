using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuiaBakio.Helpers
{
    public static class StyleHelper
    {
        public static void ApplyStyle(VisualElement element, string styleKey)
        {
            if (Application.Current?.Resources.TryGetValue(styleKey, out var styleObj) == true && styleObj is Style style)
            {
                element.Style = style;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Estilo '{styleKey}' no encontrado.");
            }
        }
    }
}
