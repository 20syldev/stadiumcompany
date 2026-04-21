using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class AdminConnectionsView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly ActivityLogRepository _activityLogRepository = new();

    private int _currentPage = 0;
    private const int PageSize = 50;
    private int _totalCount = 0;

    public AdminConnectionsView()
    {
        InitializeComponent();
    }

    public AdminConnectionsView(MainWindow mainWindow, User user)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _currentUser = user;

        if (!user.IsAdmin) return;

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
        TxtTitle.Text = loc.T("admin.connections_title");
        TxtFrom.Text = loc.T("admin.from");
        TxtTo.Text = loc.T("admin.to");
        TxtBtnSearch.Text = loc.T("admin.btn_search");
        TxtBtnReset.Text = loc.T("admin.btn_reset");
        TxtBtnPrevious.Text = loc.T("admin.previous");
        TxtBtnNext.Text = loc.T("admin.next");
        TxtEmptyState.Text = loc.T("admin.no_connections");
        UpdatePagination();
    }

    #endregion

    #region Columns

    private void SetupColumns()
    {
        var loc = LocalizationManager.Instance;
        ConnectionsDataGrid.Columns.Clear();
        ConnectionsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("admin.col_user"),
            Binding = new Binding("UserName"),
            Width = new DataGridLength(2, DataGridLengthUnitType.Star)
        });
        ConnectionsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("admin.col_email"),
            Binding = new Binding("Email"),
            Width = new DataGridLength(2.5, DataGridLengthUnitType.Star)
        });
        ConnectionsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("admin.col_date"),
            Binding = new Binding("DateFormatted"),
            Width = new DataGridLength(2, DataGridLengthUnitType.Star)
        });
        ConnectionsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("admin.col_source"),
            Binding = new Binding("Source"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });
    }

    #endregion

    #region Data loading

    private void LoadData()
    {
        try
        {
            var from = DpFrom.SelectedDate?.Date;
            var to = DpTo.SelectedDate?.Date?.AddDays(1).AddSeconds(-1);
            int offset = _currentPage * PageSize;

            _totalCount = _activityLogRepository.GetLoginCount(from, to);
            var logs = _activityLogRepository.SearchLogins(from, to, PageSize, offset);

            var display = logs.Select(l => new ConnectionDisplay
            {
                UserName = string.IsNullOrWhiteSpace(l.UserName) ? $"#{l.UserId}" : l.UserName,
                Email = l.UserEmail ?? "",
                DateFormatted = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Source = l.Source
            }).ToList();

            ConnectionsDataGrid.ItemsSource = display;
            EmptyState.IsVisible = display.Count == 0;
            ConnectionsDataGrid.IsVisible = display.Count > 0;
        }
        catch
        {
            EmptyState.IsVisible = true;
            ConnectionsDataGrid.IsVisible = false;
            _totalCount = 0;
        }

        UpdatePagination();
    }

    #endregion

    #region Pagination

    private void UpdatePagination()
    {
        var loc = LocalizationManager.Instance;
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalCount / PageSize));
        TxtPageInfo.Text = string.Format(loc.T("admin.pagination"), _currentPage + 1, totalPages);
        BtnPrevious.IsEnabled = _currentPage > 0;
        BtnNext.IsEnabled = (_currentPage + 1) < totalPages;
    }

    private void BtnPrevious_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentPage > 0) { _currentPage--; LoadData(); }
    }

    private void BtnNext_Click(object? sender, RoutedEventArgs e)
    {
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalCount / PageSize));
        if (_currentPage + 1 < totalPages) { _currentPage++; LoadData(); }
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

    private void BtnSearch_Click(object? sender, RoutedEventArgs e)
    {
        _currentPage = 0;
        LoadData();
    }

    private void BtnReset_Click(object? sender, RoutedEventArgs e)
    {
        DpFrom.SelectedDate = null;
        DpTo.SelectedDate = null;
        _currentPage = 0;
        LoadData();
    }

    #endregion

    #region Display DTO

    private class ConnectionDisplay
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string DateFormatted { get; set; } = "";
        public string Source { get; set; } = "";
    }

    #endregion
}
