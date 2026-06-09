using Avalonia.Controls;
using Avalonia.Interactivity;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class StatisticsView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly QuestionnaireRepository _questionnaireRepository = new();

    public StatisticsView()
    {
        InitializeComponent();
    }

    public StatisticsView(MainWindow mainWindow, User user)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _currentUser = user;

        var loc = LocalizationManager.Instance;
        loc.LanguageChanged += UpdateTexts;

        SetupColumns();
        UpdateTexts();
        LoadData();
    }

    #region Localization

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;
        TxtTitle.Text = loc.T("stats.title");
        TxtEmptyState.Text = loc.T("stats.no_data");
        LoadData();
    }

    #endregion

    #region Column setup

    private void SetupColumns()
    {
        var loc = LocalizationManager.Instance;
        StatsDataGrid.Columns.Clear();

        StatsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("stats.theme_col"),
            Binding = new Avalonia.Data.Binding("ThemeNameDisplay"),
            Width = new DataGridLength(3, DataGridLengthUnitType.Star)
        });
        StatsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("stats.count_col"),
            Binding = new Avalonia.Data.Binding("QuestionnaireCount"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
    }

    #endregion

    #region Data loading

    private void LoadData()
    {
        var loc = LocalizationManager.Instance;
        var stats = _questionnaireRepository.GetCountByTheme();

        // Translate theme names for display
        var displayStats = stats.Select(s => new ThemeStatDisplay
        {
            ThemeNameDisplay = loc.TranslateTheme(s.ThemeName),
            QuestionnaireCount = s.QuestionnaireCount
        }).ToList();

        StatsDataGrid.ItemsSource = displayStats;
        EmptyState.IsVisible = displayStats.Count == 0;
        StatsDataGrid.IsVisible = displayStats.Count > 0;

        var total = stats.Sum(s => s.QuestionnaireCount);
        TxtTotal.Text = string.Format(loc.T("stats.total"), total);
    }

    #endregion

    #region Event handlers

    private void BtnBack_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.ShowMainView(_currentUser);
    }

    private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
    {
        LoadData();
    }

    #endregion

    // Display model with translated theme name
    public class ThemeStatDisplay
    {
        public string ThemeNameDisplay { get; set; } = "";
        public int QuestionnaireCount { get; set; }
    }
}
