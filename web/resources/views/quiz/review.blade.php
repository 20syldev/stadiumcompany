@extends('layouts.app')

@section('content')
<div class="max-w-3xl mx-auto">
    <!-- Header -->
    <div class="rounded-xl p-6 mb-6" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <h1 class="text-xl font-bold mb-4" style="color: var(--color-text-primary);">
            {{ __('messages.review.title') }}
        </h1>
        <p class="text-sm mb-1" style="color: var(--color-text-primary);">
            <strong>{{ __('messages.review.theme') }}</strong>
            {{ $questionnaire->theme?->name }}
        </p>
        <p class="text-sm mb-3" style="color: var(--color-text-primary);">
            <strong>{{ __('messages.review.questionnaire_name') }}</strong>
            {{ $questionnaire->name }}
        </p>
        <p class="text-sm mb-1" style="color: var(--color-text-secondary);">
            {{ __('messages.review.score_total', ['score' => $submission->score, 'max' => $submission->max_score]) }}
        </p>
        @if($ranking['total'] > 0)
        <p class="text-sm font-semibold" style="color: var(--color-accent);">
            {{ __('messages.review.ranking') }} {{ $ranking['rank'] }}/{{ $ranking['total'] }}
        </p>
        @endif
    </div>

    <!-- Questions -->
    <div class="space-y-4">
        @foreach($questionsData as $question)
            @php
                $allCorrect = collect($question['answers'])->filter(fn($a) => $a['is_correct'] && $a['was_selected'])->count();
                $anyWrong   = collect($question['answers'])->filter(fn($a) => !$a['is_correct'] && $a['was_selected'])->count();
                $anyMissed  = collect($question['answers'])->filter(fn($a) => $a['is_correct'] && !$a['was_selected'])->count();
                $isCorrect  = $allCorrect > 0 && $anyWrong === 0 && $anyMissed === 0;
            @endphp
            <div class="rounded-xl p-5" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                <!-- Question header -->
                <div class="flex items-start gap-3 mb-4">
                    <span class="flex-shrink-0 w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold text-white"
                          style="background-color: {{ $isCorrect ? 'var(--color-success)' : 'var(--color-danger)' }};">
                        {{ $question['number'] }}
                    </span>
                    <p class="text-sm font-medium" style="color: var(--color-text-primary);">
                        {{ $question['label'] }}
                    </p>
                </div>

                <!-- Answers -->
                <div class="space-y-2 ml-10">
                    @foreach($question['answers'] as $answer)
                        @php
                            $isCorrectAnswer  = $answer['is_correct'];
                            $wasSelected      = $answer['was_selected'];

                            if ($isCorrectAnswer && $wasSelected) {
                                // Green filled — correct and selected
                                $bgStyle     = 'background-color: var(--color-answer-correct-bg); border-color: var(--color-answer-correct-border);';
                                $textStyle   = 'color: var(--color-success);';
                                $iconPath    = 'M5 13l4 4L19 7';
                                $labelKey    = 'messages.review.answer_correct_selected';
                                $showLabel   = true;
                            } elseif ($isCorrectAnswer && !$wasSelected) {
                                // Green outline — correct but missed
                                $bgStyle     = 'background-color: var(--color-answer-correct-bg); border-color: var(--color-answer-correct-border);';
                                $textStyle   = 'color: var(--color-success);';
                                $iconPath    = 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z';
                                $labelKey    = 'messages.review.answer_correct_missed';
                                $showLabel   = true;
                            } elseif (!$isCorrectAnswer && $wasSelected) {
                                // Red filled — wrong and selected
                                $bgStyle     = 'background-color: var(--color-answer-wrong-bg); border-color: var(--color-answer-wrong-border);';
                                $textStyle   = 'color: var(--color-danger);';
                                $iconPath    = 'M6 18L18 6M6 6l12 12';
                                $labelKey    = 'messages.review.answer_wrong_selected';
                                $showLabel   = true;
                            } else {
                                // Neutral — wrong and not selected
                                $bgStyle     = 'border-color: var(--color-border-subtle);';
                                $textStyle   = 'color: var(--color-text-tertiary);';
                                $iconPath    = null;
                                $labelKey    = null;
                                $showLabel   = false;
                            }
                        @endphp
                        <div class="flex items-center gap-3 px-4 py-2.5 rounded-lg border"
                             style="{{ $bgStyle }}">
                            @if($iconPath)
                                <svg class="w-4 h-4 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"
                                     style="{{ $textStyle }}">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="{{ $iconPath }}"/>
                                </svg>
                            @else
                                <span class="w-4 h-4 flex-shrink-0"></span>
                            @endif

                            <span class="text-sm flex-1" style="{{ $showLabel ? $textStyle : 'color: var(--color-text-secondary);' }}">
                                {{ $answer['label'] }}
                            </span>

                            @if($showLabel)
                                <span class="text-xs font-medium flex-shrink-0" style="{{ $textStyle }}">
                                    {{ __($labelKey) }}
                                </span>
                            @endif
                        </div>
                    @endforeach
                </div>
            </div>
        @endforeach
    </div>

    <!-- Back button -->
    <div class="mt-6">
        <a href="{{ route('dashboard', ['tab' => 'results']) }}"
           class="inline-flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition hover:opacity-80"
           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
            </svg>
            {{ __('messages.review.back') }}
        </a>
    </div>
</div>
@endsection
