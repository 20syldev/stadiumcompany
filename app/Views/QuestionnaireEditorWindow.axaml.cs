using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;
using System.Collections.ObjectModel;

namespace StadiumCompany.Views;

public partial class QuestionnaireEditorWindow : Window
{
    private readonly int _userId;
    private int? _questionnaireId;
    private readonly bool _readOnly;
    private readonly QuestionnaireRepository _questionnaireRepository = new();
    private readonly ThemeRepository _themeRepository = new();
    private readonly QuestionRepository _questionRepository = new();

    private Questionnaire? _questionnaire;

    public ObservableCollection<QuestionViewModel> Questions { get; } = new();

    public QuestionnaireEditorWindow()
    {
        InitializeComponent();
    }

    public QuestionnaireEditorWindow(int userId, int? questionnaireId = null, bool readOnly = false)
    {
        InitializeComponent();
        _userId = userId;
        _questionnaireId = questionnaireId;
        _readOnly = readOnly;

        var loc = LocalizationManager.Instance;

        if (_questionnaireId.HasValue)
        {
            Title = readOnly ? loc.T("editor.view_questionnaire") : loc.T("editor.edit_questionnaire");
            TitleText.Text = Title;
        }
        else
        {
            Title = loc.T("editor.new_questionnaire");
            TitleText.Text = Title;
        }

        DgvQuestions.ItemsSource = Questions;
        DgvQuestions.PointerPressed += DataGrid_RightClickSelect;
        LoadThemes();

        if (_questionnaireId.HasValue)
        {
            LoadQuestionnaire();
        }

        if (_readOnly)
        {
            ConfigureReadOnlyMode();
        }

        BtnAddQuestion.IsEnabled = !_readOnly;

        // Localization
        LblName.Text = loc.T("editor.name");
        TxtName.Watermark = loc.T("editor.name_placeholder");
        LblTheme.Text = loc.T("editor.theme");
        ChkPublished.Content = loc.T("editor.published");
        LblQuestions.Text = loc.T("editor.questions");
        TxtBtnAddQuestion.Text = loc.T("editor.add_question");
        BtnCancel.Content = readOnly ? loc.T("editor.close") : loc.T("editor.cancel");
        BtnSave.Content = loc.T("editor.save");
    }

    private void ConfigureReadOnlyMode()
    {
        TxtName.IsEnabled = false;
        CmbTheme.IsEnabled = false;
        ChkPublished.IsEnabled = false;
        BtnAddQuestion.IsVisible = false;
        BtnSave.IsVisible = false;
        DgvQuestions.ContextMenu = null;
    }

    private static readonly Theme _otherThemeSentinel = new() { Id = -1, Label = "Autre..." };
    private bool _isHandlingThemeSelection = false;

    private void LoadThemes()
    {
        var loc = LocalizationManager.Instance;
        var themes = _themeRepository.GetAll();
        _otherThemeSentinel.Label = loc.T("theme.other");
        themes.Add(_otherThemeSentinel);
        CmbTheme.ItemsSource = themes;
        CmbTheme.DisplayMemberBinding = new Avalonia.Data.Binding("Label");
        if (themes.Count > 1)
        {
            CmbTheme.SelectedIndex = 0;
        }
        CmbTheme.SelectionChanged += CmbTheme_SelectionChanged;
    }

