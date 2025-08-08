using GuiaBakio.Pages;

namespace GuiaBakio
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("localidadPage", typeof(LocalidadPage));
            Routing.RegisterRoute("apartadoPage", typeof(ApartadoPage));
            Routing.RegisterRoute("mainPage", typeof(MainPage));
        }
    }
}
