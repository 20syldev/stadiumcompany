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
</head>
<body class="font-sans antialiased min-h-screen" style="background-color: var(--color-bg-app); color: var(--color-text-primary);">
    <!-- Navigation -->
    <nav style="background-color: var(--color-bg-card); border-bottom: 1px solid var(--color-border-subtle);">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="flex justify-between h-16">
                <div class="flex items-center gap-3">
                    <a href="{{ route('dashboard') }}" class="flex items-center gap-2">
                        <svg class="w-6 h-6" style="color: var(--color-accent);" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253"/>
                        </svg>
                        <span class="text-lg font-bold" style="color: var(--color-text-primary);">{{ __('messages.main.title') }}</span>
                    </a>
                </div>
                <div class="flex items-center">
                    <!-- User menu dropdown -->
                    <div x-data="{ open: false }" class="relative">
                        <button @click="open = !open" class="flex items-center gap-2 px-3 py-1.5 rounded-lg transition-all duration-150 hover:brightness-95" style="background-color: var(--color-bg-app); border: 1px solid var(--color-border-subtle);">
                            <span class="flex items-center justify-center w-7 h-7 rounded-full text-white text-xs font-bold" style="background-color: var(--color-accent);">
                                {{ strtoupper(substr(auth()->user()->first_name ?? auth()->user()->email, 0, 1)) }}
                            </span>
                            <span class="text-sm hidden sm:block" style="color: var(--color-text-primary);">
                                {{ auth()->user()->full_name ?? auth()->user()->email }}
                            </span>
                            <svg class="w-4 h-4" style="color: var(--color-text-secondary);" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
                            </svg>
                        </button>
                        <div x-show="open" @click.away="open = false" x-transition
                             class="absolute right-0 mt-2 w-48 rounded-lg shadow-lg py-1 z-50"
                             style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-subtle);">
                            <!-- Toggle Theme -->
                            <form method="POST" action="{{ route('preferences.toggle-theme') }}">
                                @csrf
                                <button type="submit" class="w-full text-left px-4 py-2 text-sm hover:opacity-80" style="color: var(--color-text-primary);">
                                    {{ session('theme', 'Light') === 'Light' ? __('messages.user_menu.dark_theme') : __('messages.user_menu.light_theme') }}
                                </button>
                            </form>
                            <!-- Toggle Language -->
                            <form method="POST" action="{{ route('preferences.toggle-language') }}">
                                @csrf
                                <button type="submit" class="w-full text-left px-4 py-2 text-sm hover:opacity-80" style="color: var(--color-text-primary);">
                                    {{ __('messages.user_menu.language') }}
                                </button>
                            </form>
                            <hr style="border-color: var(--color-border-subtle);">
                            <!-- Logout -->
                            <form method="POST" action="{{ route('logout') }}">
                                @csrf
                                <button type="submit" class="w-full text-left px-4 py-2 text-sm" style="color: var(--color-danger);">
                                    {{ __('messages.user_menu.logout') }}
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </nav>

    <!-- Flash Messages -->
    @if (session('success'))
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-4">
            <div class="rounded-lg px-4 py-3 text-sm text-white" style="background-color: var(--color-success);">
                {{ session('success') }}
            </div>
        </div>
    @endif
    @if (session('error'))
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-4">
            <div class="rounded-lg px-4 py-3 text-sm text-white" style="background-color: var(--color-danger);">
                {{ session('error') }}
            </div>
        </div>
    @endif

    <!-- Page Content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        @yield('content')
    </main>

    @stack('scripts')
</body>
</html>
