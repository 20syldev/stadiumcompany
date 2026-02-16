@extends('layouts.app')

@section('content')
<div x-data="quizPlayer()" class="max-w-3xl mx-auto">
    <!-- Demo Banner -->
    @if($isDemoMode)
        <div class="rounded-lg px-4 py-3 mb-4 flex items-center gap-2 text-sm font-medium" style="background-color: var(--color-warning); color: #000;">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"/>
            </svg>
            {{ __('messages.quiz.demo_banner') }}
        </div>
    @endif

    <!-- Header -->
    <div class="rounded-xl p-6 mb-4" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <div class="flex items-center justify-between mb-2">
            <h1 class="text-lg font-bold" style="color: var(--color-text-primary);">
                {{ $isDemoMode ? __('messages.quiz.demo_title') : __('messages.quiz.title') }} - {{ $questionnaire->name }}
            </h1>
            <span class="text-sm" style="color: var(--color-text-tertiary);" x-text="progressText"></span>
        </div>
        @if($questionnaire->theme)
            <p class="text-sm mb-3" style="color: var(--color-text-secondary);">
                {{ __('messages.quiz.theme', ['name' => $questionnaire->theme->name]) }}
            </p>
        @endif
        <!-- Progress bar -->
        <div class="w-full h-2 rounded-full overflow-hidden" style="background-color: var(--color-border-subtle);">
            <div class="h-full rounded-full transition-all duration-300"
                 :style="'background-color: var(--color-accent); width: ' + progressPercent + '%'"></div>
        </div>
    </div>

    <!-- Question / Results -->
    <template x-if="!showResults">
        <div class="rounded-xl p-6" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
            <div class="flex items-start gap-3 mb-4">
                <span class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold text-white" style="background-color: var(--color-accent);"
                      x-text="currentQuestion.number"></span>
                <div>
                    <span class="text-xs font-medium px-2 py-0.5 rounded" style="background-color: var(--color-border-subtle); color: var(--color-text-tertiary);"
                          x-text="currentQuestion.answer_type === 'TRUE_FALSE' ? '{{ __('messages.question.type_truefalse') }}' : '{{ __('messages.question.type_multiple') }}'"></span>
                </div>
            </div>
            <h2 class="text-base font-semibold mb-6" style="color: var(--color-text-primary);" x-text="currentQuestion.label"></h2>

            <!-- TRUE_FALSE -->
            <template x-if="currentQuestion.answer_type === 'TRUE_FALSE'">
                <div class="space-y-3">
                    <template x-for="answer in currentQuestion.answers" :key="answer.id">
                        <label class="flex items-center gap-3 p-3 rounded-lg cursor-pointer transition"
                               :style="isSelected(answer.id) ? 'border: 2px solid var(--color-accent); background-color: var(--color-bg-card-hover);' : 'border: 2px solid var(--color-border-subtle);'">
                            <input type="radio" :name="'q_' + currentQuestion.id" :value="answer.id"
                                   @change="selectAnswer(currentQuestion.id, answer.id, 'radio')"
                                   :checked="isSelected(answer.id)"
                                   style="color: var(--color-accent);">
                            <span class="text-sm" style="color: var(--color-text-primary);" x-text="answer.label"></span>
                        </label>
                    </template>
                </div>
            </template>

            <!-- MULTIPLE_CHOICE -->
            <template x-if="currentQuestion.answer_type === 'MULTIPLE_CHOICE'">
                <div class="space-y-3">
                    <template x-for="answer in currentQuestion.answers" :key="answer.id">
                        <label class="flex items-center gap-3 p-3 rounded-lg cursor-pointer transition"
                               :style="isSelected(answer.id) ? 'border: 2px solid var(--color-accent); background-color: var(--color-bg-card-hover);' : 'border: 2px solid var(--color-border-subtle);'">
                            <input type="checkbox" :value="answer.id"
                                   @change="selectAnswer(currentQuestion.id, answer.id, 'checkbox')"
                                   :checked="isSelected(answer.id)"
                                   class="rounded" style="color: var(--color-accent);">
                            <span class="text-sm" style="color: var(--color-text-primary);" x-text="answer.label"></span>
                        </label>
                    </template>
                </div>
            </template>
        </div>
    </template>

    <!-- Results -->
    <template x-if="showResults">
        <div class="rounded-xl p-8 text-center" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
            <div class="mb-4">
                <svg x-show="scorePercent >= 80" class="w-16 h-16 mx-auto" style="color: var(--color-success);" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
                </svg>
                <svg x-show="scorePercent >= 50 && scorePercent < 80" class="w-16 h-16 mx-auto" style="color: var(--color-warning);" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
                </svg>
                <svg x-show="scorePercent < 50" class="w-16 h-16 mx-auto" style="color: var(--color-danger);" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z"/>
                </svg>
            </div>
            <h2 class="text-xl font-bold mb-2" style="color: var(--color-text-primary);">{{ __('messages.quiz.results_title') }}</h2>
            <p class="text-sm mb-3" style="color: var(--color-success);">{{ __('messages.quiz.answers_saved') }}</p>
            <p class="text-lg mb-1" style="color: var(--color-text-secondary);">
                {{ __('messages.quiz.score') }} <span x-text="scoreDisplay"></span>
            </p>
            <p class="text-3xl font-bold" :style="'color: ' + scoreColor" x-text="scorePercent + '%'"></p>
        </div>
    </template>

    <!-- Navigation -->
    <div class="flex items-center justify-between mt-6">
        <button x-show="!showResults && currentIndex > 0" @click="prev()"
                class="px-4 py-2 rounded-lg text-sm font-medium"
                style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            {{ __('messages.quiz.previous') }}
        </button>
        <div x-show="showResults || currentIndex === 0"></div>

        <div class="flex gap-3">
            <button x-show="!showResults && currentIndex < questions.length - 1" @click="next()"
                    class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                    style="background-color: var(--color-accent);">
                {{ __('messages.quiz.next') }}
            </button>
            <button x-show="!showResults && currentIndex === questions.length - 1" @click="finish()"
                    class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                    style="background-color: var(--color-success);">
                {{ __('messages.quiz.finish') }}
            </button>
            <a x-show="showResults" href="{{ route('dashboard') }}"
               class="px-4 py-2 rounded-lg text-sm font-medium"
               style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                {{ __('messages.quiz.close') }}
            </a>
        </div>
    </div>
