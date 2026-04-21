{{-- Partial: cards grid/list for dashboard (used standalone for AJAX responses) --}}

@if($tab === 'results')

    @if($submissions->isEmpty())
        <div class="flex flex-col items-center justify-center py-20" style="color: var(--color-text-tertiary);">
            <svg class="w-16 h-16 mb-4 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
            </svg>
            <p class="text-sm">{{ __('messages.review.no_results') }}</p>
        </div>
    @else
        <div class="{{ $viewMode === 'list' ? 'flex flex-col gap-3' : 'grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4' }}">
            @foreach($submissions as $sub)
                @php $percent = $sub->max_score > 0 ? round(($sub->score / $sub->max_score) * 100) : 0; @endphp
                @if($viewMode === 'list')
                <div class="flex items-center gap-4 rounded-xl px-5 py-3 transition-all hover:shadow-md"
                     style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                    <div class="flex-1 min-w-0">
                        <h3 class="font-semibold text-sm truncate" style="color: var(--color-text-primary);">
                            {{ $sub->questionnaire?->name ?? __('messages.common.unknown') }}
                        </h3>
                        @if($sub->questionnaire?->theme)
                            <span class="inline-block px-2 py-0.5 rounded text-xs font-medium mt-0.5"
                                  style="background-color: var(--color-accent); color: white; opacity: 0.9;">
                                {{ $sub->questionnaire->theme->name }}
                            </span>
                        @endif
                    </div>
                    <div class="text-right shrink-0">
                        <p class="text-sm font-semibold" style="color: var(--color-text-primary);">{{ $sub->score }}/{{ $sub->max_score }}</p>
                        <p class="text-xs" style="color: var(--color-text-tertiary);">{{ $percent }}%</p>
                    </div>
                    <p class="text-xs shrink-0 hidden sm:block" style="color: var(--color-text-tertiary);">{{ $sub->created_at->format('d/m/Y') }}</p>
                    <a href="{{ route('quiz.review', $sub) }}"
                       class="shrink-0 inline-flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium text-white transition hover:opacity-90"
                       style="background-color: var(--color-accent);">
                        <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                        </svg>
                        {{ __('messages.review.view_correction') }}
                    </a>
                </div>
                @else
                <div class="rounded-xl p-5 transition-all hover:shadow-md"
                     style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                    <h3 class="font-semibold text-base mb-2 line-clamp-2" style="color: var(--color-text-primary);">
                        {{ $sub->questionnaire?->name ?? __('messages.common.unknown') }}
                    </h3>
                    @if($sub->questionnaire?->theme)
                        <span class="inline-block px-2 py-0.5 rounded text-xs font-medium mb-2"
                              style="background-color: var(--color-accent); color: white; opacity: 0.9;">
                            {{ $sub->questionnaire->theme->name }}
                        </span>
                    @endif
                    <p class="text-sm mb-1" style="color: var(--color-text-secondary);">
                        {{ __('messages.review.score_label') }} {{ $sub->score }}/{{ $sub->max_score }} ({{ $percent }}%)
                    </p>
                    <p class="text-xs mb-3" style="color: var(--color-text-tertiary);">{{ $sub->created_at->format('d/m/Y H:i') }}</p>
                    <div class="pt-3" style="border-top: 1px solid var(--color-border-subtle);">
                        <a href="{{ route('quiz.review', $sub) }}"
                           class="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium text-white transition hover:opacity-90"
                           style="background-color: var(--color-accent);">
                            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                            </svg>
                            {{ __('messages.review.view_correction') }}
                        </a>
                    </div>
                </div>
                @endif
            @endforeach
        </div>

        {{-- Pagination --}}
        @if($submissions->hasPages())
            <div class="mt-6 flex justify-center gap-1">
                @if($submissions->onFirstPage())
                    <span class="px-3 py-1.5 rounded-lg text-sm opacity-40 cursor-default" style="border: 1px solid var(--color-border-subtle); color: var(--color-text-tertiary);">&laquo;</span>
                @else
                    <button data-page="{{ $submissions->currentPage() - 1 }}"
                            class="px-3 py-1.5 rounded-lg text-sm transition hover:opacity-80"
                            style="border: 1px solid var(--color-border-subtle); color: var(--color-text-secondary);">&laquo;</button>
                @endif

                @foreach($submissions->getUrlRange(max(1, $submissions->currentPage() - 2), min($submissions->lastPage(), $submissions->currentPage() + 2)) as $page => $url)
                    <button data-page="{{ $page }}"
                            class="px-3 py-1.5 rounded-lg text-sm font-medium transition"
                            style="{{ $page == $submissions->currentPage() ? 'background-color: var(--color-accent); color: white;' : 'border: 1px solid var(--color-border-subtle); color: var(--color-text-secondary);' }}">
                        {{ $page }}
                    </button>
                @endforeach

                @if($submissions->hasMorePages())
                    <button data-page="{{ $submissions->currentPage() + 1 }}"
                            class="px-3 py-1.5 rounded-lg text-sm transition hover:opacity-80"
                            style="border: 1px solid var(--color-border-subtle); color: var(--color-text-secondary);">&raquo;</button>
                @else
                    <span class="px-3 py-1.5 rounded-lg text-sm opacity-40 cursor-default" style="border: 1px solid var(--color-border-subtle); color: var(--color-text-tertiary);">&raquo;</span>
                @endif
            </div>
        @endif
    @endif

