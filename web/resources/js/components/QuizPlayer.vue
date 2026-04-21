<template>
  <div class="max-w-3xl mx-auto">
    <!-- Demo Banner -->
    <div v-if="isDemoMode" class="rounded-lg px-4 py-3 mb-4 flex items-center gap-2 text-sm font-medium" style="background-color: var(--color-warning); color: #000;">
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"/>
      </svg>
      {{ t('quiz', 'demo_banner') }}
    </div>

    <!-- Header -->
    <div class="rounded-xl p-6 mb-4" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
      <div class="flex items-center justify-between mb-2">
        <h1 class="text-lg font-bold" style="color: var(--color-text-primary);">
          {{ isDemoMode ? t('quiz', 'demo_title') : t('quiz', 'title') }} - {{ questionnaireName }}
        </h1>
        <span class="text-sm" style="color: var(--color-text-tertiary);">{{ progressText }}</span>
      </div>
      <p v-if="themeName" class="text-sm mb-3" style="color: var(--color-text-secondary);">
        {{ t('quiz', 'theme').replace(':name', themeName) }}
      </p>
      <!-- Progress bar -->
      <div class="w-full h-2 rounded-full overflow-hidden" style="background-color: var(--color-border-subtle);">
        <div class="h-full rounded-full transition-all duration-300" :style="'background-color: var(--color-accent); width: ' + progressPercent + '%'"></div>
      </div>
    </div>

    <!-- Question Card -->
    <div v-if="!showResults" class="rounded-xl p-6" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
      <div class="flex items-start gap-3 mb-4">
        <span class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold text-white" style="background-color: var(--color-accent);">
          {{ currentQuestion.number }}
        </span>
        <div>
          <span class="text-xs font-medium px-2 py-0.5 rounded" style="background-color: var(--color-border-subtle); color: var(--color-text-tertiary);">
            {{ currentQuestion.answer_type === 'TRUE_FALSE' ? t('question', 'type_truefalse') : t('question', 'type_multiple') }}
          </span>
        </div>
      </div>
      <h2 class="text-base font-semibold mb-6" style="color: var(--color-text-primary);">{{ currentQuestion.label }}</h2>

      <!-- TRUE_FALSE answers -->
      <div v-if="currentQuestion.answer_type === 'TRUE_FALSE'" class="space-y-3">
        <label v-for="answer in currentQuestion.answers" :key="answer.id"
               class="flex items-center gap-3 p-3 rounded-lg cursor-pointer transition"
               :style="isSelected(answer.id) ? 'border: 2px solid var(--color-accent); background-color: var(--color-bg-card-hover);' : 'border: 2px solid var(--color-border-subtle);'">
          <input type="radio" :name="'q_' + currentQuestion.id" :value="answer.id"
                 @change="selectAnswer(currentQuestion.id, answer.id, 'radio')"
                 :checked="isSelected(answer.id)"
                 style="color: var(--color-accent);">
          <span class="text-sm" style="color: var(--color-text-primary);">{{ answer.label }}</span>
        </label>
      </div>

      <!-- MULTIPLE_CHOICE answers -->
      <div v-if="currentQuestion.answer_type === 'MULTIPLE_CHOICE'" class="space-y-3">
        <label v-for="answer in currentQuestion.answers" :key="answer.id"
               class="flex items-center gap-3 p-3 rounded-lg cursor-pointer transition"
               :style="isSelected(answer.id) ? 'border: 2px solid var(--color-accent); background-color: var(--color-bg-card-hover);' : 'border: 2px solid var(--color-border-subtle);'">
          <input type="checkbox" :value="answer.id"
                 @change="selectAnswer(currentQuestion.id, answer.id, 'checkbox')"
                 :checked="isSelected(answer.id)"
                 class="rounded" style="color: var(--color-accent);">
          <span class="text-sm" style="color: var(--color-text-primary);">{{ answer.label }}</span>
        </label>
      </div>

      <!-- Feedback section -->
      <div class="mt-6 pt-4" style="border-top: 1px solid var(--color-border-subtle);">
        <p v-if="feedbackSent[currentQuestion.id]" class="text-sm font-medium flex items-center gap-1.5"
           :style="feedbackSent[currentQuestion.id] === 'already' ? 'color: var(--color-accent);' : 'color: var(--color-success);'">
          <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"/></svg>
          {{ feedbackSent[currentQuestion.id] === 'already' ? t('feedback', 'already_sent') : t('feedback', 'saved') }}
        </p>
        <div v-else>
          <button type="button" @click="toggleFeedback(currentQuestion.id)"
                  class="flex items-center gap-2 text-sm font-semibold transition-colors hover:opacity-80"
                  style="color: var(--color-text-secondary);">
            <svg class="w-4 h-4 transition-transform duration-200" :class="feedbackOpen[currentQuestion.id] ? 'rotate-180' : ''"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="6 9 12 15 18 9"/></svg>
            {{ t('feedback', 'title') }}
          </button>
          <div v-show="feedbackOpen[currentQuestion.id]" class="mt-3">
            <!-- Star rating -->
            <div class="flex items-center gap-1 mb-3">
              <button v-for="star in 5" :key="star" type="button"
                      @click="setRating(currentQuestion.id, star)"
                      @mouseenter="setHoverRating(currentQuestion.id, star)"
                      @mouseleave="setHoverRating(currentQuestion.id, 0)"
                      class="focus:outline-none transition-transform duration-150"
                      :class="star === (hoverRating[currentQuestion.id] || 0) ? 'scale-125' : ''">
                <svg class="w-5 h-5 transition-colors duration-150"
                     viewBox="0 0 24 24"
                     :fill="star <= getStarDisplay(currentQuestion.id) ? 'currentColor' : 'none'"
                     stroke="currentColor" stroke-width="2"
                     :style="star <= getStarDisplay(currentQuestion.id) ? 'color: var(--color-warning)' : 'color: var(--color-border-subtle)'">
                  <polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/>
                </svg>
              </button>
            </div>
            <!-- Comment -->
            <textarea :value="getFeedbackForm(currentQuestion.id).comment"
                      @input="setComment(currentQuestion.id, $event.target.value)"
                      class="w-full rounded-lg p-3 text-sm resize-none"
                      style="background-color: var(--color-bg-page); border: 1px solid var(--color-border-subtle); color: var(--color-text-primary);"
                      rows="2"
                      :placeholder="t('feedback', 'comment_placeholder')"></textarea>
            <div class="flex justify-end mt-2">
              <button type="button" @click="submitFeedback(currentQuestion.id)"
                      :disabled="!getFeedbackForm(currentQuestion.id).rating"
                      class="px-4 py-1.5 rounded-lg text-xs font-semibold text-white transition hover:opacity-90 disabled:opacity-40"
                      style="background-color: var(--color-accent);">
                {{ t('feedback', 'submit') }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Results Card -->
    <div v-if="showResults" class="rounded-xl p-8 text-center" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
      <div class="mb-4">
        <svg v-if="scorePercent >= 80" class="w-16 h-16 mx-auto" style="color: var(--color-success);" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm-2 15l-5-5 1.41-1.41L10 14.17l7.59-7.59L19 8l-9 9z"/>
        </svg>
        <svg v-else-if="scorePercent >= 50" class="w-16 h-16 mx-auto" style="color: var(--color-warning);" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
        </svg>
        <svg v-else class="w-16 h-16 mx-auto" style="color: var(--color-danger);" fill="currentColor" viewBox="0 0 24 24">
          <path d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z"/>
        </svg>
      </div>
      <h2 class="text-xl font-bold mb-2" style="color: var(--color-text-primary);">{{ t('quiz', 'results_title') }}</h2>
      <p class="text-sm mb-3" style="color: var(--color-success);">{{ t('quiz', 'answers_saved') }}</p>
      <p class="text-lg mb-1" style="color: var(--color-text-secondary);">
        {{ t('quiz', 'score') }} {{ scoreDisplay }}
      </p>
      <p class="text-3xl font-bold" :style="'color: ' + scoreColor">{{ scorePercent }}%</p>
      <!-- Classement (Besoin 1) -->
      <p v-if="rankDisplay" class="text-lg font-semibold mt-3" style="color: var(--color-text-secondary);">
        {{ t('quiz', 'ranking') }} <span class="font-bold" style="color: var(--color-accent);">{{ rankDisplay }}</span>
      </p>
    </div>

    <!-- Navigation -->
    <div class="flex items-center justify-between mt-6">
      <button v-if="!showResults && currentIndex > 0" @click="prev()"
              class="px-4 py-2 rounded-lg text-sm font-medium"
              style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
        {{ t('quiz', 'previous') }}
      </button>
      <div v-else></div>

      <div class="flex gap-3">
        <button v-if="!showResults && currentIndex < questions.length - 1" @click="next()"
                class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                style="background-color: var(--color-accent);">
          {{ t('quiz', 'next') }}
        </button>
        <button v-if="!showResults && currentIndex === questions.length - 1" @click="finish()"
                class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                style="background-color: var(--color-success);">
          {{ t('quiz', 'finish') }}
        </button>
        <a v-if="showResults && submissionId" :href="reviewBaseUrl + '/' + submissionId"
           class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
           style="background-color: var(--color-accent);">
          {{ t('review', 'view_correction') }}
        </a>
        <a v-if="showResults" :href="dashboardUrl"
           class="px-4 py-2 rounded-lg text-sm font-medium"
           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
          {{ t('quiz', 'close') }}
        </a>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'QuizPlayer',

  data() {
    const d = window.__QUIZ_DATA__ || {};
    return {
      questions: d.questions || [],
      isDemoMode: d.isDemoMode || false,
      questionnaireName: d.questionnaireName || '',
      themeName: d.themeName || '',
      scoreUrl: d.scoreUrl || '',
      reviewBaseUrl: d.reviewBaseUrl || '',
      dashboardUrl: d.dashboardUrl || '',
      feedbackBaseUrl: d.feedbackBaseUrl || '',
      translations: d.translations || {},
      currentIndex: 0,
      selectedAnswers: {},
      showResults: false,
      scoreDisplay: '',
      scorePercent: 0,
      scoreColor: '',
      submissionId: null,
      rankDisplay: '',
      feedbackForms: {},
      feedbackSent: Object.fromEntries((d.alreadySentFeedback || []).map(id => [id, 'already'])),
      feedbackOpen: {},
      hoverRating: {},
    };
  },

  computed: {
    currentQuestion() {
      return this.questions[this.currentIndex];
    },
    progressText() {
      return this.t('quiz', 'progress')
        .replace(':current', this.currentIndex + 1)
        .replace(':total', this.questions.length);
    },
    progressPercent() {
      if (this.showResults) return 100;
      return ((this.currentIndex + 1) / this.questions.length) * 100;
    },
  },

  methods: {
    t(section, key) {
      return window.__TRANSLATIONS__?.[section]?.[key] || this.translations?.[section]?.[key] || key;
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

    toggleFeedback(questionId) {
      this.feedbackOpen[questionId] = !this.feedbackOpen[questionId];
    },

    getFeedbackForm(questionId) {
      if (!this.feedbackForms[questionId]) {
        this.feedbackForms[questionId] = { rating: 0, comment: '' };
      }
      return this.feedbackForms[questionId];
    },

    setRating(questionId, rating) {
      this.getFeedbackForm(questionId).rating = rating;
    },

    setComment(questionId, comment) {
      this.getFeedbackForm(questionId).comment = comment;
    },

    setHoverRating(questionId, rating) {
      this.hoverRating[questionId] = rating;
    },

    getStarDisplay(questionId) {
      return this.hoverRating[questionId] || this.getFeedbackForm(questionId).rating || 0;
    },

    async submitFeedback(questionId) {
      const form = this.getFeedbackForm(questionId);
      const url = this.feedbackBaseUrl + '/' + questionId + '/feedback';
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content,
        },
        body: JSON.stringify({ rating: form.rating, comment: form.comment || null }),
      });
      if (response.ok) {
        this.feedbackSent[questionId] = 'just';
      }
    },

    async finish() {
      const response = await fetch(this.scoreUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-CSRF-TOKEN': document.querySelector('meta[name="csrf-token"]').content,
        },
        body: JSON.stringify({ answers: this.selectedAnswers }),
      });

      const result = await response.json();
      this.submissionId = result.submissionId;
      this.scorePercent = result.percent;
      this.scoreDisplay = result.score + '/' + result.maxScore;

      if (result.rank && result.total) {
        this.rankDisplay = result.rank + '/' + result.total;
      }

      if (this.scorePercent >= 80) {
        this.scoreColor = 'var(--color-success)';
      } else if (this.scorePercent >= 50) {
        this.scoreColor = 'var(--color-warning)';
      } else {
        this.scoreColor = 'var(--color-danger)';
      }

      this.showResults = true;
    },
  },
};
</script>
