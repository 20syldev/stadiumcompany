using Avalonia.Controls;
using Avalonia.Interactivity;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class AdminLogsView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly ActivityLogRepository _activityLogRepository = new();

    private int _currentPage = 0;
    private const int PageSize = 50;
    private int _totalCount = 0;

    public AdminLogsView()
    {
        InitializeComponent();
    }

    public AdminLogsView(MainWindow mainWindow, User user)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _currentUser = user;

        if (!user.IsAdmin) return;

        var loc = LocalizationManager.Instance;
        loc.LanguageChanged += UpdateTexts;

        UpdateTexts();
        SetupColumns();
        LoadFilterOptions();
        LoadData();
    }

    #region Localization

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        TxtTitle.Text = loc.T("admin.title");
        TxtSearch.Watermark = loc.T("admin.search_placeholder");
        TxtFrom.Text = loc.T("admin.from");
        TxtTo.Text = loc.T("admin.to");
        TxtBtnSearch.Text = loc.T("admin.btn_search");
        TxtBtnReset.Text = loc.T("admin.btn_reset");
        TxtBtnPrevious.Text = loc.T("admin.previous");
        TxtBtnNext.Text = loc.T("admin.next");
        TxtEmptyState.Text = loc.T("admin.no_results");
    }

    #endregion

    #region Column setup

    private void SetupColumns()
    {
        var loc = LocalizationManager.Instance;
        LogsDataGrid.Columns.Clear();
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_id"), Binding = new Avalonia.Data.Binding("Id"), Width = new DataGridLength(50) });
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_user"), Binding = new Avalonia.Data.Binding("UserName"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_action"), Binding = new Avalonia.Data.Binding("Action"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_entity"), Binding = new Avalonia.Data.Binding("EntityDisplay"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_details"), Binding = new Avalonia.Data.Binding("Details"), Width = new DataGridLength(3, DataGridLengthUnitType.Star) });
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_source"), Binding = new Avalonia.Data.Binding("Source"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        LogsDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_date"), Binding = new Avalonia.Data.Binding("CreatedAtFormatted"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
    }

    #endregion

    #region Filters

    private void LoadFilterOptions()
    {
        var loc = LocalizationManager.Instance;
        CmbFilter.Items.Clear();
        CmbFilter.Items.Add(new ComboBoxItem { Content = loc.T("admin.filter_all"), Tag = "" });

        try
        {
            foreach (var action in _activityLogRepository.GetDistinctActions())
            {
                CmbFilter.Items.Add(new ComboBoxItem { Content = action, Tag = action });
            }
        }
        catch
        {
            // Table might not exist yet
        }

        CmbFilter.SelectedIndex = 0;
    }

    private string? GetSearchQuery()
    {
        var text = TxtSearch.Text?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    private string? GetFilterValue()
    {
        if (CmbFilter.SelectedItem is ComboBoxItem item && item.Tag is string tag && !string.IsNullOrEmpty(tag))
            return tag;
        return null;
    }

    private DateTime? GetFromDate()
    {
        return DpFrom.SelectedDate?.Date;
    }

    private DateTime? GetToDate()
    {
        var date = DpTo.SelectedDate?.Date;
        return date?.AddDays(1).AddSeconds(-1);
    }

    #endregion

    #region Data loading

    private void LoadData()
    {
        try
        {
            var query = GetSearchQuery();
            var filter = GetFilterValue();
            var from = GetFromDate();
            var to = GetToDate();
            int offset = _currentPage * PageSize;

            _totalCount = _activityLogRepository.GetCount(query, filter, from, to);
            var logs = _activityLogRepository.Search(query, filter, from, to, PageSize, offset);

            var displayLogs = logs.Select(l => new ActivityLogDisplay
            {
                Id = l.Id,
                UserName = l.UserName ?? $"#{l.UserId}",
                Action = l.Action,
                EntityDisplay = l.EntityType != null ? $"{l.EntityType}#{l.EntityId}" : "",
                Details = l.Details ?? "",
                Source = l.Source,
                CreatedAtFormatted = l.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            LogsDataGrid.ItemsSource = displayLogs;
            EmptyState.IsVisible = displayLogs.Count == 0;
            LogsDataGrid.IsVisible = displayLogs.Count > 0;

            UpdatePagination();
        }
        catch
        {
            EmptyState.IsVisible = true;
            LogsDataGrid.IsVisible = false;
            _totalCount = 0;
            UpdatePagination();
        }
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
        if (_currentPage > 0)
        {
            _currentPage--;
            LoadData();
        }
    }

    private void BtnNext_Click(object? sender, RoutedEventArgs e)
    {
        int totalPages = Math.Max(1, (int)Math.Ceiling((double)_totalCount / PageSize));
        if (_currentPage + 1 < totalPages)
        {
            _currentPage++;
            LoadData();
        }
    }

    #endregion

    #region Event handlers

    private void BtnBack_Click(object? sender, RoutedEventArgs e)
    {
        _mainWindow.ShowMainView(_currentUser);
    }

    private void BtnRefresh_Click(object? sender, RoutedEventArgs e)
    {
        LoadFilterOptions();
        LoadData();
    }

    private void BtnSearch_Click(object? sender, RoutedEventArgs e)
    {
        _currentPage = 0;
        LoadData();
    }

    private void BtnReset_Click(object? sender, RoutedEventArgs e)
    {
        TxtSearch.Text = "";
        CmbFilter.SelectedIndex = 0;
        DpFrom.SelectedDate = null;
        DpTo.SelectedDate = null;
        _currentPage = 0;
        LoadData();
    }

    #endregion

    #region Display models

    private class ActivityLogDisplay
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Action { get; set; } = "";
        public string EntityDisplay { get; set; } = "";
        public string Details { get; set; } = "";
        public string Source { get; set; } = "";
        public string CreatedAtFormatted { get; set; } = "";
    }

    #endregion
}
