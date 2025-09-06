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