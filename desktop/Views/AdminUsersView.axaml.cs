using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class AdminUsersView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly UserRepository _userRepository = new();

    public AdminUsersView()
    {
        InitializeComponent();
    }

    public AdminUsersView(MainWindow mainWindow, User user)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _currentUser = user;

        if (!user.IsAdmin) return;

        var loc = LocalizationManager.Instance;
        loc.LanguageChanged += UpdateTexts;

        UpdateTexts();
        SetupColumns();
        LoadData();
    }

    #region Localization

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        TxtTitle.Text = loc.T("admin.users_title");
        TxtBtnArchive.Text = loc.T("admin.archive_inactive");
        TxtEmptyState.Text = loc.T("admin.no_users");
    }

    #endregion

    #region Column setup

    private void SetupColumns()
    {
        var loc = LocalizationManager.Instance;
        UsersDataGrid.Columns.Clear();
        UsersDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_id"), Binding = new Avalonia.Data.Binding("Id"), Width = new DataGridLength(50) });
        UsersDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_email"), Binding = new Avalonia.Data.Binding("Email"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
        UsersDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_name"), Binding = new Avalonia.Data.Binding("FullName"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });
        UsersDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_archived"), Binding = new Avalonia.Data.Binding("ArchivedDisplay"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        UsersDataGrid.Columns.Add(new DataGridTextColumn { Header = loc.T("admin.col_created"), Binding = new Avalonia.Data.Binding("CreatedAtFormatted"), Width = new DataGridLength(2, DataGridLengthUnitType.Star) });

        // Unarchive button column
        var unarchiveTemplate = new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(120),
            CellTemplate = new FuncDataTemplate<UserDisplay>((user, _) =>
            {
                if (user == null || !user.IsArchived) return new Panel();

                var loc2 = LocalizationManager.Instance;
                var btn = new Button
                {
                    Content = loc2.T("admin.unarchive"),
                    Padding = new Avalonia.Thickness(10, 5),
                    FontSize = 12,
                    Tag = user.Id,
                };
                btn.Click += BtnUnarchive_Click;
                return btn;
            })
        };
        UsersDataGrid.Columns.Add(unarchiveTemplate);
    }

    #endregion

    #region Data loading

    private void LoadData()
    {
        try
        {
            var users = _userRepository.GetAllNonAdmin();
            var loc = LocalizationManager.Instance;

            var displayUsers = users.Select(u => new UserDisplay
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName ?? "",
                IsArchived = u.IsArchived,
                ArchivedDisplay = u.IsArchived ? loc.T("common.yes") : loc.T("common.no"),
                CreatedAtFormatted = u.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            }).ToList();

            UsersDataGrid.ItemsSource = displayUsers;
            EmptyState.IsVisible = displayUsers.Count == 0;
            UsersDataGrid.IsVisible = displayUsers.Count > 0;

            TxtUserCount.Text = string.Format(loc.T("admin.user_count"), displayUsers.Count);
        }
        catch
        {
            EmptyState.IsVisible = true;
            UsersDataGrid.IsVisible = false;
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
        LoadData();
    }

    private async void BtnArchiveInactive_Click(object? sender, RoutedEventArgs e)
    {
        var loc = LocalizationManager.Instance;

        var dialog = new ContentDialog
        {
            Title = loc.T("admin.archive_confirm_title"),
            Content = loc.T("admin.archive_confirm_message"),
            PrimaryButtonText = loc.T("common.yes"),
            CloseButtonText = loc.T("common.cancel"),
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            var count = _userRepository.ArchiveInactiveUsers();
            ActivityLogger.Log(_currentUser.Id, "users.archive", "users", null, $"Archived {count} inactive user(s)");

            var resultDialog = new ContentDialog
            {
                Title = loc.T("admin.archive_confirm_title"),
                Content = string.Format(loc.T("admin.archive_result"), count),
                CloseButtonText = loc.T("common.ok"),
            };
            await resultDialog.ShowAsync();

            LoadData();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = loc.T("common.error"),
                Content = ex.Message,
                CloseButtonText = loc.T("common.ok"),
            };
            await errorDialog.ShowAsync();
        }
    }

    private async void BtnUnarchive_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not int userId) return;

        var loc = LocalizationManager.Instance;

        var dialog = new ContentDialog
        {
            Title = loc.T("admin.unarchive_confirm_title"),
            Content = loc.T("admin.unarchive_confirm_message"),
            PrimaryButtonText = loc.T("common.yes"),
            CloseButtonText = loc.T("common.cancel"),
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            _userRepository.UnarchiveUser(userId);
            ActivityLogger.Log(_currentUser.Id, "users.unarchive", "user", userId);
            LoadData();
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = loc.T("common.error"),
                Content = ex.Message,
                CloseButtonText = loc.T("common.ok"),
            };
            await errorDialog.ShowAsync();
        }
    }

    #endregion

    #region Display models

    public class UserDisplay
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsArchived { get; set; }
        public string ArchivedDisplay { get; set; } = "";
        public string CreatedAtFormatted { get; set; } = "";
    }

    #endregion
}
