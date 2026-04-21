<template>
  <div>
    <!-- Search + controls (tabs: published & mine only) -->
    <div v-if="tab !== 'results'" class="flex items-center gap-3 mb-6">
      <div class="relative flex-1">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" style="color: var(--color-text-tertiary);">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0"/>
          </svg>
        </div>
        <input
          type="text"
          v-model="search"
          @input="debounceSearch"
          :placeholder="t('search_placeholder')"
          class="w-full pl-10 pr-4 py-2.5 rounded-xl text-sm border transition focus:outline-none"
          style="background-color: var(--color-bg-card); border-color: var(--color-border-card); color: var(--color-text-primary);"
        />
      </div>

      <div class="flex rounded-xl overflow-hidden shrink-0" style="border: 1px solid var(--color-border-card);">
        <button
          @click="setViewMode('grid')"
          class="px-3 py-2.5 transition"
          :style="viewMode === 'grid' ? 'background-color: var(--color-accent); color: white;' : 'background-color: var(--color-bg-card); color: var(--color-text-secondary);'"
        >
          <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
            <path d="M3 3h7v7H3V3zm0 11h7v7H3v-7zm11-11h7v7h-7V3zm0 11h7v7h-7v-7z"/>
          </svg>
        </button>
        <button
          @click="setViewMode('list')"
          class="px-3 py-2.5 transition"
          :style="viewMode === 'list' ? 'background-color: var(--color-accent); color: white;' : 'background-color: var(--color-bg-card); color: var(--color-text-secondary);'"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
          </svg>
        </button>
      </div>
    </div>

    <!-- Loading spinner -->
    <div v-if="loading" class="flex justify-center py-12">
      <svg class="animate-spin w-7 h-7" fill="none" viewBox="0 0 24 24" style="color: var(--color-accent);">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
      </svg>
    </div>

    <!-- Cards (server-rendered HTML) -->
    <div v-show="!loading" v-html="cardsHtml" ref="cardsContainer"></div>
  </div>
</template>

<script>
export default {
  name: 'Dashboard',

  props: {
    'data-tab': String,
    'data-initial-search': String,
    'data-initial-theme-id': String,
    'data-initial-view': String,
    'data-initial-sort': String,
  },

  data() {
    return {
      tab: this['data-tab'] || 'published',
      search: this['data-initial-search'] || '',
      themeId: this['data-initial-theme-id'] ? parseInt(this['data-initial-theme-id']) : null,
      viewMode: this['data-initial-view'] || 'grid',
      sort: this['data-initial-sort'] || 'date',
      loading: false,
      cardsHtml: '',
      debounceTimer: null,
    };
  },

  mounted() {
    this.fetchResults();
    window.addEventListener('filter-theme', this.onThemeFilter);
  },

  beforeUnmount() {
    window.removeEventListener('filter-theme', this.onThemeFilter);
  },

  methods: {
    t(key) {
      const trans = window.__TRANSLATIONS__?.main || {};
      return trans[key] || key;
    },

    onThemeFilter(event) {
      this.themeId = event.detail.id;
      // Update sidebar button styles
      document.querySelectorAll('[data-theme-btn]').forEach(btn => {
        const btnId = btn.dataset.themeBtn === 'null' ? null : parseInt(btn.dataset.themeBtn);
        const active = btnId === this.themeId;
        btn.style.cssText = active
          ? 'background-color: var(--color-accent); color: white;'
          : 'color: var(--color-text-secondary);';
      });
      this.fetchResults();
    },

    debounceSearch() {
      clearTimeout(this.debounceTimer);
      this.debounceTimer = setTimeout(() => this.fetchResults(), 300);
    },

    setViewMode(mode) {
      this.viewMode = mode;
      this.fetchResults();
    },

    async fetchResults(page = null) {
      this.loading = true;
      const params = new URLSearchParams();
      params.set('tab', this.tab);
      if (this.search) params.set('search', this.search);
      if (this.themeId !== null && this.themeId !== undefined) params.set('theme_id', this.themeId);
      params.set('sort', this.sort);
      params.set('view', this.viewMode);
      if (page) params.set('page', page);

      const url = window.location.pathname + '?' + params.toString();

      try {
        const response = await fetch(url, {
          headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        const html = await response.text();
        this.cardsHtml = html;
        window.history.replaceState({}, '', url);
        this.$nextTick(() => this.bindPaginationLinks());
      } catch (e) {
        console.error(e);
      } finally {
        this.loading = false;
      }
    },

    bindPaginationLinks() {
      if (!this.$refs.cardsContainer) return;
      this.$refs.cardsContainer.querySelectorAll('[data-page]').forEach(link => {
        link.addEventListener('click', (e) => {
          e.preventDefault();
          this.fetchResults(link.dataset.page);
        });
      });
    },
  },
};
</script>
