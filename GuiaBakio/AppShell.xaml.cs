using GuiaBakio.Pages;

namespace GuiaBakio
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("localidadPage", typeof(LocalidadPage));
            Routing.RegisterRoute("notaPage", typeof(NotaPage));
            Routing.RegisterRoute("mainPage", typeof(MainPage));
            Routing.RegisterRoute("loginPage", typeof(LoginPage));
            Routing.RegisterRoute("loadingPage", typeof(LoadingPage));
        }
    }
}
