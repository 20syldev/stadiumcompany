@extends('layouts.app')

@push('sidebar-nav')
    @if($tab !== 'results')
    <div class="mt-2 pt-2" style="border-top: 1px solid var(--color-border-subtle);">
        <p class="px-3 text-xs font-semibold uppercase tracking-wider mb-1" style="color: var(--color-text-tertiary);">
            Thèmes
        </p>
        <div class="space-y-0.5">
            <button
                onclick="window.dispatchEvent(new CustomEvent('filter-theme', { detail: { id: null } }))"
                data-theme-btn="null"
                class="w-full text-left px-3 py-1.5 rounded-lg text-xs font-medium transition"
                style="{{ !request('theme_id') ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary);' }}"
            >{{ __('messages.main.filter_all_themes') }}</button>

            @foreach($themes as $theme)
            <button
                onclick="window.dispatchEvent(new CustomEvent('filter-theme', { detail: { id: {{ $theme->id }} } }))"
                data-theme-btn="{{ $theme->id }}"
                class="w-full text-left px-3 py-1.5 rounded-lg text-xs font-medium transition truncate"
                style="{{ request('theme_id') == $theme->id ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary);' }}"
            >{{ __('messages.theme_label.names.' . $theme->name, [], app()->getLocale()) !== 'messages.theme_label.names.' . $theme->name ? __('messages.theme_label.names.' . $theme->name) : $theme->name }}</button>
            @endforeach
        </div>
    </div>
    @endif

    @if($tab === 'mine')
    <div class="mt-3 px-1 space-y-1.5">
        <a href="{{ route('questionnaires.create') }}"
           class="flex items-center justify-center gap-2 w-full px-3 py-2 rounded-lg text-xs font-semibold text-white transition hover:opacity-90"
           style="background-color: var(--color-accent);">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            {{ __('messages.main.new_questionnaire') }}
        </a>
        <a href="{{ route('questionnaires.export.csv') }}"
           class="flex items-center justify-center gap-2 w-full px-3 py-2 rounded-lg text-xs font-medium transition hover:opacity-80"
           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export CSV
        </a>
    </div>
    @endif
@endpush

@section('content')
<div id="dashboard-app"
     data-tab="{{ $tab }}"
     data-initial-search="{{ request('search', '') }}"
     data-initial-theme-id="{{ request('theme_id', '') }}"
     data-initial-view="{{ request('view', 'grid') }}"
     data-initial-sort="{{ request('sort', 'date') }}">
</div>
@endsection
