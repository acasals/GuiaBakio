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

        var dbName = "GuiaBakio.db";
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, dbName);
        if (!File.Exists(dbPath))
        {
            Preferences.Clear(); // Limpiar las preferencias si la base de datos no existe  
        }

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