@extends('layouts.app')

@section('content')
<div x-data="questionnaireEditor()" class="max-w-4xl mx-auto">
    <h1 class="text-xl font-bold mb-6" style="color: var(--color-text-primary);">
        @if($readonly)
            {{ __('messages.editor.view_questionnaire') }}
        @elseif($questionnaire)
            {{ __('messages.editor.edit_questionnaire') }}
        @else
            {{ __('messages.editor.new_questionnaire') }}
        @endif
    </h1>

    <form method="POST"
          action="{{ $questionnaire ? route('questionnaires.update', $questionnaire) : route('questionnaires.store') }}"
          id="editor-form">
        @csrf
        @if($questionnaire)
            @method('PUT')
        @endif

        <!-- Metadata Card -->
        <div class="rounded-xl p-6 mb-6" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                    <label class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.editor.name') }}</label>
                    <input type="text" name="name" value="{{ old('name', $questionnaire->name ?? '') }}"
                           class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                           style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                           placeholder="{{ __('messages.editor.name_placeholder') }}"
                           {{ $readonly ? 'disabled' : '' }} required>
                    @error('name')
                        <p class="mt-1 text-sm" style="color: var(--color-danger);">{{ $message }}</p>
                    @enderror
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.editor.theme') }}</label>
                    <div class="flex gap-2">
                        <select name="theme_id" x-model="themeId"
                                class="flex-1 rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                                style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                                {{ $readonly ? 'disabled' : '' }} required>
                            @foreach($themes as $theme)
                                <option value="{{ $theme->id }}" {{ (old('theme_id', $questionnaire->theme_id ?? '') == $theme->id) ? 'selected' : '' }}>
                                    {{ __('messages.theme_label.names.' . $theme->name, [], app()->getLocale()) !== 'messages.theme_label.names.' . $theme->name ? __('messages.theme_label.names.' . $theme->name) : $theme->name }}
                                </option>
                            @endforeach
                            @if(!$readonly)
                                <option value="__new__">{{ __('messages.theme_label.other') }}</option>
                            @endif
                        </select>
                    </div>
                    @error('theme_id')
                        <p class="mt-1 text-sm" style="color: var(--color-danger);">{{ $message }}</p>
                    @enderror
                </div>
            </div>
            @if(!$readonly)
                <div class="mt-4">
                    <label class="inline-flex items-center gap-2 cursor-pointer">
                        <input type="checkbox" name="published" value="1"
                               {{ old('published', $questionnaire->published ?? false) ? 'checked' : '' }}
                               class="rounded" style="color: var(--color-accent);">
                        <span class="text-sm" style="color: var(--color-text-secondary);">{{ __('messages.editor.published') }}</span>
                    </label>
                </div>
            @else
                @if($questionnaire->published)
                    <span class="inline-block mt-4 px-2 py-0.5 rounded text-xs font-medium" style="background-color: var(--color-success); color: white;">
                        {{ __('messages.main.card_published') }}
                    </span>
                @endif
            @endif
        </div>

        <!-- New Theme Modal -->
        <div x-show="showNewTheme" x-cloak class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
            <div class="rounded-xl p-6 w-96" style="background-color: var(--color-bg-card);" @click.away="showNewTheme = false">
                <h3 class="text-lg font-bold mb-4" style="color: var(--color-text-primary);">{{ __('messages.theme_label.new_title') }}</h3>
                <input type="text" x-model="newThemeName" class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2 mb-4"
                       style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                       placeholder="{{ __('messages.theme_label.new_prompt') }}">
                <div class="flex justify-end gap-2">
                    <button type="button" @click="showNewTheme = false" class="px-4 py-2 rounded-lg text-sm" style="color: var(--color-text-secondary);">
                        {{ __('messages.common.cancel') }}
                    </button>
                    <button type="button" @click="createTheme()" class="px-4 py-2 rounded-lg text-sm font-semibold text-white" style="background-color: var(--color-accent);">
                        {{ __('messages.theme_label.create') }}
                    </button>
                </div>
            </div>
        </div>

        <!-- Questions -->
        <h2 class="text-lg font-semibold mb-4" style="color: var(--color-text-primary);">{{ __('messages.editor.questions') }}</h2>

        <template x-for="(question, qIdx) in questions" :key="qIdx">
            <div class="rounded-xl p-5 mb-4" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                <div class="flex items-start gap-3 mb-4">
                    <!-- Number badge -->
                    <span class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold text-white" style="background-color: var(--color-accent);"
                          x-text="qIdx + 1"></span>
                    <div class="flex-1">
                        <input type="text" :name="'questions['+qIdx+'][label]'" x-model="question.label"
                               class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                               style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                               placeholder="{{ __('messages.question.label_placeholder') }}"
                               {{ $readonly ? 'disabled' : '' }} required>
                    </div>
                    <div class="flex items-center gap-2">
                        <select :name="'questions['+qIdx+'][answer_type]'" x-model="question.answer_type"
                                @change="onTypeChange(qIdx)"
                                class="rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                                style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                                {{ $readonly ? 'disabled' : '' }}>
                            <option value="TRUE_FALSE">{{ __('messages.question.type_truefalse') }}</option>
                            <option value="MULTIPLE_CHOICE">{{ __('messages.question.type_multiple') }}</option>
                        </select>
                        @if(!$readonly)
                            <button type="button" @click="removeQuestion(qIdx)" class="p-1.5 rounded hover:opacity-70" style="color: var(--color-danger);">
                                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                                </svg>
                            </button>
                        @endif
                    </div>
                </div>

                <!-- TRUE_FALSE answers -->
                <template x-if="question.answer_type === 'TRUE_FALSE'">
                    <div class="ml-11">
                        <p class="text-xs font-medium mb-2" style="color: var(--color-text-tertiary);">{{ __('messages.question.correct_answer') }}</p>
                        <div class="flex gap-4 mb-3">
                            <label class="inline-flex items-center gap-2 cursor-pointer">
                                <input type="radio" :name="'questions['+qIdx+'][tf_correct]'" value="true"
                                       x-model="question.tf_correct" {{ $readonly ? 'disabled' : '' }}
                                       style="color: var(--color-accent);">
                                <span class="text-sm" style="color: var(--color-text-primary);">{{ __('messages.question.true') }}</span>
                            </label>
                            <label class="inline-flex items-center gap-2 cursor-pointer">
                                <input type="radio" :name="'questions['+qIdx+'][tf_correct]'" value="false"
                                       x-model="question.tf_correct" {{ $readonly ? 'disabled' : '' }}
                                       style="color: var(--color-accent);">
                                <span class="text-sm" style="color: var(--color-text-primary);">{{ __('messages.question.false') }}</span>
                            </label>
                        </div>
                        <div class="flex items-center gap-2">
                            <label class="text-xs" style="color: var(--color-text-tertiary);">{{ __('messages.question.col_weight') }}:</label>
                            <input type="number" :name="'questions['+qIdx+'][tf_weight]'" x-model="question.tf_weight"
                                   class="w-20 rounded-lg px-2 py-1 text-sm border"
                                   style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                                   step="0.5" min="0" {{ $readonly ? 'disabled' : '' }}>
                        </div>
                        <!-- Hidden answers for TRUE_FALSE -->
                        <input type="hidden" :name="'questions['+qIdx+'][answers][0][label]'" :value="'{{ __('messages.question.true') }}'">
                        <input type="hidden" :name="'questions['+qIdx+'][answers][0][is_correct]'" :value="question.tf_correct === 'true' ? '1' : '0'">
                        <input type="hidden" :name="'questions['+qIdx+'][answers][0][weight]'" :value="question.tf_correct === 'true' ? question.tf_weight : 0">
                        <input type="hidden" :name="'questions['+qIdx+'][answers][1][label]'" :value="'{{ __('messages.question.false') }}'">
                        <input type="hidden" :name="'questions['+qIdx+'][answers][1][is_correct]'" :value="question.tf_correct === 'false' ? '1' : '0'">
                        <input type="hidden" :name="'questions['+qIdx+'][answers][1][weight]'" :value="question.tf_correct === 'false' ? question.tf_weight : 0">
                    </div>
                </template>

                <!-- MULTIPLE_CHOICE answers -->
                <template x-if="question.answer_type === 'MULTIPLE_CHOICE'">
                    <div class="ml-11">
                        <div class="flex items-center justify-between mb-2">
                            <p class="text-xs font-medium" style="color: var(--color-text-tertiary);">{{ __('messages.question.answers') }}</p>
                            @if(!$readonly)
                                <div class="flex items-center gap-2">
                                    <input type="number" x-model="question.distributeValue"
                                           class="w-16 rounded px-2 py-1 text-xs border text-center"
                                           style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                                           step="0.5" min="0">
                                    <button type="button" @click="distributePoints(qIdx)"
                                            class="text-xs px-2 py-1 rounded hover:opacity-80"
                                            style="color: var(--color-accent); border: 1px solid var(--color-accent);">
                                        {{ __('messages.editor.distribute_points') }}
                                    </button>
                                </div>
                            @endif
                        </div>

                        <!-- Answer header -->
                        <div class="grid grid-cols-12 gap-2 mb-1 text-xs font-medium" style="color: var(--color-text-tertiary);">
                            <div class="col-span-6">{{ __('messages.question.col_answer') }}</div>
                            <div class="col-span-2 text-center">{{ __('messages.question.col_correct') }}</div>
                            <div class="col-span-2 text-center">{{ __('messages.question.col_weight') }}</div>
                            <div class="col-span-2"></div>
                        </div>

                        <!-- Answer rows -->
                        <template x-for="(answer, aIdx) in question.answers" :key="aIdx">
                            <div class="grid grid-cols-12 gap-2 mb-2 items-center">
                                <div class="col-span-6">
                                    <input type="text" :name="'questions['+qIdx+'][answers]['+aIdx+'][label]'" x-model="answer.label"
                                           class="w-full rounded-lg px-2 py-1.5 text-sm border"
                                           style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                                           placeholder="{{ __('messages.editor.answer_placeholder') }}"
                                           {{ $readonly ? 'disabled' : '' }} required>
                                </div>
                                <div class="col-span-2 text-center">
                                    <input type="checkbox" :name="'questions['+qIdx+'][answers]['+aIdx+'][is_correct]'" value="1"
                                           x-model="answer.is_correct" @change="answer.weight = answer.is_correct ? 1 : 0" {{ $readonly ? 'disabled' : '' }}
                                           class="rounded" style="color: var(--color-accent);">
                                </div>
                                <div class="col-span-2">
                                    <input type="number" :name="'questions['+qIdx+'][answers]['+aIdx+'][weight]'" x-model="answer.weight"
                                           class="w-full rounded-lg px-2 py-1.5 text-sm border text-center"
                                           style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                                           step="0.5" {{ $readonly ? 'disabled' : '' }}>
                                </div>
                                <div class="col-span-2 text-center">
                                    @if(!$readonly)
                                        <button type="button" @click="removeAnswer(qIdx, aIdx)"
                                                x-show="question.answers.length > 2"
                                                class="p-1 rounded hover:opacity-70" style="color: var(--color-danger);">
                                            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                                            </svg>
                                        </button>
                                    @endif
                                </div>
                            </div>
                        </template>

                        @if(!$readonly)
                            <button type="button" @click="addAnswer(qIdx)"
                                    class="text-sm mt-1 hover:underline" style="color: var(--color-accent);">
                                + {{ __('messages.editor.add_answer') }}
                            </button>
                        @endif
                    </div>
                </template>
            </div>
        </template>

        <!-- Add Question & Actions -->
        <div class="flex items-center justify-between mt-6">
            @if(!$readonly)
                <button type="button" @click="addQuestion()"
                        class="inline-flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition hover:opacity-90"
                        style="color: var(--color-accent); border: 1px solid var(--color-accent);">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
                    </svg>
                    {{ __('messages.editor.add_question') }}
                </button>
            @else
                <div></div>
            @endif

            <div class="flex gap-3">
                <a href="{{ route('dashboard') }}"
                   class="px-6 py-2 rounded-lg text-sm font-medium"
                   style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                    {{ $readonly ? __('messages.editor.close') : __('messages.editor.cancel') }}
                </a>
                @if(!$readonly)
                    <button type="submit"
                            class="px-6 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                            style="background-color: var(--color-accent);">
                        {{ __('messages.editor.save') }}
                    </button>
                @endif
            </div>
        </div>
    </form>
