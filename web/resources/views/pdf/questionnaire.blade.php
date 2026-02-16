<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: DejaVu Sans, sans-serif; font-size: 12px; color: #1e293b; margin: 0; padding: 20px; }
        .header { text-align: center; margin-bottom: 30px; padding-bottom: 15px; border-bottom: 2px solid #667eea; }
        .header h1 { font-size: 18px; color: #667eea; margin: 0 0 10px 0; letter-spacing: 2px; }
        .header h2 { font-size: 16px; margin: 0 0 8px 0; }
        .meta { font-size: 11px; color: #64748b; }
        .question { margin-bottom: 20px; padding: 15px; border: 1px solid #e2e8f0; border-radius: 8px; page-break-inside: avoid; }
        .question-header { display: flex; margin-bottom: 10px; }
        .question-number { background-color: #667eea; color: white; width: 28px; height: 28px; border-radius: 50%; text-align: center; line-height: 28px; font-weight: bold; font-size: 13px; float: left; margin-right: 10px; }
        .question-label { font-weight: bold; font-size: 13px; margin-top: 4px; }
        .question-type { font-size: 10px; color: #64748b; margin-bottom: 8px; margin-left: 38px; }
        .answers-table { width: 100%; border-collapse: collapse; margin-left: 38px; margin-top: 8px; }
        .answers-table th { text-align: left; font-size: 10px; color: #64748b; padding: 4px 8px; border-bottom: 1px solid #e2e8f0; }
        .answers-table td { padding: 6px 8px; font-size: 11px; border-bottom: 1px solid #f1f5f9; }
        .correct { background-color: #f0fdf4; }
        .footer { text-align: center; font-size: 10px; color: #94a3b8; margin-top: 30px; padding-top: 10px; border-top: 1px solid #e2e8f0; }
    </style>
</head>
<body>
    <div class="header">
        <h1>{{ __('messages.pdf.header') }}</h1>
        <h2>{{ $questionnaire->name }}</h2>
        <div class="meta">
            {{ __('messages.pdf.theme', ['name' => $questionnaire->theme->name ?? '']) }} |
            {{ __('messages.pdf.question_count', ['count' => $questionnaire->questions->count()]) }} |
            {{ __('messages.pdf.date', ['date' => now()->format('d/m/Y')]) }}
        </div>
    </div>

    @foreach($questionnaire->questions as $question)
        <div class="question">
            <div>
                <div class="question-number">{{ $question->number }}</div>
                <div class="question-label">{{ $question->label }}</div>
            </div>
            <div class="question-type" style="clear: both;">
                @if($question->answer_type->value === 'TRUE_FALSE')
                    {{ __('messages.pdf.type_truefalse') }}
                @elseif($question->answer_type->value === 'MULTIPLE_CHOICE')
                    {{ __('messages.pdf.type_multiple') }}
                @else
                    {{ __('messages.pdf.type_unknown') }}
                @endif
            </div>
            <table class="answers-table" style="width: calc(100% - 38px);">
                <thead>
                    <tr>
                        <th>{{ __('messages.question.col_answer') }}</th>
                        <th style="width: 60px;">{{ __('messages.question.col_correct') }}</th>
                        <th style="width: 60px;">{{ __('messages.question.col_weight') }}</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach($question->answers as $answer)
                        <tr class="{{ $answer->is_correct ? 'correct' : '' }}">
                            <td>{{ $answer->label }}</td>
                            <td style="text-align: center;">{{ $answer->is_correct ? __('messages.common.yes') : __('messages.common.no') }}</td>
                            <td style="text-align: center;">{{ $answer->weight }}</td>
                        </tr>
                    @endforeach
                </tbody>
            </table>
        </div>
    @endforeach

    <div class="footer">
        {{ __('messages.pdf.document_label') }} - {{ now()->format('d/m/Y') }}
    </div>
</body>
</html>
