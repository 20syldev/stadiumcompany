<!DOCTYPE html>
<html lang="{{ str_replace('_', '-', app()->getLocale()) }}" class="{{ session('theme', 'Light') === 'Dark' ? 'dark' : '' }}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>{{ __('messages.app.title') }}</title>
    <link rel="preconnect" href="https://fonts.bunny.net">
    <link href="https://fonts.bunny.net/css?family=figtree:400,500,600,700&display=swap" rel="stylesheet" />
    @vite(['resources/css/app.css', 'resources/js/app.js'])
    <script>window.__TRANSLATIONS__ = @json(trans('messages'));</script>
</head>
<body class="font-sans antialiased flex h-screen overflow-hidden" style="background-color: var(--color-bg-app); color: var(--color-text-primary);">

    <!-- Sidebar -->
    <aside class="w-56 shrink-0 flex flex-col h-screen" style="background-color: var(--color-bg-card); border-right: 1px solid var(--color-border-subtle);">

        <!-- Logo -->
        <div class="h-14 flex items-center px-4 shrink-0" style="border-bottom: 1px solid var(--color-border-subtle);">
            <a href="{{ route('dashboard') }}" class="flex items-center gap-2.5">
                <span class="flex items-center justify-center w-7 h-7 rounded-lg text-white" style="background-color: var(--color-accent);">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"/>
                    </svg>
                </span>
                <span class="text-sm font-bold" style="color: var(--color-text-primary);">{{ __('messages.app.title') }}</span>
            </a>
        </div>

        <!-- Nav -->
        <nav class="flex-1 overflow-y-auto p-2 space-y-0.5">
            @php
                $currentTab = request()->routeIs('dashboard') ? request()->query('tab', 'published') : '';
            @endphp

            <a href="{{ route('dashboard', ['tab' => 'published']) }}"
               class="flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-medium transition"
               style="{{ $currentTab === 'published' ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary);' }}">
                <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
                </svg>
                {{ __('messages.main.tab_published') }}
            </a>

            <a href="{{ route('dashboard', ['tab' => 'mine']) }}"
               class="flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-medium transition"
               style="{{ $currentTab === 'mine' ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary);' }}">
                <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                </svg>
                {{ __('messages.main.tab_mine') }}
            </a>

            <a href="{{ route('dashboard', ['tab' => 'results']) }}"
               class="flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-medium transition"
               style="{{ $currentTab === 'results' ? 'background-color: var(--color-accent); color: white;' : 'color: var(--color-text-secondary);' }}">
                <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/>
                </svg>
                {{ __('messages.review.tab_results') }}
            </a>

            @stack('sidebar-nav')
        </nav>

        <!-- User section -->
        <div class="shrink-0 p-2" style="border-top: 1px solid var(--color-border-subtle);">
            <div class="relative">
                <button id="user-menu-btn"
                        class="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg transition hover:opacity-80"
                        style="color: var(--color-text-secondary);">
                    <span class="flex items-center justify-center w-7 h-7 rounded-full text-white text-xs font-bold shrink-0" style="background-color: var(--color-accent);">
                        {{ strtoupper(substr(auth()->user()->first_name ?? auth()->user()->email, 0, 1)) }}
                    </span>
                    <span class="text-sm font-medium truncate flex-1 text-left" style="color: var(--color-text-primary);">
                        {{ auth()->user()->full_name ?? auth()->user()->email }}
                    </span>
                    <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" viewBox="0 0 24 24">
                        <path d="M7 15l5 5 5-5"/><path d="M7 9l5-5 5 5"/>
                    </svg>
                </button>

                <div id="user-menu-dropdown"
                     class="hidden absolute bottom-full left-0 right-0 mb-1 rounded-lg shadow-lg py-1 z-50"
                     style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-subtle);">
                    <form method="POST" action="{{ route('preferences.toggle-theme') }}">
                        @csrf
                        <button type="submit" class="w-full text-left px-4 py-2 text-sm hover:opacity-80" style="color: var(--color-text-primary);">
                            {{ session('theme', 'Light') === 'Light' ? __('messages.user_menu.dark_theme') : __('messages.user_menu.light_theme') }}
                        </button>
                    </form>
                    <form method="POST" action="{{ route('preferences.toggle-language') }}">
                        @csrf
                        <button type="submit" class="w-full text-left px-4 py-2 text-sm hover:opacity-80" style="color: var(--color-text-primary);">
                            {{ __('messages.user_menu.language') }}
                        </button>
                    </form>
                    @if(auth()->user()->is_admin)
                        <a href="{{ route('admin.logs.index') }}" class="block px-4 py-2 text-sm hover:opacity-80" style="color: var(--color-text-primary);">
                            {{ __('messages.admin.logs_title') }}
                        </a>
                        <a href="{{ route('admin.rankings.index') }}" class="block px-4 py-2 text-sm hover:opacity-80" style="color: var(--color-text-primary);">
                            {{ __('messages.admin.rankings_title') }}
                        </a>
                    @endif
                    <hr style="border-color: var(--color-border-subtle);">
                    <form method="POST" action="{{ route('logout') }}">
                        @csrf
                        <button type="submit" class="w-full text-left px-4 py-2 text-sm" style="color: var(--color-danger);">
                            {{ __('messages.user_menu.logout') }}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    </aside>

    <!-- Main area -->
    <div class="flex-1 flex flex-col min-h-0 overflow-hidden">

        <!-- Pending Migrations Warning -->
        @if (!empty($pendingMigrations))
            <div class="px-6 pt-4">
                <div class="rounded-lg px-4 py-3 text-sm font-medium flex items-center gap-2" style="background-color: var(--color-warning); color: #000;">
                    <svg class="w-5 h-5 shrink-0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/>
                    </svg>
                    {{ $pendingMigrations }} migration(s) en attente — <code class="font-mono">php artisan migrate</code>
                </div>
            </div>
        @endif

        <!-- Flash Messages -->
        @if (session('success'))
            <div class="px-6 pt-4">
                <div class="rounded-lg px-4 py-3 text-sm text-white" style="background-color: var(--color-success);">
                    {{ session('success') }}
                </div>
            </div>
        @endif
        @if (session('error'))
            <div class="px-6 pt-4">
                <div class="rounded-lg px-4 py-3 text-sm text-white" style="background-color: var(--color-danger);">
                    {{ session('error') }}
                </div>
            </div>
        @endif

        <!-- Scrollable content -->
        <main class="flex-1 overflow-y-auto px-6 py-6">
            @yield('content')
        </main>
    </div>

    @stack('scripts')
    <div id="cookie-consent-app"></div>
</body>
</html>
