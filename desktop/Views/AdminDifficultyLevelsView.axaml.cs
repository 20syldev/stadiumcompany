using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class AdminDifficultyLevelsView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly DifficultyLevelRepository _repo = new();

    public AdminDifficultyLevelsView()
    {
        InitializeComponent();
    }

    public AdminDifficultyLevelsView(MainWindow mainWindow, User user)
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
        TxtTitle.Text = loc.T("difficulty.title");
        TxtBtnAdd.Text = loc.T("difficulty.add");
        TxtEmptyState.Text = loc.T("difficulty.no_levels");
    }

    #endregion

    #region Column setup

    private void SetupColumns()
    {
        var loc = LocalizationManager.Instance;
        LevelsDataGrid.Columns.Clear();

        LevelsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("difficulty.col_id"),
            Binding = new Avalonia.Data.Binding("Id"),
            Width = new DataGridLength(50)
        });
        LevelsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("difficulty.col_label"),
            Binding = new Avalonia.Data.Binding("Label"),
            Width = new DataGridLength(2, DataGridLengthUnitType.Star)
        });
        LevelsDataGrid.Columns.Add(new DataGridTextColumn
        {
            Header = loc.T("difficulty.col_position"),
            Binding = new Avalonia.Data.Binding("Position"),
            Width = new DataGridLength(1, DataGridLengthUnitType.Star)
        });

        // Edit button column
        LevelsDataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(90),
            CellTemplate = new FuncDataTemplate<DifficultyLevel>((level, _) =>
            {
                if (level == null) return new Panel();
                var btn = new Button
                {
                    Content = LocalizationManager.Instance.T("common.edit"),
                    Padding = new Avalonia.Thickness(10, 5),
                    FontSize = 12,
                    Tag = level
                };
                btn.Click += BtnEdit_Click;
                return btn;
            })
        });

        // Delete button column
        LevelsDataGrid.Columns.Add(new DataGridTemplateColumn
        {
            Header = "",
            Width = new DataGridLength(90),
            CellTemplate = new FuncDataTemplate<DifficultyLevel>((level, _) =>
            {
                if (level == null) return new Panel();
                var loc2 = LocalizationManager.Instance;
                var app = Avalonia.Application.Current!;
                app.TryFindResource("DangerBrush", app.ActualThemeVariant, out var dangerRes);
                var dangerBrush = dangerRes as Avalonia.Media.IBrush ?? Avalonia.Media.Brushes.Red;

                var btn = new Button
                {
                    Content = loc2.T("common.delete"),
                    Padding = new Avalonia.Thickness(10, 5),
                    FontSize = 12,
                    Tag = level,
                    Background = Avalonia.Media.Brushes.Transparent,
                    Foreground = dangerBrush,
                };
                btn.Click += BtnDelete_Click;
                return btn;
            })
        });
    }

    #endregion

    #region Data loading

    private void LoadData()
    {
        var levels = _repo.GetAll();
        LevelsDataGrid.ItemsSource = levels;
        EmptyState.IsVisible = levels.Count == 0;
        LevelsDataGrid.IsVisible = levels.Count > 0;

        var loc = LocalizationManager.Instance;
        TxtLevelCount.Text = string.Format(loc.T("difficulty.count"), levels.Count);
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

    private async void BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        var loc = LocalizationManager.Instance;
        var (labelBox, positionBox) = BuildLevelInputs();

        var dialog = new ContentDialog
        {
            Title = loc.T("difficulty.add_title"),
            Content = BuildDialogContent(loc.T("difficulty.label"), labelBox,
                                         loc.T("difficulty.position"), positionBox),
            PrimaryButtonText = loc.T("difficulty.add"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(labelBox.Text))
        {
            _repo.Create(labelBox.Text.Trim(), (int)(positionBox.Value ?? 0));
            ActivityLogger.Log(_currentUser.Id, "difficulty.create", "difficulty_level", null, labelBox.Text.Trim());
            LoadData();
        }
    }

    private async void BtnEdit_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not DifficultyLevel level) return;

        var loc = LocalizationManager.Instance;
        var (labelBox, positionBox) = BuildLevelInputs(level.Label, level.Position);

        var dialog = new ContentDialog
        {
            Title = loc.T("difficulty.edit_title"),
            Content = BuildDialogContent(loc.T("difficulty.label"), labelBox,
                                         loc.T("difficulty.position"), positionBox),
            PrimaryButtonText = loc.T("common.edit"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(labelBox.Text))
        {
            level.Label = labelBox.Text.Trim();
            level.Position = (int)(positionBox.Value ?? 0);
            _repo.Update(level);
            ActivityLogger.Log(_currentUser.Id, "difficulty.update", "difficulty_level", level.Id, level.Label);
            LoadData();
        }
    }

    private async void BtnDelete_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not DifficultyLevel level) return;

        var loc = LocalizationManager.Instance;

        // Warn if questionnaires currently use this level
        var usageCount = _repo.CountUsage(level.Id);
        var message = usageCount > 0
            ? $"{string.Format(loc.T("difficulty.confirm_delete_in_use"), usageCount)}\n{string.Format(loc.T("difficulty.confirm_delete_message"), level.Label)}"
            : string.Format(loc.T("difficulty.confirm_delete_message"), level.Label);

        var dialog = new ContentDialog
        {
            Title = loc.T("difficulty.confirm_delete_title"),
            Content = message,
            PrimaryButtonText = loc.T("common.delete"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _repo.Delete(level.Id);
            ActivityLogger.Log(_currentUser.Id, "difficulty.delete", "difficulty_level", level.Id, level.Label);
            LoadData();
        }
    }

    #endregion

    #region UI helpers

    private static (TextBox labelBox, NumericUpDown positionBox) BuildLevelInputs(
        string label = "", int position = 0)
    {
        var labelBox = new TextBox
        {
            Text = label,
            Watermark = LocalizationManager.Instance.T("difficulty.label_placeholder"),
            Height = 40,
            CornerRadius = new Avalonia.CornerRadius(8)
        };
        var positionBox = new NumericUpDown
        {
            Value = position,
            Minimum = 0,
            Maximum = 999,
            Increment = 1,
            Width = 120
        };
        return (labelBox, positionBox);
    }

    private static StackPanel BuildDialogContent(
        string labelText, TextBox labelBox, string positionText, NumericUpDown positionBox)
    {
        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = labelText, FontSize = 13 });
        panel.Children.Add(labelBox);
        panel.Children.Add(new TextBlock { Text = positionText, FontSize = 13 });
        panel.Children.Add(positionBox);
        return panel;
    }

    #endregion
}