</div>

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

@push('scripts')
<script>
function quizPlayer() {
    const questions = @json($questionsJson);

    return {
        questions,
        currentIndex: 0,
        selectedAnswers: {},
        showResults: false,
        scoreDisplay: '',
        scorePercent: 0,
        scoreColor: '',

        get currentQuestion() {
            return this.questions[this.currentIndex];
        },

        get progressText() {
            return '{{ __('messages.quiz.progress', ['current' => "CURR", 'total' => "TOTAL"]) }}'
                .replace('CURR', this.currentIndex + 1)
                .replace('TOTAL', this.questions.length);
        },

        get progressPercent() {
            if (this.showResults) return 100;
            return ((this.currentIndex + 1) / this.questions.length) * 100;
        },

        isSelected(answerId) {
            const qId = this.currentQuestion.id;
            return (this.selectedAnswers[qId] || []).includes(answerId);
        },

        selectAnswer(questionId, answerId, type) {
            if (!this.selectedAnswers[questionId]) {
                this.selectedAnswers[questionId] = [];
            }
            if (type === 'radio') {
                this.selectedAnswers[questionId] = [answerId];
            } else {
                const idx = this.selectedAnswers[questionId].indexOf(answerId);
                if (idx > -1) {
                    this.selectedAnswers[questionId].splice(idx, 1);
                } else {
                    this.selectedAnswers[questionId].push(answerId);
                }
            }
        },

        prev() {
            if (this.currentIndex > 0) this.currentIndex--;
        },

        next() {
            if (this.currentIndex < this.questions.length - 1) this.currentIndex++;
        },

        async finish() {
            const response = await fetch('{{ route('quiz.score', $questionnaire) }}', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content,
                },
                body: JSON.stringify({ answers: this.selectedAnswers }),
            });

            const result = await response.json();
            const score = result.score;
            const maxScore = result.maxScore;

            this.scorePercent = result.percent;
            this.scoreDisplay = score + '/' + maxScore;

            if (this.scorePercent >= 80) {
                this.scoreColor = 'var(--color-success)';
            } else if (this.scorePercent >= 50) {
                this.scoreColor = 'var(--color-warning)';
            } else {
                this.scoreColor = 'var(--color-danger)';
            }

            this.showResults = true;
        },
    };
}
</script>
@endpush
@endsection
