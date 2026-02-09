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
using System.Linq;

namespace StadiumCompany.Views;

public partial class QuestionEditorWindow : Window
{
    private readonly int _questionnaireId;
    private int? _questionId;
    private readonly QuestionRepository _questionRepository = new();
    private readonly AnswerRepository _answerRepository = new();

    private Question? _question;

    public ObservableCollection<AnswerViewModel> Answers { get; } = new();

    public QuestionEditorWindow()
    {
        InitializeComponent();
    }

    public QuestionEditorWindow(int questionnaireId, int? questionId = null)
    {
        InitializeComponent();
        _questionnaireId = questionnaireId;
        _questionId = questionId;

        var loc = LocalizationManager.Instance;

        if (_questionId.HasValue)
        {
            Title = loc.T("question.edit");
            TitleText.Text = Title;
        }
        else
        {
            Title = loc.T("question.new");
            TitleText.Text = Title;
        }

        CmbType.SelectedIndex = 0;
        DgvAnswers.ItemsSource = Answers;
        DgvAnswers.PointerPressed += DataGrid_RightClickSelect;

        if (_questionId.HasValue)
        {
            LoadQuestion();
        }
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

    private void LoadQuestion()
    {
        _question = _questionRepository.GetById(_questionId!.Value);
        if (_question == null)
        {
            Close(false);
            return;
        }

        TxtLabel.Text = _question.Label;
        CmbType.SelectedIndex = _question.AnswerType == AnswerType.TRUE_FALSE ? 0 : 1;

        if (_question.AnswerType == AnswerType.TRUE_FALSE)
        {
            var answers = _answerRepository.GetByQuestion(_questionId!.Value);
            var loc = LocalizationManager.Instance;
            var trueAnswer = answers.FirstOrDefault(a => a.Label == loc.T("question.true"));
            if (trueAnswer != null)
            {
                RbTrue.IsChecked = trueAnswer.IsCorrect;
                RbFalse.IsChecked = !trueAnswer.IsCorrect;
            }
            var correctAnswer = answers.FirstOrDefault(a => a.IsCorrect);
            if (correctAnswer != null)
            {
                NumTfWeight.Value = correctAnswer.Weight;
            }
        }

        LoadAnswers();
    }

    private void LoadAnswers()
    {
        if (!_questionId.HasValue) return;

        Answers.Clear();
        var answers = _answerRepository.GetByQuestion(_questionId.Value);
        foreach (var a in answers)
        {
            Answers.Add(new AnswerViewModel(a));
        }
    }

    private void CmbType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        bool isTrueFalse = CmbType.SelectedIndex == 0;
        PnlTrueFalse.IsVisible = isTrueFalse;
        PnlAnswers.IsVisible = !isTrueFalse;
    }

    private async void BtnAddAnswer_Click(object? sender, RoutedEventArgs e)
    {
        if (!_questionId.HasValue)
        {
            if (!SaveQuestionFirst()) return;
        }

        var dialog = new AnswerEditorWindow(_questionId!.Value);
        var result = await dialog.ShowDialog<bool>(this);
        if (result)
        {
            LoadAnswers();
        }
    }

    private bool SaveQuestionFirst()
    {
        if (string.IsNullOrWhiteSpace(TxtLabel.Text))
        {
            return false;
        }

        var question = new Question
        {
            QuestionnaireId = _questionnaireId,
            Number = _questionRepository.GetNextNumber(_questionnaireId),
            Label = TxtLabel.Text.Trim(),
            AnswerType = AnswerType.MULTIPLE_CHOICE
        };

        var newId = _questionRepository.Create(question);
        _questionId = newId;
        _question = question;
        _question.Id = newId;

        var loc = LocalizationManager.Instance;
        Title = loc.T("question.edit");
        TitleText.Text = Title;

        return true;
    }

    private void DgvAnswers_DoubleTapped(object? sender, TappedEventArgs e)
    {
        EditSelectedAnswer();
    }

