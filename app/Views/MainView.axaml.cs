using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;
using StadiumCompany.Services.PdfGenerator;
using System.Collections.ObjectModel;

namespace StadiumCompany.Views;

public partial class MainView : UserControl
{
    private readonly MainWindow _mainWindow = null!;
    private readonly User _currentUser = null!;
    private readonly QuestionnaireRepository _questionnaireRepository = new();
    private readonly UserPreferencesRepository _preferencesRepository = new();
    private bool _isDarkTheme = false;

    public ObservableCollection<QuestionnaireViewModel> MyQuestionnaires { get; } = new();
    public ObservableCollection<PublishedQuestionnaireViewModel> PublishedQuestionnaires { get; } = new();

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

        DgvMyQuestionnaires.ItemsSource = MyQuestionnaires;
        DgvPublishedQuestionnaires.ItemsSource = PublishedQuestionnaires;

        DgvMyQuestionnaires.DoubleTapped += (s, e) => EditSelectedQuestionnaire();
        DgvPublishedQuestionnaires.DoubleTapped += (s, e) => ViewSelectedPublishedQuestionnaire();

        // Right-click auto-select
        DgvMyQuestionnaires.PointerPressed += DataGrid_RightClickSelect;
        DgvPublishedQuestionnaires.PointerPressed += DataGrid_RightClickSelect;

        // Selection changed -> show/hide action bar
        DgvMyQuestionnaires.SelectionChanged += (s, e) => PnlMyActions.IsVisible = DgvMyQuestionnaires.SelectedItem != null;
        DgvPublishedQuestionnaires.SelectionChanged += (s, e) => PnlPubActions.IsVisible = DgvPublishedQuestionnaires.SelectedItem != null;

        // Load saved preferences
        LoadUserPreferences();

        // Localization
        loc.LanguageChanged += UpdateTexts;
        UpdateTexts();

