@extends('layouts.app')

@section('content')
<div id="quiz-player-app" class="max-w-3xl mx-auto"></div>
@endsection

@push('scripts')
@php
    $questionsJson = $questionnaire->questions->map(function($q) {
        return [
            'id' => $q->id,
            'number' => $q->number,
            'label' => $q->label,
            'answer_type' => $q->answer_type->value,
            'answers' => $q->answers->map(function($a) {
                return [
                    'id' => $a->id,
                    'label' => $a->label,
                    'is_correct' => $a->is_correct,
                    'weight' => (float) $a->weight,
                ];
            })->toArray(),
        ];
    })->toArray();
@endphp
<script>
window.__QUIZ_DATA__ = {
    questions: @json($questionsJson),
    isDemoMode: @json($isDemoMode),
    questionnaireName: @json($questionnaire->name),
    themeName: @json($questionnaire->theme?->name),
    scoreUrl: '{{ route('quiz.score', $questionnaire) }}',
    reviewBaseUrl: '{{ url('quiz/review') }}',
    dashboardUrl: '{{ route('dashboard') }}',
    feedbackBaseUrl: '{{ url('questions') }}',
    alreadySentFeedback: @json($alreadySentFeedback),
    csrfToken: '{{ csrf_token() }}',
};
</script>
@endpush
