using Avalonia;
using Avalonia.Controls;
using StadiumCompany.Models;

namespace StadiumCompany.Views;

public partial class MainWindow : Window
{
    private readonly Panel _backgroundPanel;

    public MainWindow()
    {
        InitializeComponent();
        _backgroundPanel = this.FindControl<Panel>("BackgroundPanel")!;
        ShowLoginView();
    }

    public void ShowLoginView()
    {
        Title = "Stadium Company";
        MinWidth = 800;
        MinHeight = 550;
        CanResize = true;
        WindowState = WindowState.Maximized;

        _backgroundPanel.Bind(Panel.BackgroundProperty, this.GetResourceObservable("AppBackgroundBrush"));

        ContentArea.Content = new LoginView(this);
    }

    public void ShowMainView(User user)
    {
        Title = "Stadium Company - Questionnaires";
        MinWidth = 800;
        MinHeight = 550;
        CanResize = true;
        WindowState = WindowState.Maximized;

        _backgroundPanel.Bind(Panel.BackgroundProperty, this.GetResourceObservable("AppBackgroundAltBrush"));

        ContentArea.Content = new MainView(this, user);
    }

    public void Logout()
    {
        ShowLoginView();
    }
}