</div>

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
@endphp

@push('scripts')
<script>
function questionnaireEditor() {
    return {
        themeId: '{{ old('theme_id', $questionnaire->theme_id ?? '') }}',
        showNewTheme: false,
        newThemeName: '',
        questions: @json($editorQuestions),

        init() {
            this.$watch('themeId', (val) => {
                if (val === '__new__') {
                    this.showNewTheme = true;
                }
            });
        },

        addQuestion() {
            this.questions.push({
                label: '',
                answer_type: 'TRUE_FALSE',
                tf_correct: 'true',
                tf_weight: 1,
                answers: [],
                distributeValue: 1,
            });
        },

        removeQuestion(idx) {
            this.questions.splice(idx, 1);
        },

        onTypeChange(qIdx) {
            let q = this.questions[qIdx];
            if (q.answer_type === 'MULTIPLE_CHOICE' && q.answers.length === 0) {
                q.answers = [
                    { label: '', is_correct: false, weight: 0 },
                    { label: '', is_correct: false, weight: 0 },
                ];
            }
        },

        addAnswer(qIdx) {
            this.questions[qIdx].answers.push({ label: '', is_correct: false, weight: 0 });
        },

        removeAnswer(qIdx, aIdx) {
            this.questions[qIdx].answers.splice(aIdx, 1);
        },

        distributePoints(qIdx) {
            let q = this.questions[qIdx];
            let correctCount = q.answers.filter(a => a.is_correct).length;
            if (correctCount === 0) return;
            let total = parseFloat(q.distributeValue) || 1;
            let each = Math.round((total / correctCount) * 100) / 100;
            q.answers.forEach(a => {
                a.weight = a.is_correct ? each : 0;
            });
        },

        async createTheme() {
            if (!this.newThemeName.trim()) return;
            try {
                const response = await fetch('{{ route('themes.store') }}', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content,
                        'Accept': 'application/json',
                    },
                    body: JSON.stringify({ name: this.newThemeName }),
                });
                const theme = await response.json();
                if (theme.id) {
                    // Add option to select
                    const select = document.querySelector('select[name="theme_id"]');
                    const option = new Option(this.newThemeName, theme.id, true, true);
                    select.insertBefore(option, select.lastElementChild);
                    this.themeId = String(theme.id);
                    this.showNewTheme = false;
                    this.newThemeName = '';
                }
            } catch (e) {
                alert('{{ __('messages.common.error') }}');
            }
        },
    };
}
</script>
@endpush
@endsection
