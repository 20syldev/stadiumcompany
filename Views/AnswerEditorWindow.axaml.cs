using Avalonia.Controls;
using Avalonia.Interactivity;
using StadiumCompany.DAL;
using StadiumCompany.Models;
using StadiumCompany.Services;

namespace StadiumCompany.Views;

public partial class AnswerEditorWindow : Window
{
    private readonly int _questionId;
    private readonly int? _answerId;
    private readonly AnswerRepository _answerRepository = new();

    private Answer? _answer;

    public AnswerEditorWindow()
    {
        InitializeComponent();
    }

    public AnswerEditorWindow(int questionId, int? answerId = null)
    {
        InitializeComponent();
        _questionId = questionId;
        _answerId = answerId;

        var loc = LocalizationManager.Instance;
        loc.LanguageChanged += UpdateTexts;

        if (_answerId.HasValue)
        {
            Title = loc.T("answer.edit");
            TitleText.Text = Title;
            LoadAnswer();
        }
        else
        {
            Title = loc.T("answer.new");
            TitleText.Text = Title;
        }

        UpdateTexts();
    }

    private void UpdateTexts()
    {
        var loc = LocalizationManager.Instance;

        LblAnswerText.Text = loc.T("answer.label");
        TxtLabel.Watermark = loc.T("answer.label_placeholder");
        ChkCorrect.Content = loc.T("answer.is_correct");
        LblWeight.Text = loc.T("answer.weight");
        BtnCancel.Content = loc.T("common.cancel");
        BtnSave.Content = loc.T("answer.apply");

        if (_answerId.HasValue)
        {
            Title = loc.T("answer.edit");
            TitleText.Text = Title;
        }
        else
        {
            Title = loc.T("answer.new");
            TitleText.Text = Title;
        }
    }

    private void LoadAnswer()
    {
        var answers = _answerRepository.GetByQuestion(_questionId);
        _answer = answers.FirstOrDefault(a => a.Id == _answerId);

        if (_answer == null)
        {
            Close(false);
            return;
        }

        TxtLabel.Text = _answer.Label;
        ChkCorrect.IsChecked = _answer.IsCorrect;
        NumWeight.Value = _answer.Weight;
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

        var answer = new Answer
        {
            Id = _answerId ?? 0,
            QuestionId = _questionId,
            Label = TxtLabel.Text.Trim(),
            IsCorrect = ChkCorrect.IsChecked ?? false,
            Weight = NumWeight.Value ?? 0
        };

        if (_answerId.HasValue)
        {
            _answerRepository.Update(answer);
        }
        else
        {
            _answerRepository.Create(answer);
        }

        Close(true);
    }
}
