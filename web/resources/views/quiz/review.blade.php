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
        <p class="text-sm" style="color: var(--color-text-secondary);">
            {{ __('messages.review.score_total', ['score' => $submission->score, 'max' => $submission->max_score]) }}
        </p>
    </div>

    <!-- Questions -->
    <div class="space-y-4">
        @foreach($questionsData as $question)
            <div class="rounded-xl p-5" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                <p class="text-sm mb-2" style="color: var(--color-text-primary);">
                    <strong>Q{{ $question['number'] }}.</strong> {{ $question['label'] }}
                </p>
                <p class="text-sm mb-1" style="color: var(--color-text-secondary);">
                    {{ __('messages.review.answers') }} {{ implode(' ; ', $question['all_labels']) }}
                </p>
                <p class="text-sm font-medium" style="color: var(--color-success);">
                    {{ __('messages.review.correct_answer') }} {{ implode(' ; ', $question['correct_labels']) }}
                </p>
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
