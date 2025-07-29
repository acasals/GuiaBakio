using GuiaBakio.Pages;

namespace GuiaBakio
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("localidadPage", typeof(LocalidadPage));
        }
    }
}
