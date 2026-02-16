using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;
using StadiumCompany.Services.PdfGenerator;

namespace StadiumCompany.Views;

public partial class MainView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly QuestionnaireRepository _questionnaireRepository = new();
    private readonly UserPreferencesRepository _preferencesRepository = new();
    private bool _isDarkTheme = false;
    private bool _showingMine = true;

    private List<Questionnaire> _myQuestionnaires = [];
    private List<Questionnaire> _publishedQuestionnaires = [];

    private static IBrush Res(string key)
    {
        var app = Application.Current!;
        if (app.TryFindResource(key, app.ActualThemeVariant, out var value) && value is IBrush brush)
            return brush;
        return Brushes.Transparent;
    }

    public MainView()
    {
        InitializeComponent();
    }

    public MainView(MainWindow mainWindow, User user)
    {
        InitializeComponent();
        _mainWindow = mainWindow;
        _currentUser = user;

        var loc = LocalizationManager.Instance;

        LblWelcome.Text = loc.T("main.welcome", user.FullName ?? user.Email);
        TxtUserName.Text = user.FullName ?? user.Email;
        TxtUserInitial.Text = (user.FullName ?? user.Email).Substring(0, 1).ToUpper();

        // Load saved preferences
        LoadUserPreferences();

        // Localization
        loc.LanguageChanged += UpdateTexts;
        UpdateTexts();

        LoadQuestionnaires();
        UpdateTabStyles();
        BuildCards();
    }

    #region Tab switching

    private void BtnTabMine_Click(object? sender, RoutedEventArgs e)
    {
        _showingMine = true;
        UpdateTabStyles();
        BuildCards();
    }

    private void BtnTabPublished_Click(object? sender, RoutedEventArgs e)
    {
        _showingMine = false;
        UpdateTabStyles();
        BuildCards();
    }

    private void UpdateTabStyles()
    {
        if (_showingMine)
        {
            BtnTabMine.Classes.Set("accent", true);
            BtnTabMine.FontWeight = FontWeight.SemiBold;
            BtnTabPublished.Classes.Set("accent", false);
            BtnTabPublished.Background = Brushes.Transparent;
            BtnTabPublished.FontWeight = FontWeight.Normal;
            BtnAdd.IsVisible = true;
        }
        else
        {
            BtnTabPublished.Classes.Set("accent", true);
            BtnTabPublished.FontWeight = FontWeight.SemiBold;
            BtnTabMine.Classes.Set("accent", false);
            BtnTabMine.Background = Brushes.Transparent;
            BtnTabMine.FontWeight = FontWeight.Normal;
            BtnAdd.IsVisible = false;
        }
    }

    #endregion

    #region Card building

    private void BuildCards()
    {
        CardsContainer.Children.Clear();
        var loc = LocalizationManager.Instance;

        if (_showingMine)
        {
            if (_myQuestionnaires.Count == 0)
            {
                EmptyState.IsVisible = true;
                TxtEmptyState.Text = loc.T("main.empty_mine");
                return;
            }
            EmptyState.IsVisible = false;

            foreach (var q in _myQuestionnaires)
            {
                CardsContainer.Children.Add(BuildMyQuestionnaireCard(q));
            }
        }
        else
        {
            if (_publishedQuestionnaires.Count == 0)
            {
                EmptyState.IsVisible = true;
                TxtEmptyState.Text = loc.T("main.empty_published");
                return;
            }
            EmptyState.IsVisible = false;

            foreach (var q in _publishedQuestionnaires)
            {
                CardsContainer.Children.Add(BuildPublishedQuestionnaireCard(q));
            }
        }
    }

    private Border BuildMyQuestionnaireCard(Questionnaire q)
    {
        var loc = LocalizationManager.Instance;

        var card = new Border
        {
            Width = 340,
            Margin = new Thickness(0, 0, 16, 16),
            Background = Res("CardBackgroundBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 18),
            BorderBrush = Res("BorderSubtleBrush"),
            BorderThickness = new Thickness(1)
        };
        card.Classes.Add("card-hoverable");

        var content = new StackPanel { Spacing = 10 };

        // Name
        content.Children.Add(new TextBlock
        {
            Text = q.Name,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            Foreground = Res("TextPrimaryBrush"),
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxLines = 2
        });

        // Theme badge
        var themeName = q.Theme?.Label != null ? loc.TranslateTheme(q.Theme.Label) : "";
        if (!string.IsNullOrEmpty(themeName))
        {
            var badge = new Border
            {
                Background = Res("BadgeBackgroundBrush"),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            badge.Child = new TextBlock
            {
                Text = themeName,
                FontSize = 12,
                Foreground = Res("BadgeTextBrush")
            };
            content.Children.Add(badge);
        }

        // Stats row: question count + published status
        var statsRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 4, 0, 0)
        };

        statsRow.Children.Add(new TextBlock
        {
            Text = loc.T("main.card_questions_count", q.QuestionCount),
            FontSize = 13,
            Foreground = Res("TextTertiaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        });

        var statusBadge = new Border
        {
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 2),
            Background = q.Published
                ? Res("PublishedBadgeBrush")
                : Res("DraftBadgeBrush"),
            Opacity = 0.8
        };
        statusBadge.Child = new TextBlock
        {
            Text = q.Published ? loc.T("main.card_published") : loc.T("main.card_draft"),
            FontSize = 11,
            Foreground = Brushes.White,
            FontWeight = FontWeight.SemiBold
        };
        statsRow.Children.Add(statusBadge);
        content.Children.Add(statsRow);

        // Action buttons: Play + options menu
        var actionsRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 6, 0, 0)
        };

        var btnPlay = new Button
        {
            Padding = new Thickness(14, 8),
            CornerRadius = new CornerRadius(8),
            Classes = { "accent" }
        };
        var playContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        playContent.Children.Add(new SymbolIcon { Symbol = Symbol.Play, FontSize = 14 });
        playContent.Children.Add(new TextBlock { Text = loc.T("main.action_play"), FontSize = 13 });
        btnPlay.Content = playContent;
        btnPlay.Click += async (s, e) => await PlayQuiz(q.Id, isDemoMode: true);
        Grid.SetColumn(btnPlay, 0);
        btnPlay.HorizontalAlignment = HorizontalAlignment.Left;
        actionsRow.Children.Add(btnPlay);

        var optionsMenu = new MenuFlyout();
        optionsMenu.Items.Add(CreateMenuItem(Symbol.Edit, loc.T("main.action_edit"), () => EditQuestionnaire(q.Id)));
        optionsMenu.Items.Add(CreateMenuItem(Symbol.Document, loc.T("main.action_pdf"), () => GeneratePdfForMine(q)));
        optionsMenu.Items.Add(new Separator());
        optionsMenu.Items.Add(CreateMenuItem(Symbol.Delete, loc.T("main.action_delete"), () => DeleteQuestionnaire(q), isDanger: true));

        var btnOptions = new Button
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(8, 6),
            CornerRadius = new CornerRadius(8),
            Content = new SymbolIcon { Symbol = Symbol.More, FontSize = 18, Foreground = Res("TextTertiaryBrush") },
            Flyout = optionsMenu
        };
        Grid.SetColumn(btnOptions, 1);
        actionsRow.Children.Add(btnOptions);

        content.Children.Add(actionsRow);

        card.Child = content;
        return card;
    }

    private Border BuildPublishedQuestionnaireCard(Questionnaire q)
    {
        var loc = LocalizationManager.Instance;

        var card = new Border
        {
            Width = 340,
            Margin = new Thickness(0, 0, 16, 16),
            Background = Res("CardBackgroundBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20, 18),
            BorderBrush = Res("BorderSubtleBrush"),
            BorderThickness = new Thickness(1)
        };
        card.Classes.Add("card-hoverable");

        var content = new StackPanel { Spacing = 10 };

        // Name
        content.Children.Add(new TextBlock
        {
            Text = q.Name,
            FontSize = 16,
            FontWeight = FontWeight.SemiBold,
            Foreground = Res("TextPrimaryBrush"),
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxLines = 2
        });

        // Theme badge
        var themeName = q.Theme?.Label != null ? loc.TranslateTheme(q.Theme.Label) : "";
        if (!string.IsNullOrEmpty(themeName))
        {
            var badge = new Border
            {
                Background = Res("BadgeBackgroundBrush"),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            badge.Child = new TextBlock
            {
                Text = themeName,
                FontSize = 12,
                Foreground = Res("BadgeTextBrush")
            };
            content.Children.Add(badge);
        }

        // Stats row: question count + author
        var statsRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 4, 0, 0)
        };

        statsRow.Children.Add(new TextBlock
        {
            Text = loc.T("main.card_questions_count", q.QuestionCount),
            FontSize = 13,
            Foreground = Res("TextTertiaryBrush"),
            VerticalAlignment = VerticalAlignment.Center
        });

        var authorName = q.Owner?.FullName ?? q.Owner?.Email ?? loc.T("common.unknown");
        statsRow.Children.Add(new TextBlock
        {
            Text = loc.T("main.card_by", authorName),
            FontSize = 12,
            Foreground = Res("TextTertiaryBrush"),
            VerticalAlignment = VerticalAlignment.Center,
            FontStyle = FontStyle.Italic
        });
        content.Children.Add(statsRow);

        // Action buttons
        // Action buttons: Play + options menu
        var actionsRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0, 6, 0, 0)
        };

        var btnPlay = new Button
        {
            Padding = new Thickness(14, 8),
            CornerRadius = new CornerRadius(8),
            Classes = { "accent" }
        };
        var playContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
        playContent.Children.Add(new SymbolIcon { Symbol = Symbol.Play, FontSize = 14 });
        playContent.Children.Add(new TextBlock { Text = loc.T("main.action_play"), FontSize = 13 });
        btnPlay.Content = playContent;
        btnPlay.Click += async (s, e) => await PlayQuiz(q.Id);
        Grid.SetColumn(btnPlay, 0);
        btnPlay.HorizontalAlignment = HorizontalAlignment.Left;
        actionsRow.Children.Add(btnPlay);

        var optionsMenu = new MenuFlyout();
        optionsMenu.Items.Add(CreateMenuItem(Symbol.View, loc.T("main.action_view"), () => ViewQuestionnaire(q.Id)));
        optionsMenu.Items.Add(CreateMenuItem(Symbol.Copy, loc.T("main.action_fork"), () => ForkQuestionnaire(q)));
        optionsMenu.Items.Add(CreateMenuItem(Symbol.Document, loc.T("main.action_pdf"), async () => await GeneratePdf(q.Id, q.Name)));

        var btnOptions = new Button
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(8, 6),
            CornerRadius = new CornerRadius(8),
            Content = new SymbolIcon { Symbol = Symbol.More, FontSize = 18, Foreground = Res("TextTertiaryBrush") },
            Flyout = optionsMenu
        };
        Grid.SetColumn(btnOptions, 1);
        actionsRow.Children.Add(btnOptions);

        content.Children.Add(actionsRow);

        card.Child = content;
        return card;
    }

    private static MenuItem CreateMenuItem(Symbol icon, string text, Action onClick, bool isDanger = false)
    {
        var item = new MenuItem
        {
            Icon = new SymbolIcon { Symbol = icon, FontSize = 14 },
            Header = text
        };
        if (isDanger)
        {
            item.Foreground = Res("DangerBrush");
        }
        item.Click += (s, e) => onClick();
        return item;
    }

    #endregion

    #region Localization

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        TxtTitle.Text = loc.T("main.title");
        LblWelcome.Text = loc.T("main.welcome", _currentUser.FullName ?? _currentUser.Email);
        TxtBtnAdd.Text = loc.T("main.new_questionnaire");

        // Tab labels
        TxtTabMine.Text = loc.T("main.tab_mine");
        TxtTabPublished.Text = loc.T("main.tab_published");

        // User menu
        TxtThemeToggle.Text = _isDarkTheme ? loc.T("user_menu.light_theme") : loc.T("user_menu.dark_theme");
        TxtLanguageToggle.Text = loc.T("user_menu.language");
        TxtLogout.Text = loc.T("user_menu.logout");

        // Reload data to update translated fields
        LoadQuestionnaires();
        BuildCards();
    }

    #endregion

    #region Data loading

    private void LoadQuestionnaires()
    {
        _myQuestionnaires = _questionnaireRepository.GetByUser(_currentUser.Id);
        _publishedQuestionnaires = _questionnaireRepository.GetPublishedByOthers(_currentUser.Id);
    }

    #endregion

    #region User menu

    private void BtnUserMenu_Click(object? sender, RoutedEventArgs e)
    {
        UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
    }

    private void BtnToggleTheme_Click(object? sender, RoutedEventArgs e)
    {
        UserMenuPopup.IsOpen = false;
        _isDarkTheme = !_isDarkTheme;

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = _isDarkTheme
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        var loc = LocalizationManager.Instance;
        TxtThemeToggle.Text = _isDarkTheme ? loc.T("user_menu.light_theme") : loc.T("user_menu.dark_theme");

        SaveUserPreferences();

        // Rebuild cards to pick up new theme brushes
        BuildCards();
    }

    private void BtnToggleLanguage_Click(object? sender, RoutedEventArgs e)
    {
        UserMenuPopup.IsOpen = false;
        LocalizationManager.Instance.ToggleLanguage();

        SaveUserPreferences();
    }

    private void LoadUserPreferences()
    {
        try
        {
            var prefs = _preferencesRepository.GetByUserId(_currentUser.Id);
            if (prefs == null) return;

            _isDarkTheme = prefs.Theme == "Dark";
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = _isDarkTheme
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light;
            }

            LocalizationManager.Instance.SetLanguage(prefs.Language);
        }
        catch
        {
            // Table might not exist yet
        }
    }

    private void SaveUserPreferences()
    {
        try
        {
            var prefs = new Models.UserPreferences
            {
                UserId = _currentUser.Id,
                Theme = _isDarkTheme ? "Dark" : "Light",
                Language = LocalizationManager.Instance.CurrentLanguage
            };
            _preferencesRepository.Save(prefs);
        }
        catch
        {
            // Ignore save errors
        }
    }

    private void BtnLogout_Click(object? sender, RoutedEventArgs e)
    {
        UserMenuPopup.IsOpen = false;
        _mainWindow.Logout();
    }

    #endregion

    #region CRUD actions

    private async void BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new QuestionnaireEditorWindow(_currentUser.Id);
        var result = await dialog.ShowDialog<bool>(_mainWindow);
        if (result)
        {
            LoadQuestionnaires();
            BuildCards();
        }
    }

    private async void EditQuestionnaire(int id)
    {
        var dialog = new QuestionnaireEditorWindow(_currentUser.Id, id);
        var result = await dialog.ShowDialog<bool>(_mainWindow);
        if (result)
        {
            LoadQuestionnaires();
            BuildCards();
        }
    }

    private async void ViewQuestionnaire(int id)
    {
        var dialog = new QuestionnaireEditorWindow(_currentUser.Id, id, readOnly: true);
        await dialog.ShowDialog<bool>(_mainWindow);
    }

    private async void DeleteQuestionnaire(Questionnaire q)
    {
        var loc = LocalizationManager.Instance;
        var topLevel = TopLevel.GetTopLevel(this);
        var dialog = new ContentDialog
        {
            Title = loc.T("main.confirm_delete_title"),
            Content = loc.T("main.confirm_delete_message", q.Name),
            PrimaryButtonText = loc.T("common.delete"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync(topLevel);
        if (result == ContentDialogResult.Primary)
        {
            _questionnaireRepository.Delete(q.Id, _currentUser.Id);
            LoadQuestionnaires();
            BuildCards();
        }
    }

    #endregion

    #region Fork

    private async void ForkQuestionnaire(Questionnaire q)
    {
        var loc = LocalizationManager.Instance;
        var topLevel = TopLevel.GetTopLevel(this);
        var dialog = new ContentDialog
        {
            Title = loc.T("main.confirm_fork_title"),
            Content = loc.T("main.confirm_fork_message", q.Name),
            PrimaryButtonText = loc.T("main.action_fork"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync(topLevel);
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                _questionnaireRepository.Fork(q.Id, _currentUser.Id);
                LoadQuestionnaires();
                _showingMine = true;
                UpdateTabStyles();
                BuildCards();

                var successDialog = new ContentDialog
                {
                    Title = loc.T("main.fork_success_title"),
                    Content = loc.T("main.fork_success_message"),
                    CloseButtonText = loc.T("common.ok")
                };
                await successDialog.ShowAsync(topLevel);
            }
            catch (Exception ex)
            {
                var errorDialog = new ContentDialog
                {
                    Title = loc.T("common.error"),
                    Content = ex.Message,
                    CloseButtonText = loc.T("common.ok")
                };
                await errorDialog.ShowAsync(topLevel);
            }
        }
    }

    #endregion

    #region PDF

    private async void GeneratePdfForMine(Questionnaire q)
    {
        if (!q.Published)
        {
            var loc = LocalizationManager.Instance;
            var topLevel = TopLevel.GetTopLevel(this);
            var dialog = new ContentDialog
            {
                Title = loc.T("main.pdf_impossible_title"),
                Content = loc.T("main.pdf_impossible_message"),
                CloseButtonText = loc.T("common.ok")
            };
            await dialog.ShowAsync(topLevel);
            return;
        }

        await GeneratePdf(q.Id, q.Name);
    }

    private async Task GeneratePdf(int questionnaireId, string questionnaireName)
    {
        var questionnaire = _questionnaireRepository.GetFullById(questionnaireId);
        if (questionnaire == null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var loc = LocalizationManager.Instance;
        var suggestedFileName = $"{loc.T("pdf.filename_prefix")}_{questionnaireName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = loc.T("main.pdf_save_title"),
            SuggestedFileName = suggestedFileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType(loc.T("main.pdf_file_type")) { Patterns = new[] { "*.pdf" } }
            }
        });

        if (file == null) return;

        try
        {
            var filePath = file.Path.LocalPath;
            QuestionnairePdfGenerator.Generate(questionnaire, filePath);

            var successDialog = new ContentDialog
            {
                Title = loc.T("main.pdf_success_title"),
                Content = $"{filePath}",
                CloseButtonText = loc.T("common.ok")
            };
            await successDialog.ShowAsync(topLevel);
        }
        catch (Exception ex)
        {
            var errorDialog = new ContentDialog
            {
                Title = loc.T("main.pdf_error_title"),
                Content = ex.Message,
                CloseButtonText = loc.T("common.ok")
            };
            await errorDialog.ShowAsync(topLevel);
        }
    }

    #endregion

    #region Quiz

    private async Task PlayQuiz(int questionnaireId, bool isDemoMode = false)
    {
        var questionnaire = _questionnaireRepository.GetFullById(questionnaireId);
        if (questionnaire == null) return;

        if (questionnaire.Questions.Count == 0)
        {
            var loc = LocalizationManager.Instance;
            var topLevel = TopLevel.GetTopLevel(this);
            var dialog = new ContentDialog
            {
                Title = loc.T("main.quiz_impossible_title"),
                Content = loc.T("main.quiz_impossible_message"),
                CloseButtonText = loc.T("common.ok")
            };
            await dialog.ShowAsync(topLevel);
            return;
        }

        var quizWindow = new QuizPlayerWindow(questionnaire, isDemoMode);
        await quizWindow.ShowDialog(_mainWindow);
    }

    #endregion
}
