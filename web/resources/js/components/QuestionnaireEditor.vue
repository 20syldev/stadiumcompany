<template>
  <div class="max-w-4xl mx-auto">
    <h1 class="text-xl font-bold mb-6" style="color: var(--color-text-primary);">
      {{ title }}
    </h1>

    <form method="POST" :action="data.formAction" ref="editorForm" id="editor-form">
      <input type="hidden" name="_token" :value="data.csrfToken">
      <input v-if="data.isUpdate" type="hidden" name="_method" value="PUT">

      <!-- Metadata Card -->
      <div class="rounded-xl p-6 mb-6" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ t('editor.name') }}</label>
            <input type="text" name="name" :value="data.oldName"
                   class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                   style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                   :placeholder="t('editor.name_placeholder')"
                   :disabled="data.readonly" required>
            <p v-if="data.errors && data.errors.name" class="mt-1 text-sm" style="color: var(--color-danger);">{{ data.errors.name[0] }}</p>
          </div>
          <div>
            <label class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ t('editor.theme') }}</label>
            <div class="flex gap-2">
              <select name="theme_id" v-model="themeId"
                      class="flex-1 rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                      style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                      :disabled="data.readonly" required>
                <option v-for="theme in themes" :key="theme.id" :value="String(theme.id)">{{ theme.label }}</option>
                <option v-if="!data.readonly" value="__new__">{{ t('theme_label.other') }}</option>
              </select>
            </div>
            <p v-if="data.errors && data.errors.theme_id" class="mt-1 text-sm" style="color: var(--color-danger);">{{ data.errors.theme_id[0] }}</p>
          </div>
        </div>
        <template v-if="!data.readonly">
          <input type="hidden" name="published" v-model="publishValue">
        </template>
        <template v-else-if="data.isPublished">
          <span class="inline-block mt-4 px-2 py-0.5 rounded text-xs font-medium" style="background-color: var(--color-success); color: white;">
            {{ t('main.card_published') }}
          </span>
        </template>
      </div>

      <!-- New Theme Modal -->
      <div v-show="showNewTheme" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
        <div class="rounded-xl p-6 w-96" style="background-color: var(--color-bg-card);" @click.self="showNewTheme = false">
          <h3 class="text-lg font-bold mb-4" style="color: var(--color-text-primary);">{{ t('theme_label.new_title') }}</h3>
          <input type="text" v-model="newThemeName"
                 class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2 mb-4"
                 style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                 :placeholder="t('theme_label.new_prompt')">
          <div class="flex justify-end gap-2">
            <button type="button" @click="showNewTheme = false" class="px-4 py-2 rounded-lg text-sm" style="color: var(--color-text-secondary);">
              {{ t('common.cancel') }}
            </button>
            <button type="button" @click="createTheme()" class="px-4 py-2 rounded-lg text-sm font-semibold text-white" style="background-color: var(--color-accent);">
              {{ t('theme_label.create') }}
            </button>
          </div>
        </div>
      </div>

      <!-- Questions -->
      <h2 class="text-lg font-semibold mb-4" style="color: var(--color-text-primary);">{{ t('editor.questions') }}</h2>

      <div v-for="(question, qIdx) in questions" :key="qIdx"
           class="rounded-xl p-5 mb-4" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <div class="flex items-start gap-3 mb-4">
          <span class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold text-white" style="background-color: var(--color-accent);">
            {{ qIdx + 1 }}
          </span>
          <div class="flex-1">
            <input type="text" :name="'questions['+qIdx+'][label]'" v-model="question.label"
                   class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                   style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                   :placeholder="t('question.label_placeholder')"
                   :disabled="data.readonly" required>
          </div>
          <div class="flex items-center gap-2">
            <select :name="'questions['+qIdx+'][answer_type]'" v-model="question.answer_type"
                    @change="onTypeChange(qIdx)"
                    class="rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                    style="width: fit-content; background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                    :disabled="data.readonly">
              <option value="TRUE_FALSE">{{ t('question.type_truefalse') }}</option>
              <option value="MULTIPLE_CHOICE">{{ t('question.type_multiple') }}</option>
            </select>
            <button v-if="!data.readonly" type="button" @click="removeQuestion(qIdx)"
                    class="p-1.5 rounded hover:opacity-70" style="color: var(--color-danger);">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
              </svg>
            </button>
          </div>
        </div>

        <!-- TRUE_FALSE answers -->
        <div v-if="question.answer_type === 'TRUE_FALSE'" class="ml-11">
          <p class="text-xs font-medium mb-2" style="color: var(--color-text-tertiary);">{{ t('question.correct_answer') }}</p>
          <div class="flex gap-4 mb-3">
            <label class="inline-flex items-center gap-2 cursor-pointer">
              <input type="radio" :name="'questions['+qIdx+'][tf_correct]'" value="true"
                     v-model="question.tf_correct" :disabled="data.readonly"
                     style="color: var(--color-accent);">
              <span class="text-sm" style="color: var(--color-text-primary);">{{ t('question.true') }}</span>
            </label>
            <label class="inline-flex items-center gap-2 cursor-pointer">
              <input type="radio" :name="'questions['+qIdx+'][tf_correct]'" value="false"
                     v-model="question.tf_correct" :disabled="data.readonly"
                     style="color: var(--color-accent);">
              <span class="text-sm" style="color: var(--color-text-primary);">{{ t('question.false') }}</span>
            </label>
          </div>
          <div class="flex items-center gap-2">
            <label class="text-xs" style="color: var(--color-text-tertiary);">{{ t('question.col_weight') }}:</label>
            <input type="number" :name="'questions['+qIdx+'][tf_weight]'" v-model="question.tf_weight"
                   class="w-20 rounded-lg px-2 py-1 text-sm border"
                   style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                   step="0.5" min="0" :disabled="data.readonly">
          </div>
          <!-- Hidden inputs for TRUE_FALSE -->
          <input type="hidden" :name="'questions['+qIdx+'][answers][0][label]'" :value="t('question.true')">
          <input type="hidden" :name="'questions['+qIdx+'][answers][0][is_correct]'" :value="question.tf_correct === 'true' ? '1' : '0'">
          <input type="hidden" :name="'questions['+qIdx+'][answers][0][weight]'" :value="question.tf_correct === 'true' ? question.tf_weight : 0">
          <input type="hidden" :name="'questions['+qIdx+'][answers][1][label]'" :value="t('question.false')">
          <input type="hidden" :name="'questions['+qIdx+'][answers][1][is_correct]'" :value="question.tf_correct === 'false' ? '1' : '0'">
          <input type="hidden" :name="'questions['+qIdx+'][answers][1][weight]'" :value="question.tf_correct === 'false' ? question.tf_weight : 0">
        </div>

        <!-- MULTIPLE_CHOICE answers -->
        <div v-if="question.answer_type === 'MULTIPLE_CHOICE'" class="ml-11">
          <div class="flex items-center justify-between mb-2">
            <p class="text-xs font-medium" style="color: var(--color-text-tertiary);">{{ t('question.answers') }}</p>
            <div v-if="!data.readonly" class="flex items-center gap-2">
              <input type="number" v-model="question.distributeValue"
                     class="w-16 rounded px-2 py-1 text-xs border text-center"
                     style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                     step="0.5" min="0">
              <button type="button" @click="distributePoints(qIdx)"
                      class="text-xs px-2 py-1 rounded hover:opacity-80"
                      style="color: var(--color-accent); border: 1px solid var(--color-accent);">
                {{ t('editor.distribute_points') }}
              </button>
            </div>
          </div>

          <!-- Answer header -->
          <div class="grid grid-cols-12 gap-2 mb-1 text-xs font-medium" style="color: var(--color-text-tertiary);">
            <div class="col-span-6">{{ t('question.col_answer') }}</div>
            <div class="col-span-2 text-center">{{ t('question.col_correct') }}</div>
            <div class="col-span-2 text-center">{{ t('question.col_weight') }}</div>
            <div class="col-span-2"></div>
          </div>

          <!-- Answer rows -->
          <div v-for="(answer, aIdx) in question.answers" :key="aIdx"
               class="grid grid-cols-12 gap-2 mb-2 items-center">
            <div class="col-span-6">
              <input type="text" :name="'questions['+qIdx+'][answers]['+aIdx+'][label]'" v-model="answer.label"
                     class="w-full rounded-lg px-2 py-1.5 text-sm border"
                     style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                     :placeholder="t('editor.answer_placeholder')"
                     :disabled="data.readonly" required>
            </div>
            <div class="col-span-2 text-center">
              <input type="checkbox" :name="'questions['+qIdx+'][answers]['+aIdx+'][is_correct]'" value="1"
                     v-model="answer.is_correct" @change="answer.weight = answer.is_correct ? 1 : 0"
                     :disabled="data.readonly"
                     class="rounded" style="color: var(--color-accent);">
            </div>
            <div class="col-span-2">
              <input type="number" :name="'questions['+qIdx+'][answers]['+aIdx+'][weight]'" v-model="answer.weight"
                     class="w-full rounded-lg px-2 py-1.5 text-sm border text-center"
                     style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle);"
                     step="0.5" :disabled="data.readonly">
            </div>
            <div class="col-span-2 text-center">
              <button v-if="!data.readonly && question.answers.length > 2" type="button"
                      @click="removeAnswer(qIdx, aIdx)"
                      class="p-1 rounded hover:opacity-70" style="color: var(--color-danger);">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>
          </div>

          <button v-if="!data.readonly" type="button" @click="addAnswer(qIdx)"
                  class="text-sm mt-1 hover:underline" style="color: var(--color-accent);">
            + {{ t('editor.add_answer') }}
          </button>
        </div>
      </div>

      <!-- Add Question & Actions -->
      <div class="flex items-center justify-between mt-6">
        <button v-if="!data.readonly" type="button" @click="addQuestion()"
                class="inline-flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition hover:opacity-90"
                style="color: var(--color-accent); border: 1px solid var(--color-accent);">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          {{ t('editor.add_question') }}
        </button>
        <div v-else></div>

        <div class="flex gap-3">
          <a :href="data.dashboardUrl"
             class="px-6 py-2 rounded-lg text-sm font-medium"
             style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            {{ data.readonly ? t('editor.close') : t('editor.cancel') }}
          </a>
          <template v-if="!data.readonly">
            <button type="button"
                    @click="publishValue = '0'; $refs.editorForm.submit()"
                    class="px-6 py-2 rounded-lg text-sm font-medium transition hover:opacity-90"
                    style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
              {{ t('editor.save_draft') }}
            </button>
            <button type="button"
                    @click="publishValue = '1'; $refs.editorForm.submit()"
                    class="px-6 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                    style="background-color: var(--color-accent);">
              {{ t('editor.publish') }}
            </button>
          </template>
        </div>
      </div>
    </form>
  </div>
