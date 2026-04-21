<template>
  <div v-if="visible"
       class="fixed bottom-0 left-0 right-0 z-50 p-4"
       style="background-color: var(--color-bg-card); border-top: 1px solid var(--color-border-subtle); box-shadow: 0 -4px 16px rgba(0,0,0,0.1);">
    <div class="max-w-4xl mx-auto flex flex-col sm:flex-row items-start sm:items-center gap-4">
      <div class="flex-1">
        <p class="text-sm font-semibold mb-1" style="color: var(--color-text-primary);">{{ t('cookies.title') }}</p>
        <p class="text-sm" style="color: var(--color-text-secondary);">{{ t('cookies.message') }}</p>
      </div>
      <div class="flex gap-2 shrink-0">
        <button @click="decline()"
                class="px-4 py-2 rounded-lg text-sm font-medium transition hover:opacity-80"
                style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
          {{ t('cookies.decline') }}
        </button>
        <button @click="accept()"
                class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                style="background-color: var(--color-accent);">
          {{ t('cookies.accept') }}
        </button>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'CookieConsent',

  data() {
    return {
      visible: false,
    };
  },

  mounted() {
    this.visible = !this.getCookie('cookie_consent');
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

    getCookie(name) {
      const match = document.cookie.match(new RegExp('(?:^|; )' + name + '=([^;]*)'));
      return match ? decodeURIComponent(match[1]) : null;
    },

    setCookie(name, value, days) {
      const expires = new Date(Date.now() + days * 864e5).toUTCString();
      document.cookie = name + '=' + encodeURIComponent(value) + '; expires=' + expires + '; path=/; SameSite=Lax';
    },

    accept() {
      this.setCookie('cookie_consent', 'accepted', 365);
      this.visible = false;
    },

    decline() {
      this.setCookie('cookie_consent', 'declined', 365);
      this.visible = false;
    },
  },
};
</script>