@else

    @if($items->isEmpty())
        <div class="flex flex-col items-center justify-center py-20" style="color: var(--color-text-tertiary);">
            <svg class="w-16 h-16 mb-4 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0"/>
            </svg>
            <p class="text-sm">{{ __('messages.main.no_results_search') }}</p>
        </div>
    @else

        @if($viewMode === 'list')
        {{-- List view --}}
        <div class="flex flex-col gap-3">
            @foreach($items as $q)
            <div class="flex items-center gap-4 rounded-xl px-5 py-3 transition-all hover:shadow-md"
                 style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                <!-- Theme badge -->
                @if($q->theme)
                    <span class="hidden sm:inline-block shrink-0 px-2 py-0.5 rounded text-xs font-medium"
                          style="background-color: var(--color-accent); color: white; opacity: 0.9;">
                        {{ __('messages.theme_label.names.' . $q->theme->name, [], app()->getLocale()) !== 'messages.theme_label.names.' . $q->theme->name ? __('messages.theme_label.names.' . $q->theme->name) : $q->theme->name }}
                    </span>
                @endif

                <!-- Name -->
                <div class="flex-1 min-w-0">
                    <h3 class="font-semibold text-sm truncate" style="color: var(--color-text-primary);">{{ $q->name }}</h3>
                    <p class="text-xs mt-0.5" style="color: var(--color-text-tertiary);">
                        {{ __('messages.main.card_questions_count', ['count' => $q->questions_count]) }}
                        @if($tab === 'published')
                            &nbsp;·&nbsp; {{ __('messages.main.card_by', ['name' => $q->user->full_name ?? $q->user->email]) }}
                        @else
                            &nbsp;·&nbsp;
                            <span style="{{ $q->published ? 'color: var(--color-success)' : 'color: var(--color-text-tertiary)' }}">
                                {{ $q->published ? __('messages.main.card_published') : __('messages.main.card_draft') }}
                            </span>
                        @endif
                    </p>
                </div>

                <!-- Stats -->
                <div class="hidden md:flex items-center gap-3 shrink-0 text-xs" style="color: var(--color-text-tertiary);">
                    <span class="inline-flex items-center gap-1">
                        <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 24 24"><path d="M6 4a3 3 0 00-3 3v10a3 3 0 004.59 2.54l9-5a3 3 0 000-5.08l-9-5A3 3 0 006 4z"/></svg>
                        {{ __('messages.main.stats_played', ['count' => $q->submissions_count]) }}
                    </span>
                    @if($q->avg_score_percent !== null)
                    <span>{{ __('messages.main.stats_avg_score', ['score' => $q->avg_score_percent]) }}</span>
                    @endif
                </div>

                <!-- Actions -->
                <div class="flex items-center gap-2 shrink-0">
                    @if($q->questions_count > 0)
                        <a href="{{ route('quiz.play', $q) }}"
                           class="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium text-white transition hover:opacity-90"
                           style="background-color: var(--color-accent);">
                            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M6 4a3 3 0 00-3 3v10a3 3 0 004.59 2.54l9-5a3 3 0 000-5.08l-9-5A3 3 0 006 4z"/></svg>
                            {{ __('messages.main.action_play') }}
                        </a>
                    @endif
                    @if($tab === 'mine')
                        <a href="{{ route('questionnaires.edit', $q) }}"
                           class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_edit') }}
                        </a>
                        <a href="{{ route('questionnaires.pdf', $q) }}"
                           class="hidden sm:inline-block px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_pdf') }}
                        </a>
                        <form method="POST" action="{{ route('questionnaires.destroy', $q) }}"
                              onsubmit="return confirm('{{ __('messages.main.confirm_delete_message', ['name' => addslashes($q->name)]) }}')">
                            @csrf @method('DELETE')
                            <button type="submit" class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                                    style="color: var(--color-danger); border: 1px solid var(--color-danger);">
                                {{ __('messages.main.action_delete') }}
                            </button>
                        </form>
                    @else
                        <a href="{{ route('questionnaires.show', $q) }}"
                           class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_view') }}
                        </a>
                        <form method="POST" action="{{ route('questionnaires.fork', $q) }}"
                              onsubmit="return confirm('{{ __('messages.main.confirm_fork_message', ['name' => addslashes($q->name)]) }}')">
                            @csrf
                            <button type="submit" class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                                    style="color: var(--color-accent); border: 1px solid var(--color-accent);">
                                {{ __('messages.main.action_fork') }}
                            </button>
                        </form>
                    @endif
                </div>
            </div>
            @endforeach
        </div>

        @else
        {{-- Grid view --}}
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            @foreach($items as $q)
            <div class="rounded-xl p-5 flex flex-col transition-all hover:shadow-md"
                 style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                <!-- Name -->
                <h3 class="font-semibold text-base mb-2 line-clamp-2" style="color: var(--color-text-primary);">{{ $q->name }}</h3>

                <!-- Theme badge -->
                @if($q->theme)
                    <span class="inline-block self-start px-2 py-0.5 rounded text-xs font-medium mb-2"
                          style="background-color: var(--color-accent); color: white; opacity: 0.9;">
                        {{ __('messages.theme_label.names.' . $q->theme->name, [], app()->getLocale()) !== 'messages.theme_label.names.' . $q->theme->name ? __('messages.theme_label.names.' . $q->theme->name) : $q->theme->name }}
                    </span>
                @endif

                <!-- Info row -->
                <p class="text-xs mb-1" style="color: var(--color-text-tertiary);">
                    {{ __('messages.main.card_questions_count', ['count' => $q->questions_count]) }}
                </p>

                @if($tab === 'mine')
                    <span class="inline-block self-start px-2 py-0.5 rounded text-xs font-medium mb-2"
                          style="{{ $q->published ? 'background-color: var(--color-success); color: white;' : 'color: var(--color-text-tertiary); border: 1px solid var(--color-border-subtle);' }}">
                        {{ $q->published ? __('messages.main.card_published') : __('messages.main.card_draft') }}
                    </span>
                @else
                    <p class="text-xs mb-2" style="color: var(--color-text-tertiary);">
                        {{ __('messages.main.card_by', ['name' => $q->user->full_name ?? $q->user->email]) }}
                    </p>
                @endif

                <!-- Stats -->
                <div class="flex items-center gap-3 mb-3 text-xs" style="color: var(--color-text-tertiary);">
                    <span class="inline-flex items-center gap-1">
                        <svg class="w-3 h-3" fill="currentColor" viewBox="0 0 24 24"><path d="M6 4a3 3 0 00-3 3v10a3 3 0 004.59 2.54l9-5a3 3 0 000-5.08l-9-5A3 3 0 006 4z"/></svg>
                        {{ __('messages.main.stats_played', ['count' => $q->submissions_count]) }}
                    </span>
                    @if($q->avg_score_percent !== null)
                    <span class="inline-flex items-center gap-1">
                        <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" viewBox="0 0 24 24"><path d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
                        {{ __('messages.main.stats_avg_score', ['score' => $q->avg_score_percent]) }}
                    </span>
                    @endif
                </div>

                <!-- Actions -->
                <div class="flex flex-wrap items-center gap-2 mt-auto pt-3" style="border-top: 1px solid var(--color-border-subtle);">
                    @if($q->questions_count > 0)
                        <a href="{{ route('quiz.play', $q) }}"
                           class="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium text-white transition hover:opacity-90"
                           style="background-color: var(--color-accent);">
                            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M6 4a3 3 0 00-3 3v10a3 3 0 004.59 2.54l9-5a3 3 0 000-5.08l-9-5A3 3 0 006 4z"/></svg>
                            {{ __('messages.main.action_play') }}
                        </a>
                    @endif

                    @if($tab === 'mine')
                        <a href="{{ route('questionnaires.edit', $q) }}"
                           class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_edit') }}
                        </a>
                        <a href="{{ route('questionnaires.pdf', $q) }}"
                           class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_pdf') }}
                        </a>
                        <form method="POST" action="{{ route('questionnaires.destroy', $q) }}"
                              onsubmit="return confirm('{{ __('messages.main.confirm_delete_message', ['name' => addslashes($q->name)]) }}')">
                            @csrf @method('DELETE')
                            <button type="submit" class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                                    style="color: var(--color-danger); border: 1px solid var(--color-danger);">
                                {{ __('messages.main.action_delete') }}
                            </button>
                        </form>
                    @else
                        <a href="{{ route('questionnaires.show', $q) }}"
                           class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_view') }}
                        </a>
                        <form method="POST" action="{{ route('questionnaires.fork', $q) }}"
                              onsubmit="return confirm('{{ __('messages.main.confirm_fork_message', ['name' => addslashes($q->name)]) }}')">
                            @csrf
                            <button type="submit" class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                                    style="color: var(--color-accent); border: 1px solid var(--color-accent);">
                                {{ __('messages.main.action_fork') }}
                            </button>
                        </form>
                        <a href="{{ route('questionnaires.pdf', $q) }}"
                           class="px-3 py-1.5 rounded-lg text-xs font-medium transition hover:opacity-80"
                           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                            {{ __('messages.main.action_pdf') }}
                        </a>
                    @endif
                </div>
            </div>
            @endforeach
        </div>
        @endif

        {{-- Pagination --}}
        @if($items->hasPages())
            <div class="mt-6 flex justify-center gap-1">
                @if($items->onFirstPage())
                    <span class="px-3 py-1.5 rounded-lg text-sm opacity-40 cursor-default" style="border: 1px solid var(--color-border-subtle); color: var(--color-text-tertiary);">&laquo;</span>
                @else
                    <button data-page="{{ $items->currentPage() - 1 }}"
                            class="px-3 py-1.5 rounded-lg text-sm transition hover:opacity-80"
                            style="border: 1px solid var(--color-border-subtle); color: var(--color-text-secondary);">&laquo;</button>
                @endif

                @foreach($items->getUrlRange(max(1, $items->currentPage() - 2), min($items->lastPage(), $items->currentPage() + 2)) as $page => $url)
                    <button data-page="{{ $page }}"
                            class="px-3 py-1.5 rounded-lg text-sm font-medium transition"
                            style="{{ $page == $items->currentPage() ? 'background-color: var(--color-accent); color: white;' : 'border: 1px solid var(--color-border-subtle); color: var(--color-text-secondary);' }}">
                        {{ $page }}
                    </button>
                @endforeach

                @if($items->hasMorePages())
                    <button data-page="{{ $items->currentPage() + 1 }}"
                            class="px-3 py-1.5 rounded-lg text-sm transition hover:opacity-80"
                            style="border: 1px solid var(--color-border-subtle); color: var(--color-text-secondary);">&raquo;</button>
                @else
                    <span class="px-3 py-1.5 rounded-lg text-sm opacity-40 cursor-default" style="border: 1px solid var(--color-border-subtle); color: var(--color-text-tertiary);">&raquo;</span>
                @endif
            </div>
        @endif
    @endif

@endif
