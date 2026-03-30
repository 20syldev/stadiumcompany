@extends('layouts.app')

@section('content')
<div>
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
        <h1 class="text-xl font-bold" style="color: var(--color-text-primary);">
            {{ __('messages.admin.logs_title') }}
        </h1>
        <a href="{{ route('dashboard') }}"
           class="px-4 py-2 rounded-lg text-sm font-medium transition hover:opacity-80"
           style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
            {{ __('messages.review.back') }}
        </a>
    </div>

    <!-- Filters -->
    <form method="GET" action="{{ route('admin.logs.index') }}"
          class="rounded-xl p-4 mb-6 flex flex-wrap items-end gap-3"
          style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <div class="flex-1 min-w-[200px]">
            <label class="block text-xs font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.admin.search_placeholder') }}</label>
            <input type="text" name="search" value="{{ request('search') }}"
                   placeholder="{{ __('messages.admin.search_placeholder') }}"
                   class="w-full rounded-lg px-3 py-2 text-sm"
                   style="background-color: var(--color-bg-page); border: 1px solid var(--color-border-subtle); color: var(--color-text-primary);">
        </div>
        <div>
            <label class="block text-xs font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_action') }}</label>
            <select name="action"
                    class="rounded-lg px-3 py-2 text-sm"
                    style="background-color: var(--color-bg-page); border: 1px solid var(--color-border-subtle); color: var(--color-text-primary);">
                <option value="">{{ __('messages.admin.filter_action') }}</option>
                @foreach($actions as $act)
                    <option value="{{ $act }}" {{ request('action') === $act ? 'selected' : '' }}>{{ $act }}</option>
                @endforeach
            </select>
        </div>
        <div>
            <label class="block text-xs font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.admin.filter_from') }}</label>
            <input type="date" name="from" value="{{ request('from') }}"
                   class="rounded-lg px-3 py-2 text-sm"
                   style="background-color: var(--color-bg-page); border: 1px solid var(--color-border-subtle); color: var(--color-text-primary);">
        </div>
        <div>
            <label class="block text-xs font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.admin.filter_to') }}</label>
            <input type="date" name="to" value="{{ request('to') }}"
                   class="rounded-lg px-3 py-2 text-sm"
                   style="background-color: var(--color-bg-page); border: 1px solid var(--color-border-subtle); color: var(--color-text-primary);">
        </div>
        <div class="flex gap-2">
            <button type="submit"
                    class="px-4 py-2 rounded-lg text-sm font-semibold text-white transition hover:opacity-90"
                    style="background-color: var(--color-accent);">
                {{ __('messages.admin.search') }}
            </button>
            <a href="{{ route('admin.logs.index') }}"
               class="px-4 py-2 rounded-lg text-sm font-medium transition hover:opacity-80"
               style="color: var(--color-text-secondary); border: 1px solid var(--color-border-subtle);">
                {{ __('messages.admin.reset') }}
            </a>
        </div>
    </form>

    <!-- Table -->
    @if($logs->isEmpty())
        <div class="flex flex-col items-center justify-center py-20" style="color: var(--color-text-tertiary);">
            <svg class="w-16 h-16 mb-4 opacity-30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
            </svg>
            <p class="text-sm">{{ __('messages.admin.empty') }}</p>
        </div>
    @else
        <div class="rounded-xl overflow-hidden" style="border: 1px solid var(--color-border-card);">
            <div class="overflow-x-auto">
                <table class="w-full text-sm">
                    <thead>
                        <tr style="background-color: var(--color-bg-card);">
                            <th class="px-4 py-3 text-left font-medium" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_date') }}</th>
                            <th class="px-4 py-3 text-left font-medium" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_user') }}</th>
                            <th class="px-4 py-3 text-left font-medium" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_action') }}</th>
                            <th class="px-4 py-3 text-left font-medium" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_entity') }}</th>
                            <th class="px-4 py-3 text-left font-medium" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_details') }}</th>
                            <th class="px-4 py-3 text-left font-medium" style="color: var(--color-text-secondary);">{{ __('messages.admin.col_source') }}</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach($logs as $log)
                            <tr style="border-top: 1px solid var(--color-border-subtle); background-color: {{ $loop->even ? 'var(--color-bg-card)' : 'var(--color-bg-page)' }};">
                                <td class="px-4 py-3 whitespace-nowrap" style="color: var(--color-text-tertiary);">
                                    {{ $log->created_at->format('d/m/Y H:i') }}
                                </td>
                                <td class="px-4 py-3" style="color: var(--color-text-primary);">
                                    {{ $log->user?->full_name ?? $log->user?->email ?? '—' }}
                                </td>
                                <td class="px-4 py-3">
                                    <span class="inline-block px-2 py-0.5 rounded text-xs font-medium"
                                          style="background-color: var(--color-accent); color: white; opacity: 0.9;">
                                        {{ $log->action }}
                                    </span>
                                </td>
                                <td class="px-4 py-3" style="color: var(--color-text-secondary);">
                                    @if($log->entity_type)
                                        {{ $log->entity_type }}#{{ $log->entity_id }}
                                    @else
                                        —
                                    @endif
                                </td>
                                <td class="px-4 py-3 max-w-xs truncate" style="color: var(--color-text-secondary);">
                                    {{ $log->details ?? '—' }}
                                </td>
                                <td class="px-4 py-3" style="color: var(--color-text-tertiary);">
                                    {{ $log->source }}
                                </td>
                            </tr>
                        @endforeach
                    </tbody>
                </table>
            </div>
        </div>

        <!-- Pagination -->
        <div class="mt-4 flex justify-center">
            {{ $logs->links() }}
        </div>
    @endif
</div>
@endsection
