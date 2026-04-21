@extends('layouts.app')

@section('content')
<div id="editor-app"></div>
@endsection

@push('scripts')
@php
    $editorQuestions = $questionnaire ? $questionnaire->questions->map(function($q) {
        $tfCorrect = 'true';
        $tfWeight = 1;
        if ($q->answer_type->value === 'TRUE_FALSE' && $q->answers->count() >= 2) {
            $trueAnswer = $q->answers->first(function($a) { return $a->is_correct; });
            if ($trueAnswer) {
                $tfCorrect = $trueAnswer->label === __('messages.question.true') || $trueAnswer->label === 'Vrai' || $trueAnswer->label === 'True' ? 'true' : 'false';
                $tfWeight = $trueAnswer->weight;
            }
        }
        return [
            'label' => $q->label,
            'answer_type' => $q->answer_type->value,
            'tf_correct' => $tfCorrect,
            'tf_weight' => $tfWeight,
            'distributeValue' => 1,
            'answers' => $q->answers->map(function($a) {
                return [
                    'label' => $a->label,
                    'is_correct' => $a->is_correct,
                    'weight' => (float) $a->weight,
                ];
            })->toArray(),
        ];
    })->toArray() : [];

    $editorThemes = $themes->map(fn($t) => [
        'id' => $t->id,
        'label' => __('messages.theme_label.names.' . $t->name, [], app()->getLocale()) !== 'messages.theme_label.names.' . $t->name
            ? __('messages.theme_label.names.' . $t->name)
            : $t->name,
    ])->toArray();
@endphp
<script>
window.__EDITOR_DATA__ = {
    questions: @json($editorQuestions),
    themeId: '{{ old('theme_id', $questionnaire->theme_id ?? '') }}',
    publishValue: '{{ old('published', $questionnaire->published ?? false) ? '1' : '0' }}',
    readonly: @json($readonly ?? false),
    isUpdate: @json((bool)$questionnaire),
    isPublished: @json($questionnaire->published ?? false),
    themes: @json($editorThemes),
    themeStoreUrl: '{{ route('themes.store') }}',
    formAction: '{{ $questionnaire ? route('questionnaires.update', $questionnaire) : route('questionnaires.store') }}',
    csrfToken: '{{ csrf_token() }}',
    dashboardUrl: '{{ route('dashboard') }}',
    oldName: '{{ old('name', $questionnaire->name ?? '') }}',
    errors: @json($errors->toArray()),
};
</script>
@endpush
