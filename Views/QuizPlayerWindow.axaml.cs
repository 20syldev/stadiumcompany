using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class QuizPlayerWindow : Window
{
    private readonly Questionnaire _questionnaire;
    private readonly List<Question> _questions;
    private int _currentIndex = 0;
    private readonly Dictionary<int, List<int>> _selectedAnswers = new();
    private readonly bool _isDemoMode;

    public QuizPlayerWindow()
    {
        InitializeComponent();
        _questionnaire = null!;
        _questions = new List<Question>();
    }

    public QuizPlayerWindow(Questionnaire questionnaire, bool isDemoMode = false)
    {
        InitializeComponent();
        _questionnaire = questionnaire;
        _isDemoMode = isDemoMode;
        _questions = questionnaire.Questions.OrderBy(q => q.Number).ToList();

        var loc = LocalizationManager.Instance;
        loc.LanguageChanged += UpdateTexts;

        TxtQuizName.Text = questionnaire.Name;
        TxtTheme.Text = string.Format(loc.T("quiz.theme"), questionnaire.Theme?.Label ?? "N/A");

        if (isDemoMode)
        {
            PnlDemoBanner.IsVisible = true;
            Title = loc.T("quiz.demo_title");
            TxtDemoBanner.Text = loc.T("quiz.demo_banner");
        }

        UpdateTexts();

        if (_questions.Count > 0)
        {
            ShowQuestion(0);
        }
        else
        {
            ShowResults();
        }

        UpdateNavigationButtons();
    }

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        TxtBtnPrevious.Text = loc.T("quiz.previous");
        TxtBtnNext.Text = loc.T("quiz.next");
        TxtBtnFinish.Text = loc.T("quiz.finish");
        BtnClose.Content = loc.T("quiz.close");
        TxtResultsTitle.Text = loc.T("quiz.results_title");
        TxtScoreLabel.Text = loc.T("quiz.score");

        TxtTheme.Text = string.Format(loc.T("quiz.theme"), _questionnaire.Theme?.Label ?? "N/A");

        if (_isDemoMode)
        {
            Title = loc.T("quiz.demo_title");
            TxtDemoBanner.Text = loc.T("quiz.demo_banner");
        }

        // Update current question type text if visible
        if (_currentIndex >= 0 && _currentIndex < _questions.Count)
        {
            var question = _questions[_currentIndex];
            TxtQuestionType.Text = question.AnswerType == AnswerType.TRUE_FALSE
                ? loc.T("question.type_truefalse")
                : loc.T("question.type_multiple");
            TxtProgress.Text = string.Format(loc.T("quiz.progress"), _currentIndex + 1, _questions.Count);
        }
    }

    private void ShowQuestion(int index)
    {
        if (index < 0 || index >= _questions.Count) return;

        _currentIndex = index;
        var question = _questions[index];
        var loc = LocalizationManager.Instance;

        TxtProgress.Text = string.Format(loc.T("quiz.progress"), index + 1, _questions.Count);
        TxtQuestionNumber.Text = question.Number.ToString();
        TxtQuestionLabel.Text = question.Label;
        TxtQuestionType.Text = question.AnswerType == AnswerType.TRUE_FALSE
            ? loc.T("question.type_truefalse")
            : loc.T("question.type_multiple");

        PnlAnswers.Children.Clear();

        var selectedIds = _selectedAnswers.GetValueOrDefault(question.Id, new List<int>());

        foreach (var answer in question.Answers)
        {
            var isSelected = selectedIds.Contains(answer.Id);

            if (question.AnswerType == AnswerType.TRUE_FALSE)
            {
                var radioButton = new RadioButton
                {
                    Content = answer.Label,
                    Tag = answer.Id,
                    GroupName = $"Question_{question.Id}",
                    IsChecked = isSelected,
                    FontSize = 14,
                    Padding = new Thickness(10, 6),
                    Margin = new Thickness(0, 2)
                };
                radioButton.IsCheckedChanged += (s, e) =>
                {
                    if (radioButton.IsChecked == true)
                        OnAnswerSelected(question.Id, answer.Id, true);
                };
                PnlAnswers.Children.Add(radioButton);
            }
            else
            {
                var checkBox = new CheckBox
                {
                    Content = answer.Label,
                    Tag = answer.Id,
                    IsChecked = isSelected,
                    FontSize = 14,
                    Padding = new Thickness(10, 6),
                    Margin = new Thickness(0, 2)
                };
                checkBox.IsCheckedChanged += (s, e) =>
                {
                    if (checkBox.IsChecked == true)
                        OnAnswerSelected(question.Id, answer.Id, false);
                    else
                        OnAnswerDeselected(question.Id, answer.Id);
                };
                PnlAnswers.Children.Add(checkBox);
            }
        }

        UpdateNavigationButtons();
    }

    private void OnAnswerSelected(int questionId, int answerId, bool isSingleChoice)
    {
        if (isSingleChoice)
        {
            _selectedAnswers[questionId] = new List<int> { answerId };
        }
        else
        {
            if (!_selectedAnswers.ContainsKey(questionId))
            {
                _selectedAnswers[questionId] = new List<int>();
            }
            if (!_selectedAnswers[questionId].Contains(answerId))
            {
                _selectedAnswers[questionId].Add(answerId);
            }
        }
    }

    private void OnAnswerDeselected(int questionId, int answerId)
    {
        if (_selectedAnswers.ContainsKey(questionId))
        {
            _selectedAnswers[questionId].Remove(answerId);
        }
    }

    private void UpdateNavigationButtons()
    {
        BtnPrevious.IsEnabled = _currentIndex > 0;

        bool isLastQuestion = _currentIndex >= _questions.Count - 1;
        BtnNext.IsVisible = !isLastQuestion;
        BtnFinish.IsVisible = isLastQuestion;
    }

    private void BtnPrevious_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0)
        {
            ShowQuestion(_currentIndex - 1);
        }
    }

    private void BtnNext_Click(object? sender, RoutedEventArgs e)
    {
        if (_currentIndex < _questions.Count - 1)
        {
            ShowQuestion(_currentIndex + 1);
        }
    }

    private void BtnFinish_Click(object? sender, RoutedEventArgs e)
    {
        ShowResults();
    }

    private void BtnClose_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ShowResults()
    {
        PnlResults.IsVisible = true;
        BtnPrevious.IsVisible = false;
        BtnNext.IsVisible = false;
        BtnFinish.IsVisible = false;
        BtnClose.IsVisible = true;

        var loc = LocalizationManager.Instance;
        TxtResultsTitle.Text = loc.T("quiz.results_title");
        TxtScoreLabel.Text = loc.T("quiz.score");

        var (score, maxScore) = CalculateScore();
        TxtScore.Text = $"{FormatScore(score)}/{FormatScore(maxScore)}";

        double percent = maxScore > 0 ? (double)(score / maxScore * 100) : 0;
        TxtScorePercent.Text = $"{percent:F0}%";

        if (percent >= 80)
        {
            var brush = this.FindResource("SuccessBrush") as Avalonia.Media.IBrush;
            TxtScorePercent.Foreground = brush;
            IconResult.Symbol = Symbol.Accept;
            IconResult.Foreground = brush;
        }
        else if (percent >= 50)
        {
            var brush = this.FindResource("WarningBrush") as Avalonia.Media.IBrush;
            TxtScorePercent.Foreground = brush;
            IconResult.Symbol = Symbol.Important;
            IconResult.Foreground = brush;
        }
        else
        {
            var brush = this.FindResource("DangerBrush") as Avalonia.Media.IBrush;
            TxtScorePercent.Foreground = brush;
            IconResult.Symbol = Symbol.Cancel;
            IconResult.Foreground = brush;
        }
    }

    private static string FormatScore(decimal value) =>
        value == Math.Truncate(value) ? value.ToString("F0") : value.ToString("0.##");

    private (decimal score, decimal maxScore) CalculateScore()
    {
        decimal totalScore = 0;
        decimal maxPossibleScore = 0;

        foreach (var question in _questions)
        {
            var selectedIds = _selectedAnswers.GetValueOrDefault(question.Id, new List<int>());

            foreach (var answer in question.Answers)
            {
                if (answer.Weight > 0)
                {
                    maxPossibleScore += answer.Weight;
                }

                if (selectedIds.Contains(answer.Id))
                {
                    totalScore += answer.Weight;
                }
            }
        }

        return (Math.Max(0, totalScore), maxPossibleScore);
    }
}