</template>

<script>
export default {
  name: 'QuestionnaireEditor',

  data() {
    const d = window.__EDITOR_DATA__ || {};
    return {
      data: d,
      themeId: String(d.themeId || ''),
      publishValue: d.publishValue || '0',
      showNewTheme: false,
      newThemeName: '',
      questions: d.questions || [],
      themes: d.themes || [],
    };
  },

  computed: {
    title() {
      if (this.data.readonly) return this.t('editor.view_questionnaire');
      if (this.data.isUpdate) return this.t('editor.edit_questionnaire');
      return this.t('editor.new_questionnaire');
    },
  },

  watch: {
    themeId(val) {
      if (val === '__new__') {
        this.showNewTheme = true;
      }
    },
  },

  methods: {
    t(key) {
      const trans = window.__TRANSLATIONS__ || {};
      const parts = key.split('.');
      let obj = trans;
      for (const part of parts) {
        if (obj == null || typeof obj !== 'object') return key;
        obj = obj[part];
      }
      return (typeof obj === 'string' ? obj : null) || key;
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
      const q = this.questions[qIdx];
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
      const q = this.questions[qIdx];
      const correctCount = q.answers.filter(a => a.is_correct).length;
      if (correctCount === 0) return;
      const total = parseFloat(q.distributeValue) || 1;
      const each = Math.round((total / correctCount) * 100) / 100;
      q.answers.forEach(a => {
        a.weight = a.is_correct ? each : 0;
      });
    },

    async createTheme() {
      if (!this.newThemeName.trim()) return;
      try {
        const response = await fetch(this.data.themeStoreUrl, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': this.data.csrfToken,
            'Accept': 'application/json',
          },
          body: JSON.stringify({ name: this.newThemeName }),
        });
        const theme = await response.json();
        if (theme.id) {
          this.themes.push({ id: theme.id, label: this.newThemeName });
          this.themeId = String(theme.id);
          this.showNewTheme = false;
          this.newThemeName = '';
        }
      } catch (e) {
        alert(this.t('common.error'));
      }
    },
  },
};
</script>
