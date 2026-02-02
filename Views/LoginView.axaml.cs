using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class LoginView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly UserRepository _userRepository = new();
    private bool _isRegisterMode = false;

    public LoginView()
    {
        InitializeComponent();
    }

    public LoginView(MainWindow mainWindow)
    {
        InitializeComponent();
        _mainWindow = mainWindow;

        LocalizationManager.Instance.LanguageChanged += UpdateTexts;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        if (_isRegisterMode)
        {
            TitleText.Text = loc.T("login.register_title");
            BtnSubmit.Content = loc.T("login.register");
            TxtTogglePrompt.Text = loc.T("login.toggle_login");
            BtnToggleMode.Content = loc.T("login.submit");
        }
        else
        {
            TitleText.Text = loc.T("login.title");
            BtnSubmit.Content = loc.T("login.submit");
            TxtTogglePrompt.Text = loc.T("login.toggle_register");
            BtnToggleMode.Content = loc.T("login.register");
        }

        LblEmail.Text = loc.T("login.email");
        TxtLogin.Watermark = loc.T("login.email_placeholder");
        LblPassword.Text = loc.T("login.password");
        TxtPassword.Watermark = loc.T("login.password_placeholder");
        LblFirstName.Text = loc.T("login.firstname");
        TxtFirstName.Watermark = loc.T("login.firstname_placeholder");
        LblLastName.Text = loc.T("login.lastname");
        TxtLastName.Watermark = loc.T("login.lastname_placeholder");
    }

    private void BtnToggleMode_Click(object? sender, RoutedEventArgs e)
    {
        _isRegisterMode = !_isRegisterMode;

        PnlFirstName.IsVisible = _isRegisterMode;
        PnlLastName.IsVisible = _isRegisterMode;

        UpdateTexts();
        ErrorBorder.IsVisible = false;
        ClearFields();
    }

    private void BtnSubmit_Click(object? sender, RoutedEventArgs e)
    {
        ErrorBorder.IsVisible = false;

        if (string.IsNullOrWhiteSpace(TxtLogin.Text) || string.IsNullOrWhiteSpace(TxtPassword.Text))
        {
            var loc = LocalizationManager.Instance;
            ShowError(loc.T("login.error_fields"));
            return;
        }

        if (_isRegisterMode)
        {
            Register();
        }
        else
        {
            Login();
        }
    }

    private void Login()
    {
        var user = _userRepository.Authenticate(TxtLogin.Text!.Trim(), TxtPassword.Text!);
        if (user != null)
        {
            _mainWindow.ShowMainView(user);
        }
        else
        {
            var loc = LocalizationManager.Instance;
            ShowError(loc.T("login.error_credentials"));
        }
    }

    private async void Register()
    {
        var loc = LocalizationManager.Instance;

        if (_userRepository.EmailExists(TxtLogin.Text!.Trim()))
        {
            ShowError(loc.T("login.error_email_exists"));
            return;
        }

        var user = new User
        {
            Email = TxtLogin.Text!.Trim(),
            Password = TxtPassword.Text!,
            FirstName = string.IsNullOrWhiteSpace(TxtFirstName.Text) ? null : TxtFirstName.Text.Trim(),
            LastName = string.IsNullOrWhiteSpace(TxtLastName.Text) ? null : TxtLastName.Text.Trim()
        };

        if (_userRepository.Register(user))
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var dialog = new ContentDialog
            {
                Title = loc.T("login.register_success_title"),
                Content = loc.T("login.register_success_message"),
                PrimaryButtonText = loc.T("common.ok")
            };
            await dialog.ShowAsync(topLevel);

            BtnToggleMode_Click(null, null!);
            TxtLogin.Text = user.Email;
        }
        else
        {
            ShowError(loc.T("login.error_register"));
        }
    }

    private void ShowError(string message)
    {
        LblError.Text = message;
        ErrorBorder.IsVisible = true;
    }

    private void ClearFields()
    {
        TxtLogin.Text = "";
        TxtPassword.Text = "";
        TxtFirstName.Text = "";
        TxtLastName.Text = "";
    }
}