    private async void CmbTheme_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isHandlingThemeSelection) return;
        if (CmbTheme.SelectedItem is not Theme selected || selected.Id != -1) return;

        _isHandlingThemeSelection = true;

        var loc = LocalizationManager.Instance;
        var textInput = new TextBox
        {
            Watermark = loc.T("theme.new_prompt"),
            Height = 40,
            CornerRadius = new Avalonia.CornerRadius(8)
        };

        var dialog = new ContentDialog
        {
            Title = loc.T("theme.new_title"),
            Content = new Avalonia.Controls.StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = loc.T("theme.new_prompt") },
                    textInput
                }
            },
            PrimaryButtonText = loc.T("theme.create"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync(this);

        if (result == ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(textInput.Text))
        {
            var newTheme = _themeRepository.Create(textInput.Text.Trim());
            CmbTheme.SelectionChanged -= CmbTheme_SelectionChanged;
            var themes = _themeRepository.GetAll();
            themes.Add(_otherThemeSentinel);
            CmbTheme.ItemsSource = themes;
            var idx = themes.FindIndex(t => t.Id == newTheme.Id);
            CmbTheme.SelectedIndex = idx >= 0 ? idx : 0;
            CmbTheme.SelectionChanged += CmbTheme_SelectionChanged;
        }
        else
        {
            CmbTheme.SelectedIndex = 0;
        }

        _isHandlingThemeSelection = false;
    }

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

    private void LoadQuestionnaire()
    {
        _questionnaire = _questionnaireRepository.GetById(_questionnaireId!.Value);
        if (_questionnaire == null)
        {
            Close(false);
            return;
        }

        if (!_readOnly && _questionnaire.UserId != _userId)
        {
            Close(false);
            return;
        }

        TxtName.Text = _questionnaire.Name;
        ChkPublished.IsChecked = _questionnaire.Published;

        var themes = CmbTheme.ItemsSource as List<Theme>;
        if (themes != null)
        {
            var themeIndex = themes.FindIndex(t => t.Id == _questionnaire.ThemeId);
            if (themeIndex >= 0)
            {
                CmbTheme.SelectedIndex = themeIndex;
            }
        }

        LoadQuestions();
    }

    private void LoadQuestions()
    {
        if (!_questionnaireId.HasValue) return;

        Questions.Clear();
        var questions = _questionRepository.GetByQuestionnaire(_questionnaireId.Value);
        foreach (var q in questions)
        {
            Questions.Add(new QuestionViewModel(q));
        }
    }

    private async void BtnAddQuestion_Click(object? sender, RoutedEventArgs e)
    {
        if (_readOnly) return;

        if (!_questionnaireId.HasValue)
        {
            if (!SaveQuestionnaireFirst()) return;
        }

        var dialog = new QuestionEditorWindow(_questionnaireId!.Value);
        var result = await dialog.ShowDialog<bool>(this);
        if (result)
        {
            LoadQuestions();
        }
    }

    private bool SaveQuestionnaireFirst()
    {
        if (string.IsNullOrWhiteSpace(TxtName.Text)) return false;
        if (CmbTheme.SelectedItem is not Theme selectedTheme || selectedTheme.Id == -1) return false;

        var questionnaire = new Questionnaire
        {
            Name = TxtName.Text.Trim(),
            ThemeId = selectedTheme.Id,
            UserId = _userId,
            Published = ChkPublished.IsChecked ?? false
        };

        var newId = _questionnaireRepository.Create(questionnaire);
        _questionnaireId = newId;
        _questionnaire = questionnaire;
        _questionnaire.Id = newId;

        var loc = LocalizationManager.Instance;
        Title = loc.T("editor.edit_questionnaire");
        TitleText.Text = Title;

        return true;
    }

    private void DgvQuestions_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_readOnly) return;
        EditSelectedQuestion();
    }

    private void ContextMenu_Edit(object? sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        EditSelectedQuestion();
    }

    private void ContextMenu_Delete(object? sender, RoutedEventArgs e)
    {
        if (_readOnly) return;
        if (DgvQuestions.SelectedItem is not QuestionViewModel selected) return;

        _questionRepository.Delete(selected.Id);
        LoadQuestions();
    }

    private async void EditSelectedQuestion()
    {
        if (DgvQuestions.SelectedItem is not QuestionViewModel selected) return;

        var dialog = new QuestionEditorWindow(_questionnaireId!.Value, selected.Id);
        var result = await dialog.ShowDialog<bool>(this);
        if (result)
        {
            LoadQuestions();
        }
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        if (_readOnly) return;

        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            return;
        }

        if (CmbTheme.SelectedItem is not Theme selectedTheme || selectedTheme.Id == -1)
        {
            return;
        }

        var questionnaire = new Questionnaire
        {
            Id = _questionnaireId ?? 0,
            Name = TxtName.Text.Trim(),
            ThemeId = selectedTheme.Id,
            UserId = _userId,
            Published = ChkPublished.IsChecked ?? false
        };

        if (_questionnaireId.HasValue)
        {
            _questionnaireRepository.Update(questionnaire, _userId);
        }
        else
        {
            _questionnaireRepository.Create(questionnaire);
        }

        Close(true);
    }
}

public class QuestionViewModel
{
    public int Id { get; }
    public int Number { get; }
    public string Label { get; }
    public string AnswerTypeText { get; }

    public QuestionViewModel(Question q)
    {
        var loc = LocalizationManager.Instance;
        Id = q.Id;
        Number = q.Number;
        Label = q.Label;
        AnswerTypeText = q.AnswerType == AnswerType.TRUE_FALSE ? loc.T("question.type_truefalse") : loc.T("question.type_multiple");
    }
}
