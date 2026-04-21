<template>
  <div>
    <!-- Leaderboard view -->
    <template v-if="selectedQuestionnaire">
      <div class="flex items-center gap-3 mb-6">
        <button @click="backToList()"
                class="inline-flex items-center gap-2 px-3 py-2 rounded-lg text-sm transition hover:opacity-80"
                style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
          {{ t('admin.rankings_back') }}
        </button>
        <div>
          <h1 class="text-xl font-bold" style="color: var(--color-text-primary);">{{ selectedQuestionnaire.name }}</h1>
          <p class="text-sm" style="color: var(--color-text-secondary);">{{ selectedQuestionnaire.theme }}</p>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="leaderboardLoading" class="flex justify-center py-12">
        <svg class="animate-spin w-7 h-7" fill="none" viewBox="0 0 24 24" style="color: var(--color-accent);">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
        </svg>
      </div>

      <!-- Leaderboard table -->
      <div v-else class="rounded-xl overflow-hidden" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <div v-if="leaderboard.length === 0" class="py-12 text-center text-sm" style="color: var(--color-text-tertiary);">
          {{ t('admin.rankings_leaderboard_empty') }}
        </div>
        <table v-else class="w-full text-sm">
          <thead>
            <tr style="border-bottom: 1px solid var(--color-border-subtle);">
              <th class="px-4 py-3 text-left font-semibold" style="color: var(--color-text-secondary);">{{ t('admin.col_rank') }}</th>
              <th class="px-4 py-3 text-left font-semibold" style="color: var(--color-text-secondary);">{{ t('admin.col_name') }}</th>
              <th class="px-4 py-3 text-left font-semibold hidden sm:table-cell" style="color: var(--color-text-secondary);">{{ t('admin.col_email') }}</th>
              <th class="px-4 py-3 text-right font-semibold" style="color: var(--color-text-secondary);">{{ t('admin.col_score') }}</th>
              <th class="px-4 py-3 text-right font-semibold" style="color: var(--color-text-secondary);">{{ t('admin.col_percent') }}</th>
              <th class="px-4 py-3 text-right font-semibold hidden md:table-cell" style="color: var(--color-text-secondary);">{{ t('admin.col_date') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in leaderboard" :key="row.rank + '-' + row.user_email"
                style="border-bottom: 1px solid var(--color-border-subtle);">
              <td class="px-4 py-3">
                <span class="inline-flex items-center justify-center w-7 h-7 rounded-full text-xs font-bold"
                      :style="rankStyle(row.rank)">
                  {{ row.rank }}
                </span>
              </td>
              <td class="px-4 py-3 font-medium" style="color: var(--color-text-primary);">{{ row.user_name }}</td>
              <td class="px-4 py-3 hidden sm:table-cell" style="color: var(--color-text-secondary);">{{ row.user_email }}</td>
              <td class="px-4 py-3 text-right font-mono" style="color: var(--color-text-primary);">{{ row.score }}/{{ row.max_score }}</td>
              <td class="px-4 py-3 text-right font-semibold" :style="percentStyle(row.percent)">{{ row.percent }}%</td>
              <td class="px-4 py-3 text-right hidden md:table-cell" style="color: var(--color-text-secondary);">{{ row.date }}</td>
            </tr>
          </tbody>
        </table>

        <!-- Leaderboard pagination -->
        <div v-if="leaderboardLastPage > 1" class="flex items-center justify-center gap-2 p-4" style="border-top: 1px solid var(--color-border-subtle);">
          <button @click="loadLeaderboard(leaderboardPage - 1)"
                  :disabled="leaderboardPage <= 1"
                  class="px-3 py-1.5 rounded text-sm disabled:opacity-40"
                  style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            &laquo;
          </button>
          <span class="text-sm" style="color: var(--color-text-secondary);">{{ leaderboardPage }} / {{ leaderboardLastPage }}</span>
          <button @click="loadLeaderboard(leaderboardPage + 1)"
                  :disabled="leaderboardPage >= leaderboardLastPage"
                  class="px-3 py-1.5 rounded text-sm disabled:opacity-40"
                  style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            &raquo;
          </button>
        </div>
      </div>
    </template>

    <!-- Questionnaire list view -->
    <template v-else>
      <div class="flex items-center gap-3 mb-6">
        <h1 class="text-xl font-bold flex-1" style="color: var(--color-text-primary);">{{ t('admin.rankings_title') }}</h1>
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" style="color: var(--color-text-tertiary);">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0"/>
            </svg>
          </div>
          <input type="text" v-model="search" @input="debounceSearch()"
                 :placeholder="t('admin.rankings_search')"
                 class="pl-10 pr-4 py-2 rounded-xl text-sm border focus:outline-none"
                 style="background-color: var(--color-bg-card); border-color: var(--color-border-card); color: var(--color-text-primary);">
        </div>
      </div>

      <!-- Loading -->
      <div v-if="listLoading" class="flex justify-center py-12">
        <svg class="animate-spin w-7 h-7" fill="none" viewBox="0 0 24 24" style="color: var(--color-accent);">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
        </svg>
      </div>

      <!-- List -->
      <div v-else>
        <div v-if="questionnaires.length === 0" class="py-12 text-center text-sm" style="color: var(--color-text-tertiary);">
          {{ t('admin.rankings_empty') }}
        </div>

        <div v-else class="rounded-xl overflow-hidden" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
          <table class="w-full text-sm">
            <thead>
              <tr style="border-bottom: 1px solid var(--color-border-subtle);">
                <th class="px-4 py-3 text-left font-semibold" style="color: var(--color-text-secondary);">{{ t('main.col_name') }}</th>
                <th class="px-4 py-3 text-left font-semibold hidden sm:table-cell" style="color: var(--color-text-secondary);">{{ t('main.col_theme') }}</th>
                <th class="px-4 py-3 text-right font-semibold" style="color: var(--color-text-secondary);">{{ t('main.col_questions') }}</th>
                <th class="px-4 py-3 text-right font-semibold" style="color: var(--color-text-secondary);">Participants</th>
                <th class="px-4 py-3 text-right font-semibold hidden md:table-cell" style="color: var(--color-text-secondary);">Moy.</th>
                <th class="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="q in questionnaires" :key="q.id"
                  style="border-bottom: 1px solid var(--color-border-subtle);">
                <td class="px-4 py-3 font-medium" style="color: var(--color-text-primary);">{{ q.name }}</td>
                <td class="px-4 py-3 hidden sm:table-cell" style="color: var(--color-text-secondary);">{{ q.theme || '—' }}</td>
                <td class="px-4 py-3 text-right" style="color: var(--color-text-secondary);">{{ q.submissions_count }}</td>
                <td class="px-4 py-3 text-right" style="color: var(--color-text-secondary);">{{ q.submissions_count }}</td>
                <td class="px-4 py-3 text-right hidden md:table-cell" style="color: var(--color-text-secondary);">
                  {{ q.avg_score_percent !== null ? q.avg_score_percent + '%' : '—' }}
                </td>
                <td class="px-4 py-3 text-right">
                  <button @click="openLeaderboard(q)"
                          class="px-3 py-1.5 rounded-lg text-xs font-semibold text-white transition hover:opacity-90"
                          style="background-color: var(--color-accent);">
                    Classement
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- List pagination -->
        <div v-if="listLastPage > 1" class="flex items-center justify-center gap-2 mt-4">
          <button @click="loadList(listPage - 1)"
                  :disabled="listPage <= 1"
                  class="px-3 py-1.5 rounded text-sm disabled:opacity-40"
                  style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            &laquo;
          </button>
          <span class="text-sm" style="color: var(--color-text-secondary);">{{ listPage }} / {{ listLastPage }}</span>
          <button @click="loadList(listPage + 1)"
                  :disabled="listPage >= listLastPage"
                  class="px-3 py-1.5 rounded text-sm disabled:opacity-40"
                  style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            &raquo;
          </button>
        </div>
      </div>
    </template>
  </div>
</template>

<script>
export default {
  name: 'AdminRankings',

  props: {
    'data-questionnaires-url': String,
    'data-leaderboard-base-url': String,
  },

  data() {
    return {
      questionnairesUrl: this['data-questionnaires-url'] || '',
      leaderboardBaseUrl: this['data-leaderboard-base-url'] || '',

      // List state
      questionnaires: [],
      listLoading: false,
      listPage: 1,
      listLastPage: 1,
      search: '',
      debounceTimer: null,

      // Leaderboard state
      selectedQuestionnaire: null,
      leaderboard: [],
      leaderboardLoading: false,
      leaderboardPage: 1,
      leaderboardLastPage: 1,
    };
  },

  mounted() {
    this.loadList(1);
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

    debounceSearch() {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => this.loadList(1), 300);
    },

    async loadList(page) {
      this.listLoading = true;
      this.listPage = page;
      const params = new URLSearchParams({ page });
      if (this.search) params.set('search', this.search);
      try {
        const res = await fetch(this.questionnairesUrl + '?' + params, {
          headers: { 'X-Requested-With': 'XMLHttpRequest' },
        });
        const json = await res.json();
        this.questionnaires = json.data;
        this.listLastPage = json.last_page;
      } catch (e) {
        console.error(e);
      } finally {
        this.listLoading = false;
      }
    },

    async openLeaderboard(questionnaire) {
      this.selectedQuestionnaire = questionnaire;
      this.leaderboardPage = 1;
      await this.loadLeaderboard(1);
    },

    async loadLeaderboard(page) {
      this.leaderboardLoading = true;
      this.leaderboardPage = page;
      const url = `${this.leaderboardBaseUrl}/${this.selectedQuestionnaire.id}/leaderboard?page=${page}`;
      try {
        const res = await fetch(url, {
          headers: { 'X-Requested-With': 'XMLHttpRequest' },
        });
        const json = await res.json();
        this.leaderboard = json.data;
        this.leaderboardLastPage = json.last_page;
      } catch (e) {
        console.error(e);
      } finally {
        this.leaderboardLoading = false;
      }
    },

    backToList() {
      this.selectedQuestionnaire = null;
      this.leaderboard = [];
    },

    rankStyle(rank) {
      if (rank === 1) return 'background-color: #FFD700; color: #000;';
      if (rank === 2) return 'background-color: #C0C0C0; color: #000;';
      if (rank === 3) return 'background-color: #CD7F32; color: #fff;';
      return 'background-color: var(--color-bg-app); color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);';
    },

    percentStyle(percent) {
      if (percent >= 80) return 'color: var(--color-success); text-align: right;';
      if (percent >= 50) return 'color: var(--color-accent); text-align: right;';
      return 'color: var(--color-danger); text-align: right;';
    },
  },
};
</script>
