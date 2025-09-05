using GuiaBakio.ViewModels;

namespace GuiaBakio.Pages;

public partial class LoginPage : ContentPage
{
    private LoginViewModel _viewModel;
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }
}