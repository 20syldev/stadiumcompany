using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class QuizPlayerWindow : Window
{
    private readonly Questionnaire _questionnaire;
    private readonly List<Question> _questions;
    private int _currentIndex = 0;
    private readonly Dictionary<int, List<int>> _selectedAnswers = [];
    private readonly bool _isDemoMode;

    private static IBrush Res(string key)
    {
        var app = Application.Current!;
        if (app.TryFindResource(key, app.ActualThemeVariant, out var value) && value is IBrush brush)
            return brush;
        return Brushes.Transparent;
    }

    public QuizPlayerWindow()
    {
        InitializeComponent();
        _questionnaire = null!;
        _questions = [];
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

        // Update progress bar
        UpdateProgressBar();

        PnlAnswers.Children.Clear();

        var selectedIds = _selectedAnswers.GetValueOrDefault(question.Id, []);

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
                    Margin = new Thickness(0),
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                radioButton.IsCheckedChanged += (s, e) =>
                {
                    if (radioButton.IsChecked == true)
                        OnAnswerSelected(question.Id, answer.Id, true);
                    UpdateAnswerCardStyles();
                };
                PnlAnswers.Children.Add(WrapInAnswerCard(radioButton, isSelected));
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
                    Margin = new Thickness(0),
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                checkBox.IsCheckedChanged += (s, e) =>
                {
                    if (checkBox.IsChecked == true)
                        OnAnswerSelected(question.Id, answer.Id, false);
                    else
                        OnAnswerDeselected(question.Id, answer.Id);
                    UpdateAnswerCardStyles();
                };
                PnlAnswers.Children.Add(WrapInAnswerCard(checkBox, isSelected));
            }
        }

        UpdateNavigationButtons();
    }

    private Border WrapInAnswerCard(Control control, bool isSelected)
    {
        var card = new Border
        {
            Background = Res("CardBackgroundBrush"),
            BorderBrush = isSelected
                ? Res("AccentBrush")
                : Res("BorderSubtleBrush"),
            BorderThickness = new Thickness(isSelected ? 2 : 1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14, 10),
            Cursor = new Cursor(StandardCursorType.Hand),
            Child = control
        };
        card.Classes.Add("card-hoverable");
        return card;
    }

    private void UpdateAnswerCardStyles()
    {
        if (_currentIndex < 0 || _currentIndex >= _questions.Count) return;
        var question = _questions[_currentIndex];
        var selectedIds = _selectedAnswers.GetValueOrDefault(question.Id, []);

        foreach (var child in PnlAnswers.Children)
        {
            if (child is Border card && card.Child is Control ctrl && ctrl.Tag is int answerId)
            {
                bool isSelected = selectedIds.Contains(answerId);
                card.BorderBrush = isSelected
                    ? Res("AccentBrush")
                    : Res("BorderSubtleBrush");
                card.BorderThickness = new Thickness(isSelected ? 2 : 1);
            }
        }
    }

    private void UpdateProgressBar()
    {
        if (_questions.Count == 0) return;
        double fraction = (_currentIndex + 1.0) / _questions.Count;

        // Use LayoutUpdated to get actual width
        ProgressBarTrack.LayoutUpdated += OnProgressLayoutUpdated;
        void OnProgressLayoutUpdated(object? s, EventArgs e)
        {
            ProgressBarTrack.LayoutUpdated -= OnProgressLayoutUpdated;
            double trackWidth = ProgressBarTrack.Bounds.Width;
            if (trackWidth > 0)
            {
                ProgressBarFill.Width = trackWidth * fraction;
            }
        }
        // Also set immediately if width is already known
        if (ProgressBarTrack.Bounds.Width > 0)
        {
            ProgressBarFill.Width = ProgressBarTrack.Bounds.Width * fraction;
        }
    }

    private void OnAnswerSelected(int questionId, int answerId, bool isSingleChoice)
    {
        if (isSingleChoice)
        {
            _selectedAnswers[questionId] = [answerId];
        }
        else
        {
            if (!_selectedAnswers.ContainsKey(questionId))
            {
                _selectedAnswers[questionId] = [];
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
            var brush = Res("SuccessBrush");
            TxtScorePercent.Foreground = brush;
            IconResult.Symbol = Symbol.Accept;
            IconResult.Foreground = brush;
        }
        else if (percent >= 50)
        {
            var brush = Res("WarningBrush");
            TxtScorePercent.Foreground = brush;
            IconResult.Symbol = Symbol.Important;
            IconResult.Foreground = brush;
        }
        else
        {
            var brush = Res("DangerBrush");
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
            var selectedIds = _selectedAnswers.GetValueOrDefault(question.Id, []);

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
