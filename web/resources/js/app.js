import './bootstrap';
import { createApp } from 'vue';

import Dashboard from './components/Dashboard.vue';
import QuizPlayer from './components/QuizPlayer.vue';
import QuestionnaireEditor from './components/QuestionnaireEditor.vue';
import CookieConsent from './components/CookieConsent.vue';
import AdminRankings from './components/AdminRankings.vue';

function mountVue(id, Component, extraProps = {}) {
    const el = document.getElementById(id);
    if (!el) return;
    createApp(Component, { ...el.dataset, ...extraProps }).mount(el);
}

document.addEventListener('DOMContentLoaded', () => {
    mountVue('dashboard-app', Dashboard);
    mountVue('quiz-player-app', QuizPlayer);
    mountVue('editor-app', QuestionnaireEditor);
    mountVue('cookie-consent-app', CookieConsent);
    mountVue('rankings-app', AdminRankings);

    // User menu dropdown (simple vanilla JS toggle)
    const menuBtn = document.getElementById('user-menu-btn');
    const menuDropdown = document.getElementById('user-menu-dropdown');
    if (menuBtn && menuDropdown) {
        menuBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            menuDropdown.classList.toggle('hidden');
        });
        document.addEventListener('click', () => {
            menuDropdown.classList.add('hidden');
        });
    }
});
