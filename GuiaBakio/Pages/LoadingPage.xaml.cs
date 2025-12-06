namespace GuiaBakio.Pages;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();


        // Preferences.Clear(); // Descomenta esta línea si has borrado la base de datos y quieres reiniciar las preferencias
        if (!Preferences.ContainsKey("UsuarioId"))
        {
            await Shell.Current.GoToAsync("loginPage");
        }
        else
        {
            await Shell.Current.GoToAsync("mainPage");
        }
    }
}