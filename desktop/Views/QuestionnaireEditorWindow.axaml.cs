using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class QuestionnaireEditorWindow : Window
{
    private readonly int _userId;
    private int? _questionnaireId;
    private readonly bool _readOnly;
    private readonly QuestionnaireRepository _questionnaireRepository = new();
    private readonly ThemeRepository _themeRepository = new();
    private readonly QuestionRepository _questionRepository = new();
    private readonly AnswerRepository _answerRepository = new();

    private readonly List<EditableQuestion> _editableQuestions = [];
    private readonly List<int> _deletedQuestionIds = [];
    private readonly List<int> _deletedAnswerIds = [];

    private static IBrush Res(string key)
    {
        var app = Application.Current!;
        if (app.TryFindResource(key, app.ActualThemeVariant, out var value) && value is IBrush brush)
            return brush;
        return Brushes.Transparent;
    }

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

        LoadThemes();

        if (_questionnaireId.HasValue)
        {
            LoadQuestionnaire();
        }

        if (_readOnly)
        {
            ConfigureReadOnlyMode();
        }

        // Localization
        LblName.Text = loc.T("editor.name");
        TxtName.Watermark = loc.T("editor.name_placeholder");
        LblTheme.Text = loc.T("editor.theme");
        ChkPublished.Content = loc.T("editor.published");
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
    }

    #region Theme loading

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
            CornerRadius = new CornerRadius(8)
        };

        var dialog = new ContentDialog
        {
            Title = loc.T("theme.new_title"),
            Content = new StackPanel
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

    #endregion

    #region Load questionnaire

    private void LoadQuestionnaire()
    {
        var questionnaire = _questionnaireRepository.GetFullById(_questionnaireId!.Value);
        if (questionnaire == null)
        {
            Close(false);
            return;
        }

        if (!_readOnly && questionnaire.UserId != _userId)
        {
            Close(false);
            return;
        }

        TxtName.Text = questionnaire.Name;
        ChkPublished.IsChecked = questionnaire.Published;

        var themes = CmbTheme.ItemsSource as List<Theme>;
        if (themes != null)
        {
            var themeIndex = themes.FindIndex(t => t.Id == questionnaire.ThemeId);
            if (themeIndex >= 0)
            {
                CmbTheme.SelectedIndex = themeIndex;
            }
        }

        // Load questions and answers into editable model
        _editableQuestions.Clear();
        foreach (var q in questionnaire.Questions.OrderBy(q => q.Number))
        {
            var eq = new EditableQuestion
            {
                Id = q.Id,
                Number = q.Number,
                Label = q.Label,
                AnswerType = q.AnswerType,
                Answers = []
            };

            foreach (var a in q.Answers)
            {
                eq.Answers.Add(new EditableAnswer
                {
                    Id = a.Id,
                    Label = a.Label,
                    IsCorrect = a.IsCorrect,
                    Weight = a.Weight
                });
            }

            // For TRUE_FALSE, determine which is correct
            if (q.AnswerType == AnswerType.TRUE_FALSE)
            {
                var loc = LocalizationManager.Instance;
                var trueAnswer = q.Answers.FirstOrDefault(a => a.Label == loc.T("question.true"));
                eq.TrueFalseCorrectIsTrue = trueAnswer?.IsCorrect ?? true;
                var correctAnswer = q.Answers.FirstOrDefault(a => a.IsCorrect);
                eq.TrueFalseWeight = correctAnswer?.Weight ?? 1;
            }

            _editableQuestions.Add(eq);
        }

        RebuildQuestionCards();
    }

    #endregion

    #region Build question cards

    private void RebuildQuestionCards()
    {
        QuestionsContainer.Children.Clear();

        for (int i = 0; i < _editableQuestions.Count; i++)
        {
            var card = BuildQuestionCard(_editableQuestions[i], i);
            QuestionsContainer.Children.Add(card);
        }
    }

    private Border BuildQuestionCard(EditableQuestion eq, int index)
    {
        var loc = LocalizationManager.Instance;

        // Main card border
        var card = new Border
        {
            Background = Res("CardBackgroundBrush"),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(24, 20),
            BorderBrush = Res("BorderSubtleBrush"),
            BorderThickness = new Thickness(1)
        };

        var cardContent = new StackPanel { Spacing = 16 };

        // --- Header row: Number badge | Question text | Type dropdown | Delete ---
        var headerGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto")
        };

        // Number badge
        var numberBadge = new Border
        {
            Background = Res("AccentBrush"),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 5),
            MinWidth = 36,
            VerticalAlignment = VerticalAlignment.Center
        };
        numberBadge.Child = new TextBlock
        {
            Text = (index + 1).ToString(),
            FontSize = 13,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetColumn(numberBadge, 0);
        headerGrid.Children.Add(numberBadge);

        // Question label (inline edit)
        var txtLabel = new TextBox
        {
            Text = eq.Label,
            Watermark = loc.T("editor.name_placeholder"),
            FontSize = 15,
            FontWeight = FontWeight.Medium,
            Margin = new Thickness(12, 0),
            VerticalAlignment = VerticalAlignment.Center,
            IsReadOnly = _readOnly
        };
        txtLabel.Classes.Add("inline-edit");
        eq.LabelTextBox = txtLabel;
        Grid.SetColumn(txtLabel, 1);
        headerGrid.Children.Add(txtLabel);

        // Type dropdown
        var cmbType = new ComboBox
        {
            Width = 180,
            Height = 36,
            CornerRadius = new CornerRadius(8),
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = !_readOnly
        };
        cmbType.Items.Add(new ComboBoxItem { Content = loc.T("question.type_truefalse") });
        cmbType.Items.Add(new ComboBoxItem { Content = loc.T("question.type_multiple") });
        cmbType.SelectedIndex = eq.AnswerType == AnswerType.TRUE_FALSE ? 0 : 1;
        eq.TypeComboBox = cmbType;
        Grid.SetColumn(cmbType, 2);
        headerGrid.Children.Add(cmbType);

        // Delete button
        if (!_readOnly)
        {
            var btnDelete = new Button
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(8),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 0, 0)
            };
            btnDelete.Content = new SymbolIcon
            {
                Symbol = Symbol.Delete,
                FontSize = 16,
                Foreground = Res("DangerBrush")
            };
            btnDelete.Click += (s, e) => DeleteQuestion(eq);
            Grid.SetColumn(btnDelete, 3);
            headerGrid.Children.Add(btnDelete);
        }

        cardContent.Children.Add(headerGrid);

        // --- Answers section ---
        var answersPanel = new StackPanel { Spacing = 8, Margin = new Thickness(48, 0, 0, 0) };
        eq.AnswersPanel = answersPanel;
        BuildAnswersSection(eq);
        cardContent.Children.Add(answersPanel);

        // Type change handler
        cmbType.SelectionChanged += (s, e) =>
        {
            eq.AnswerType = cmbType.SelectedIndex == 0 ? AnswerType.TRUE_FALSE : AnswerType.MULTIPLE_CHOICE;
            if (eq.AnswerType == AnswerType.TRUE_FALSE)
            {
                eq.Answers.Clear();
            }
            BuildAnswersSection(eq);
        };

        card.Child = cardContent;
        return card;
    }

    private void BuildAnswersSection(EditableQuestion eq)
    {
        if (eq.AnswersPanel == null) return;
        eq.AnswersPanel.Children.Clear();

        var loc = LocalizationManager.Instance;

        if (eq.AnswerType == AnswerType.TRUE_FALSE)
        {
            BuildTrueFalseSection(eq);
        }
        else
        {
            BuildMultipleChoiceSection(eq);
        }
    }

    private void BuildTrueFalseSection(EditableQuestion eq)
    {
        var loc = LocalizationManager.Instance;
        var panel = eq.AnswersPanel!;

        var row = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };

        // Radio buttons
        var rbPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 20,
            VerticalAlignment = VerticalAlignment.Center
        };

        var groupName = $"TF_{eq.GetHashCode()}";
        var rbTrue = new RadioButton
        {
            Content = loc.T("question.true"),
            GroupName = groupName,
            IsChecked = eq.TrueFalseCorrectIsTrue,
            FontSize = 14,
            IsEnabled = !_readOnly
        };
        var rbFalse = new RadioButton
        {
            Content = loc.T("question.false"),
            GroupName = groupName,
            IsChecked = !eq.TrueFalseCorrectIsTrue,
            FontSize = 14,
            IsEnabled = !_readOnly
        };

        rbTrue.IsCheckedChanged += (s, e) =>
        {
            if (rbTrue.IsChecked == true) eq.TrueFalseCorrectIsTrue = true;
        };
        rbFalse.IsCheckedChanged += (s, e) =>
        {
            if (rbFalse.IsChecked == true) eq.TrueFalseCorrectIsTrue = false;
        };

        eq.RbTrue = rbTrue;
        eq.RbFalse = rbFalse;

        rbPanel.Children.Add(rbTrue);
        rbPanel.Children.Add(rbFalse);
        Grid.SetColumn(rbPanel, 0);
        row.Children.Add(rbPanel);

        // Weight
        var weightPanel = new StackPanel { Spacing = 4 };
        weightPanel.Children.Add(new TextBlock
        {
            Text = loc.T("answer.weight"),
            FontSize = 12,
            Foreground = Res("TextSecondaryBrush")
        });
        var numWeight = new NumericUpDown
        {
            Width = 120,
            Minimum = 0,
            Maximum = 100,
            Increment = 0.01m,
            FormatString = "F2",
            Value = eq.TrueFalseWeight,
            HorizontalAlignment = HorizontalAlignment.Left,
            IsEnabled = !_readOnly
        };
        eq.TfWeightInput = numWeight;
        weightPanel.Children.Add(numWeight);
        Grid.SetColumn(weightPanel, 1);
        row.Children.Add(weightPanel);

        panel.Children.Add(row);
    }

    private void BuildMultipleChoiceSection(EditableQuestion eq)
    {
        var loc = LocalizationManager.Instance;
        var panel = eq.AnswersPanel!;

        // Header row
        var headerRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,60,90,36") };
        headerRow.Children.Add(CreateHeaderLabel(loc.T("answer.label"), 0));
        headerRow.Children.Add(CreateHeaderLabel(loc.T("answer.is_correct"), 1));
        headerRow.Children.Add(CreateHeaderLabel(loc.T("answer.weight"), 2));
        panel.Children.Add(headerRow);

        // Answer rows
        foreach (var ea in eq.Answers)
        {
            panel.Children.Add(BuildAnswerRow(eq, ea));
        }

        // Action buttons
        if (!_readOnly)
        {
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Margin = new Thickness(0, 4, 0, 0)
            };

            // Add answer button
            var btnAdd = new Button
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(8, 6)
            };
            var addContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
            addContent.Children.Add(new SymbolIcon
            {
                Symbol = Symbol.Add,
                FontSize = 14,
                Foreground = Res("AccentBrush")
            });
            addContent.Children.Add(new TextBlock
            {
                Text = loc.T("editor.add_answer"),
                FontSize = 13,
                Foreground = Res("AccentBrush")
            });
            btnAdd.Content = addContent;
            btnAdd.Click += (s, e) => AddAnswer(eq);
            actionsPanel.Children.Add(btnAdd);

            // Distribute points button
            if (eq.Answers.Count > 0)
            {
                var btnDistribute = new Button
                {
                    Background = Brushes.Transparent,
                    Padding = new Thickness(8, 6)
                };
                var distContent = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };
                distContent.Children.Add(new SymbolIcon
                {
                    Symbol = Symbol.Sync,
                    FontSize = 14,
                    Foreground = Res("TextTertiaryBrush")
                });
                distContent.Children.Add(new TextBlock
                {
                    Text = loc.T("editor.distribute_points"),
                    FontSize = 13,
                    Foreground = Res("TextSecondaryBrush")
                });
                btnDistribute.Content = distContent;
                btnDistribute.Click += (s, e) => DistributePoints(eq);
                actionsPanel.Children.Add(btnDistribute);
            }

            panel.Children.Add(actionsPanel);
        }
    }

    private static TextBlock CreateHeaderLabel(string text, int column)
    {
        var tb = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeight.SemiBold,
            Opacity = 0.6,
            Margin = new Thickness(4, 0)
        };
        Grid.SetColumn(tb, column);
        return tb;
    }

    private Grid BuildAnswerRow(EditableQuestion eq, EditableAnswer ea)
    {
        var row = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,60,90,36"),
            Margin = new Thickness(0, 2)
        };

        // Label
        var txtLabel = new TextBox
        {
            Text = ea.Label,
            Watermark = LocalizationManager.Instance.T("editor.answer_placeholder"),
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            IsReadOnly = _readOnly
        };
        txtLabel.Classes.Add("inline-edit");
        ea.LabelTextBox = txtLabel;
        Grid.SetColumn(txtLabel, 0);
        row.Children.Add(txtLabel);

        // Correct checkbox
        var chkCorrect = new CheckBox
        {
            IsChecked = ea.IsCorrect,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = !_readOnly
        };
        ea.CorrectCheckBox = chkCorrect;
        Grid.SetColumn(chkCorrect, 1);
        row.Children.Add(chkCorrect);

        // Weight
        var numWeight = new NumericUpDown
        {
            Width = 85,
            Minimum = -100,
            Maximum = 100,
            Increment = 0.01m,
            FormatString = "F2",
            Value = ea.Weight,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = !_readOnly
        };
        ea.WeightInput = numWeight;
        Grid.SetColumn(numWeight, 2);
        row.Children.Add(numWeight);

        // Delete button
        if (!_readOnly)
        {
            var btnDelete = new Button
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(6),
                VerticalAlignment = VerticalAlignment.Center
            };
            btnDelete.Content = new SymbolIcon
            {
                Symbol = Symbol.Delete,
                FontSize = 12,
                Foreground = Res("DangerBrush")
            };
            btnDelete.Click += (s, e) => DeleteAnswer(eq, ea);
            Grid.SetColumn(btnDelete, 3);
            row.Children.Add(btnDelete);
        }

        return row;
    }

    #endregion

    #region Question/Answer CRUD (in memory)

    private void BtnAddQuestion_Click(object? sender, RoutedEventArgs e)
    {
        if (_readOnly) return;

        var newQuestion = new EditableQuestion
        {
            Number = _editableQuestions.Count + 1,
            AnswerType = AnswerType.TRUE_FALSE
        };
        _editableQuestions.Add(newQuestion);

        var card = BuildQuestionCard(newQuestion, _editableQuestions.Count - 1);
        QuestionsContainer.Children.Add(card);

        // Scroll to the new card
        card.BringIntoView();
    }

    private void DeleteQuestion(EditableQuestion eq)
    {
        if (eq.Id.HasValue)
        {
            _deletedQuestionIds.Add(eq.Id.Value);
        }
        _editableQuestions.Remove(eq);
        RebuildQuestionCards();
    }

    private void AddAnswer(EditableQuestion eq)
    {
        ReadBackAnswerValues(eq);
        eq.Answers.Add(new EditableAnswer());
        BuildAnswersSection(eq);
    }

    private void DeleteAnswer(EditableQuestion eq, EditableAnswer ea)
    {
        if (ea.Id.HasValue)
        {
            _deletedAnswerIds.Add(ea.Id.Value);
        }
        ReadBackAnswerValues(eq);
        eq.Answers.Remove(ea);
        BuildAnswersSection(eq);
    }

    private async void DistributePoints(EditableQuestion eq)
    {
        ReadBackAnswerValues(eq);
        var loc = LocalizationManager.Instance;
        var correctAnswers = eq.Answers.Where(a => a.IsCorrect).ToList();

        if (correctAnswers.Count == 0)
        {
            var errorDialog = new ContentDialog
            {
                Title = loc.T("question.distribute_error_title"),
                Content = loc.T("question.distribute_error_message"),
                CloseButtonText = loc.T("common.ok")
            };
            await errorDialog.ShowAsync(this);
            return;
        }

        var numInput = new NumericUpDown
        {
            Minimum = 0,
            Maximum = 100,
            Increment = 0.01m,
            FormatString = "F2",
            Value = 1,
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var dialog = new ContentDialog
        {
            Title = loc.T("question.distribute_title"),
            Content = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = loc.T("question.distribute_prompt", correctAnswers.Count),
                        TextWrapping = TextWrapping.Wrap
                    },
                    numInput
                }
            },
            PrimaryButtonText = loc.T("answer.apply"),
            CloseButtonText = loc.T("common.cancel")
        };

        var result = await dialog.ShowAsync(this);
        if (result == ContentDialogResult.Primary)
        {
            decimal pointsPerCorrect = numInput.Value ?? 1;
            foreach (var ea in eq.Answers)
            {
                ea.Weight = ea.IsCorrect ? pointsPerCorrect : 0;
            }
            BuildAnswersSection(eq);
        }
    }

    #endregion

    #region Read back values from UI

    private void ReadBackAllValues()
    {
        foreach (var eq in _editableQuestions)
        {
            eq.Label = eq.LabelTextBox?.Text?.Trim() ?? "";
            eq.AnswerType = eq.TypeComboBox?.SelectedIndex == 0
                ? AnswerType.TRUE_FALSE
                : AnswerType.MULTIPLE_CHOICE;

            if (eq.AnswerType == AnswerType.TRUE_FALSE)
            {
                eq.TrueFalseCorrectIsTrue = eq.RbTrue?.IsChecked ?? true;
                eq.TrueFalseWeight = eq.TfWeightInput?.Value ?? 1;
            }
            else
            {
                ReadBackAnswerValues(eq);
            }
        }
    }

    private static void ReadBackAnswerValues(EditableQuestion eq)
    {
        foreach (var ea in eq.Answers)
        {
            ea.Label = ea.LabelTextBox?.Text?.Trim() ?? "";
            ea.IsCorrect = ea.CorrectCheckBox?.IsChecked ?? false;
            ea.Weight = ea.WeightInput?.Value ?? 0;
        }
    }

    #endregion

    #region Save

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        if (_readOnly) return;

        ReadBackAllValues();

        // Validate questionnaire metadata
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            return;
        }

        if (CmbTheme.SelectedItem is not Theme selectedTheme || selectedTheme.Id == -1)
        {
            return;
        }

        // Save/update questionnaire
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
            _questionnaireId = _questionnaireRepository.Create(questionnaire);
        }

        // Delete removed questions
        foreach (var deletedId in _deletedQuestionIds)
        {
            _questionRepository.Delete(deletedId);
        }

        // Delete removed answers
        foreach (var deletedId in _deletedAnswerIds)
        {
            _answerRepository.Delete(deletedId);
        }

        // Save each question
        var loc = LocalizationManager.Instance;
        for (int i = 0; i < _editableQuestions.Count; i++)
        {
            var eq = _editableQuestions[i];
            eq.Number = i + 1;

            var question = new Question
            {
                Id = eq.Id ?? 0,
                QuestionnaireId = _questionnaireId!.Value,
                Number = eq.Number,
                Label = eq.Label,
                AnswerType = eq.AnswerType
            };

            int questionId;
            if (eq.Id.HasValue)
            {
                _questionRepository.Update(question);
                questionId = eq.Id.Value;
            }
            else
            {
                questionId = _questionRepository.Create(question);
            }

            // Save answers
            if (eq.AnswerType == AnswerType.TRUE_FALSE)
            {
                SaveTrueFalseAnswers(questionId, eq);
            }
            else
            {
                SaveMultipleChoiceAnswers(questionId, eq);
            }
        }

        Close(true);
    }

    private void SaveTrueFalseAnswers(int questionId, EditableQuestion eq)
    {
        var loc = LocalizationManager.Instance;

        // Delete existing answers for this question (clean slate for T/F)
        _answerRepository.DeleteByQuestion(questionId);

        decimal tfWeight = eq.TrueFalseWeight;
        bool trueIsCorrect = eq.TrueFalseCorrectIsTrue;

        _answerRepository.Create(new Answer
        {
            QuestionId = questionId,
            Label = loc.T("question.true"),
            IsCorrect = trueIsCorrect,
            Weight = trueIsCorrect ? tfWeight : 0
        });
        _answerRepository.Create(new Answer
        {
            QuestionId = questionId,
            Label = loc.T("question.false"),
            IsCorrect = !trueIsCorrect,
            Weight = !trueIsCorrect ? tfWeight : 0
        });
    }

    private void SaveMultipleChoiceAnswers(int questionId, EditableQuestion eq)
    {
        foreach (var ea in eq.Answers)
        {
            if (string.IsNullOrWhiteSpace(ea.Label)) continue;

            var answer = new Answer
            {
                Id = ea.Id ?? 0,
                QuestionId = questionId,
                Label = ea.Label,
                IsCorrect = ea.IsCorrect,
                Weight = ea.Weight
            };

            if (ea.Id.HasValue)
            {
                _answerRepository.Update(answer);
            }
            else
            {
                _answerRepository.Create(answer);
            }
        }
    }

    #endregion

    #region Editable models

    private class EditableQuestion
    {
        public int? Id { get; set; }
        public int Number { get; set; }
        public string Label { get; set; } = string.Empty;
        public AnswerType AnswerType { get; set; }
        public bool TrueFalseCorrectIsTrue { get; set; } = true;
        public decimal TrueFalseWeight { get; set; } = 1;
        public List<EditableAnswer> Answers { get; set; } = [];

        // UI references
        public TextBox? LabelTextBox { get; set; }
        public ComboBox? TypeComboBox { get; set; }
        public RadioButton? RbTrue { get; set; }
        public RadioButton? RbFalse { get; set; }
        public NumericUpDown? TfWeightInput { get; set; }
        public StackPanel? AnswersPanel { get; set; }
    }

    private class EditableAnswer
    {
        public int? Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public decimal Weight { get; set; }

        // UI references
        public TextBox? LabelTextBox { get; set; }
        public CheckBox? CorrectCheckBox { get; set; }
        public NumericUpDown? WeightInput { get; set; }
    }

    #endregion
}
