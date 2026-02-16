@extends('layouts.app')

@section('content')
<div>
    <!-- Tabs -->
    <div class="flex items-center justify-between mb-6">
        <div class="flex gap-2">
            <a href="{{ route('dashboard', ['tab' => 'mine']) }}"
               class="px-4 py-2 rounded-lg text-sm font-medium transition"
               style="{{ $tab === 'mine' ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary); background-color: var(--color-bg-card);' }}">
                {{ __('messages.main.tab_mine') }}
            </a>
            <a href="{{ route('dashboard', ['tab' => 'published']) }}"
               class="px-4 py-2 rounded-lg text-sm font-medium transition"
               style="{{ $tab === 'published' ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary); background-color: var(--color-bg-card);' }}">
                {{ __('messages.main.tab_published') }}
            </a>
        </div>

        @if($tab === 'mine')
            <a href="{{ route('questionnaires.create') }}"
               class="inline-flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
               style="background-color: var(--color-accent);">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
                </svg>
                {{ __('messages.main.new_questionnaire') }}
            </a>
        @endif
    </div>

    <!-- Cards Grid -->
    @php $items = $tab === 'mine' ? $myQuestionnaires : $publishedQuestionnaires; @endphp

    @if($items->isEmpty())
        <div class="flex flex-col items-center justify-center py-20" style="color: var(--color-text-tertiary);">
            <svg class="w-16 h-16 mb-4 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"/>
            </svg>
            <p class="text-sm">{{ $tab === 'mine' ? __('messages.main.empty_mine') : __('messages.main.empty_published') }}</p>
        </div>
    @else
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            @foreach($items as $q)
                <div class="rounded-xl p-5 transition-all hover:shadow-md"
                     style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
                    <!-- Name -->
                    <h3 class="font-semibold text-base mb-2 line-clamp-2" style="color: var(--color-text-primary);">
                        {{ $q->name }}
                    </h3>

                    <!-- Theme badge -->
                    @if($q->theme)
                        <span class="inline-block px-2 py-0.5 rounded text-xs font-medium mb-2"
                              style="background-color: var(--color-accent); color: white; opacity: 0.9;">
                            {{ __('messages.theme_label.names.' . $q->theme->name, [], app()->getLocale()) !== 'messages.theme_label.names.' . $q->theme->name ? __('messages.theme_label.names.' . $q->theme->name) : $q->theme->name }}
                        </span>
                    @endif

                    <!-- Info -->
                    <p class="text-xs mb-1" style="color: var(--color-text-tertiary);">
                        {{ __('messages.main.card_questions_count', ['count' => $q->questions_count]) }}
                    </p>

                    @if($tab === 'mine')
                        <span class="inline-block px-2 py-0.5 rounded text-xs font-medium"
                              style="{{ $q->published ? 'background-color: var(--color-success); color: white;' : 'color: var(--color-text-tertiary); border: 1px solid var(--color-border-subtle);' }}">
                            {{ $q->published ? __('messages.main.card_published') : __('messages.main.card_draft') }}
                        </span>
                    @else
                        <p class="text-xs" style="color: var(--color-text-tertiary);">
                            {{ __('messages.main.card_by', ['name' => $q->user->full_name ?? $q->user->email]) }}
                        </p>
                    @endif

                    <!-- Actions -->
                    <div class="flex flex-wrap items-center gap-2 mt-4 pt-3" style="border-top: 1px solid var(--color-border-subtle);">
                        @if($q->questions_count > 0)
                            <a href="{{ route('quiz.play', $q) }}"
                               class="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-medium text-white transition hover:opacity-90"
                               style="background-color: var(--color-accent);">
                                <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 24 24"><path d="M8 5v14l11-7z"/></svg>
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
                                @csrf
                                @method('DELETE')
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
</div>
@endsection
