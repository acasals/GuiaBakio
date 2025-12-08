using GuiaBakio.Models;

namespace GuiaBakio.Helpers
{
    public class LocalidadTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LocalidadTemplate { get; set; }
        public DataTemplate AddButtonTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is Localidad loc && loc.IsButton)
                return AddButtonTemplate;

            return LocalidadTemplate;
        }
    }
}