    private void ContextMenu_Edit(object? sender, RoutedEventArgs e)
    {
        EditSelectedAnswer();
    }

    private void ContextMenu_Delete(object? sender, RoutedEventArgs e)
    {
        if (DgvAnswers.SelectedItem is not AnswerViewModel selected) return;

        _answerRepository.Delete(selected.Id);
        LoadAnswers();
    }

    private async void EditSelectedAnswer()
    {
        if (DgvAnswers.SelectedItem is not AnswerViewModel selected) return;

        var dialog = new AnswerEditorWindow(_questionId!.Value, selected.Id);
        var result = await dialog.ShowDialog<bool>(this);
        if (result)
        {
            LoadAnswers();
        }
    }

    private async void BtnDistribute_Click(object? sender, RoutedEventArgs e)
    {
        if (!_questionId.HasValue) return;

        var loc = LocalizationManager.Instance;
        var answers = _answerRepository.GetByQuestion(_questionId.Value);
        var correctAnswers = answers.Where(a => a.IsCorrect).ToList();

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
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };

        var dialog = new ContentDialog
        {
            Title = loc.T("question.distribute_title"),
            Content = new Avalonia.Controls.StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = loc.T("question.distribute_prompt", correctAnswers.Count),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
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

            foreach (var answer in answers)
            {
                answer.Weight = answer.IsCorrect ? pointsPerCorrect : 0;
                _answerRepository.Update(answer);
            }

            LoadAnswers();
        }
    }

    private void BtnCancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void BtnSave_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtLabel.Text))
        {
            return;
        }

        var answerType = CmbType.SelectedIndex == 0 ? AnswerType.TRUE_FALSE : AnswerType.MULTIPLE_CHOICE;
        bool trueIsCorrect = RbTrue.IsChecked ?? true;

        var question = new Question
        {
            Id = _questionId ?? 0,
            QuestionnaireId = _questionnaireId,
            Number = _questionId.HasValue ? _question!.Number : _questionRepository.GetNextNumber(_questionnaireId),
            Label = TxtLabel.Text.Trim(),
            AnswerType = answerType
        };

        if (_questionId.HasValue)
        {
            _questionRepository.Update(question);

            if (answerType == AnswerType.TRUE_FALSE)
            {
                var loc = LocalizationManager.Instance;
                decimal tfWeight = NumTfWeight.Value ?? 1;
                var answers = _answerRepository.GetByQuestion(_questionId.Value);
                foreach (var answer in answers)
                {
                    answer.IsCorrect = (answer.Label == loc.T("question.true")) == trueIsCorrect;
                    answer.Weight = answer.IsCorrect ? tfWeight : 0;
                    _answerRepository.Update(answer);
                }
            }
        }
        else
        {
            var loc = LocalizationManager.Instance;
            var newId = _questionRepository.Create(question);

            if (answerType == AnswerType.TRUE_FALSE)
            {
                decimal tfWeight = NumTfWeight.Value ?? 1;
                _answerRepository.Create(new Answer { QuestionId = newId, Label = loc.T("question.true"), IsCorrect = trueIsCorrect, Weight = trueIsCorrect ? tfWeight : 0 });
                _answerRepository.Create(new Answer { QuestionId = newId, Label = loc.T("question.false"), IsCorrect = !trueIsCorrect, Weight = !trueIsCorrect ? tfWeight : 0 });
            }
        }

        Close(true);
    }
}

public class AnswerViewModel
{
    public int Id { get; }
    public string Label { get; }
    public string IsCorrectText { get; }
    public decimal Weight { get; }

    public AnswerViewModel(Answer a)
    {
        var loc = LocalizationManager.Instance;
        Id = a.Id;
        Label = a.Label;
        IsCorrectText = a.IsCorrect ? loc.T("common.yes") : loc.T("common.no");
        Weight = a.Weight;
    }
}