        LoadQuestionnaires();
    }

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        TxtTitle.Text = loc.T("main.title");
        LblWelcome.Text = loc.T("main.welcome", _currentUser.FullName ?? _currentUser.Email);
        TxtBtnAdd.Text = loc.T("main.new_questionnaire");

        // Tabs
        TabMine.Header = loc.T("main.tab_mine");
        TabPublished.Header = loc.T("main.tab_published");

        // Column headers - Mine (by index)
        if (DgvMyQuestionnaires.Columns.Count >= 4)
        {
            DgvMyQuestionnaires.Columns[0].Header = loc.T("main.col_name");
            DgvMyQuestionnaires.Columns[1].Header = loc.T("main.col_theme");
            DgvMyQuestionnaires.Columns[2].Header = loc.T("main.col_questions");
            DgvMyQuestionnaires.Columns[3].Header = loc.T("main.col_published");
        }

        // Column headers - Published (by index)
        if (DgvPublishedQuestionnaires.Columns.Count >= 4)
        {
            DgvPublishedQuestionnaires.Columns[0].Header = loc.T("main.col_name");
            DgvPublishedQuestionnaires.Columns[1].Header = loc.T("main.col_theme");
            DgvPublishedQuestionnaires.Columns[2].Header = loc.T("main.col_author");
            DgvPublishedQuestionnaires.Columns[3].Header = loc.T("main.col_questions");
        }

        // Context menus - Mine
        CtxMinePlay.Header = loc.T("main.action_play");
        CtxMineEdit.Header = loc.T("main.action_edit");
        CtxMinePdf.Header = loc.T("main.action_pdf");
        CtxMineDelete.Header = loc.T("main.action_delete");

        // Context menus - Published
        CtxPubPlay.Header = loc.T("main.action_play");
        CtxPubView.Header = loc.T("main.action_view");
        CtxPubFork.Header = loc.T("main.action_fork");
        CtxPubPdf.Header = loc.T("main.action_pdf");

        // Action bars
        TxtActionPlay.Text = loc.T("main.action_play");
        TxtActionEdit.Text = loc.T("main.action_edit");
        TxtActionPdf.Text = loc.T("main.action_pdf");
        TxtActionDelete.Text = loc.T("main.action_delete");
        TxtActionPlayPub.Text = loc.T("main.action_play");
        TxtActionView.Text = loc.T("main.action_view");
        TxtActionFork.Text = loc.T("main.action_fork");
        TxtActionPdfPub.Text = loc.T("main.action_pdf");

        // User menu
        TxtThemeToggle.Text = _isDarkTheme ? loc.T("user_menu.light_theme") : loc.T("user_menu.dark_theme");
        TxtLanguageToggle.Text = loc.T("user_menu.language");
        TxtLogout.Text = loc.T("user_menu.logout");

        // Reload data to update translated fields (Published yes/no, theme labels)
        LoadQuestionnaires();
    }

    // --- Right-click auto-select ---
    private void DataGrid_RightClickSelect(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not DataGrid dg) return;
        var point = e.GetCurrentPoint(dg);
        if (!point.Properties.IsRightButtonPressed) return;

        var pos = e.GetPosition(dg);
        var element = dg.InputHitTest(pos) as Visual;
        while (element != null && element is not DataGridRow)
            element = element.GetVisualParent() as Visual;
        if (element is DataGridRow row)
            dg.SelectedIndex = row.Index;
    }

    private void LoadQuestionnaires()
    {
        LoadMyQuestionnaires();
        LoadPublishedQuestionnaires();
    }

    private void LoadMyQuestionnaires()
    {
        MyQuestionnaires.Clear();
        var questionnaires = _questionnaireRepository.GetByUser(_currentUser.Id);
        foreach (var q in questionnaires)
        {
            MyQuestionnaires.Add(new QuestionnaireViewModel(q));
        }
    }

    private void LoadPublishedQuestionnaires()
    {
        PublishedQuestionnaires.Clear();
        var questionnaires = _questionnaireRepository.GetPublishedByOthers(_currentUser.Id);
        foreach (var q in questionnaires)
        {
            PublishedQuestionnaires.Add(new PublishedQuestionnaireViewModel(q));
        }
    }

    // --- User menu ---
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

            // Apply theme
            _isDarkTheme = prefs.Theme == "Dark";
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = _isDarkTheme
                    ? ThemeVariant.Dark
                    : ThemeVariant.Light;
            }

            // Apply language
            LocalizationManager.Instance.SetLanguage(prefs.Language);
        }
        catch
        {
            // Table might not exist yet — ignore silently
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
            // Ignore save errors silently
        }
    }

    private void BtnLogout_Click(object? sender, RoutedEventArgs e)
    {
        UserMenuPopup.IsOpen = false;
        _mainWindow.Logout();
    }

    // --- CRUD ---
    private async void BtnAdd_Click(object? sender, RoutedEventArgs e)
    {
        var dialog = new QuestionnaireEditorWindow(_currentUser.Id);
        var result = await dialog.ShowDialog<bool>(_mainWindow);
        if (result)
        {
            LoadMyQuestionnaires();
        }
    }

    private void ContextMenu_Edit(object? sender, RoutedEventArgs e)
    {
        EditSelectedQuestionnaire();
    }

    private async void ContextMenu_Delete(object? sender, RoutedEventArgs e)
    {
        if (DgvMyQuestionnaires.SelectedItem is not QuestionnaireViewModel selected) return;

        var loc = LocalizationManager.Instance;
        var topLevel = TopLevel.GetTopLevel(this);
        var dialog = new ContentDialog
        {
            Title = loc.T("main.confirm_delete_title"),
            Content = loc.T("main.confirm_delete_message", selected.Name),
            PrimaryButtonText = loc.T("common.delete"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync(topLevel);
        if (result == ContentDialogResult.Primary)
        {
            _questionnaireRepository.Delete(selected.Id, _currentUser.Id);
            LoadMyQuestionnaires();
        }
    }

    private async void EditSelectedQuestionnaire()
    {
        if (DgvMyQuestionnaires.SelectedItem is not QuestionnaireViewModel selected) return;

        var dialog = new QuestionnaireEditorWindow(_currentUser.Id, selected.Id);
        var result = await dialog.ShowDialog<bool>(_mainWindow);
        if (result)
        {
            LoadMyQuestionnaires();
        }
    }

    private void ContextMenu_View(object? sender, RoutedEventArgs e)
    {
        ViewSelectedPublishedQuestionnaire();
    }

    private async void ViewSelectedPublishedQuestionnaire()
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;

        var dialog = new QuestionnaireEditorWindow(_currentUser.Id, selected.Id, readOnly: true);
        await dialog.ShowDialog<bool>(_mainWindow);
    }

    // --- Action bar handlers (mine) ---
    private async void BtnActionPlay_Click(object? sender, RoutedEventArgs e)
    {
        if (DgvMyQuestionnaires.SelectedItem is not QuestionnaireViewModel selected) return;
        await PlayQuiz(selected.Id, isDemoMode: true);
    }

    private void BtnActionEdit_Click(object? sender, RoutedEventArgs e)
    {
        EditSelectedQuestionnaire();
    }

    private async void BtnActionPdf_Click(object? sender, RoutedEventArgs e)
    {
        if (DgvMyQuestionnaires.SelectedItem is not QuestionnaireViewModel selected) return;

        var questionnaire = _questionnaireRepository.GetById(selected.Id);
        if (questionnaire == null || !questionnaire.Published)
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

        await GeneratePdf(selected.Id, selected.Name);
    }

    private void BtnActionDelete_Click(object? sender, RoutedEventArgs e)
    {
        ContextMenu_Delete(sender, e);
    }

    // --- Action bar handlers (published) ---
    private async void BtnActionPlayPub_Click(object? sender, RoutedEventArgs e)
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;
        await PlayQuiz(selected.Id);
    }

    private void BtnActionViewPub_Click(object? sender, RoutedEventArgs e)
    {
        ViewSelectedPublishedQuestionnaire();
    }

    private async void BtnActionForkPub_Click(object? sender, RoutedEventArgs e)
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;
        await ForkQuestionnaire(selected);
    }

    private async void BtnActionPdfPub_Click(object? sender, RoutedEventArgs e)
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;
        await GeneratePdf(selected.Id, selected.Name);
    }

    // --- Fork ---
    private async void ContextMenu_Fork(object? sender, RoutedEventArgs e)
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;
        await ForkQuestionnaire(selected);
    }

    private async Task ForkQuestionnaire(PublishedQuestionnaireViewModel selected)
    {
        var loc = LocalizationManager.Instance;
        var topLevel = TopLevel.GetTopLevel(this);
        var dialog = new ContentDialog
        {
            Title = loc.T("main.confirm_fork_title"),
            Content = loc.T("main.confirm_fork_message", selected.Name),
            PrimaryButtonText = loc.T("main.action_fork"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync(topLevel);
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                _questionnaireRepository.Fork(selected.Id, _currentUser.Id);
                LoadMyQuestionnaires();
                TabMain.SelectedIndex = 0;

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

    // --- PDF ---
    private async void ContextMenu_GeneratePdf(object? sender, RoutedEventArgs e)
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;
        await GeneratePdf(selected.Id, selected.Name);
    }

    private async void ContextMenu_GeneratePdfMine(object? sender, RoutedEventArgs e)
    {
        if (DgvMyQuestionnaires.SelectedItem is not QuestionnaireViewModel selected) return;

        var questionnaire = _questionnaireRepository.GetById(selected.Id);
        if (questionnaire == null || !questionnaire.Published)
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

        await GeneratePdf(selected.Id, selected.Name);
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

    // --- Quiz ---
    private async void ContextMenu_PlayMine(object? sender, RoutedEventArgs e)
    {
        if (DgvMyQuestionnaires.SelectedItem is not QuestionnaireViewModel selected) return;
        await PlayQuiz(selected.Id, isDemoMode: true);
    }

    private async void ContextMenu_Play(object? sender, RoutedEventArgs e)
    {
        if (DgvPublishedQuestionnaires.SelectedItem is not PublishedQuestionnaireViewModel selected) return;
        await PlayQuiz(selected.Id);
    }

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
}

public class QuestionnaireViewModel
{
    public int Id { get; }
    public string Name { get; }
    public string ThemeLabel { get; }
    public int QuestionCount { get; }
    public string PublishedText { get; }

    public QuestionnaireViewModel(Questionnaire q)
    {
        var loc = LocalizationManager.Instance;
        Id = q.Id;
        Name = q.Name;
        ThemeLabel = q.Theme?.Label != null ? loc.TranslateTheme(q.Theme.Label) : "";
        QuestionCount = q.QuestionCount;
        PublishedText = q.Published ? loc.T("main.published_yes") : loc.T("main.published_no");
    }
}

public class PublishedQuestionnaireViewModel
{
    public int Id { get; }
    public string Name { get; }
    public string ThemeLabel { get; }
    public string OwnerName { get; }
    public int QuestionCount { get; }

    public PublishedQuestionnaireViewModel(Questionnaire q)
    {
        var loc = LocalizationManager.Instance;
        Id = q.Id;
        Name = q.Name;
        ThemeLabel = q.Theme?.Label != null ? loc.TranslateTheme(q.Theme.Label) : "";
        OwnerName = q.Owner?.FullName ?? q.Owner?.Email ?? loc.T("common.unknown");
        QuestionCount = q.QuestionCount;
    }
}
