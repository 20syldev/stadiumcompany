<!DOCTYPE html>
<html lang="{{ str_replace('_', '-', app()->getLocale()) }}" class="{{ session('theme', 'Light') === 'Dark' ? 'dark' : '' }}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>{{ __('messages.privacy.title') }} — {{ __('messages.app.title') }}</title>
    <link rel="preconnect" href="https://fonts.bunny.net">
    <link href="https://fonts.bunny.net/css?family=figtree:400,500,600,700&display=swap" rel="stylesheet" />
    @vite(['resources/css/app.css', 'resources/js/app.js'])
    <script>window.__TRANSLATIONS__ = @json(trans('messages'));</script>
</head>
<body class="font-sans antialiased min-h-screen py-12 px-6" style="background-color: var(--color-bg-app); color: var(--color-text-primary);">

    <div class="max-w-2xl mx-auto">

        <!-- Back link -->
        <div class="mb-8">
            @auth
                <a href="{{ route('dashboard') }}" class="text-sm hover:underline" style="color: var(--color-text-tertiary);">
                    ← {{ __('messages.privacy.back') }}
                </a>
            @else
                <a href="{{ route('login') }}" class="text-sm hover:underline" style="color: var(--color-text-tertiary);">
                    ← {{ __('messages.privacy.back') }}
                </a>
            @endauth
        </div>

        <!-- Card -->
        <div class="rounded-xl p-8" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-subtle);">

            <h1 class="text-xl font-bold mb-2" style="color: var(--color-text-primary);">{{ __('messages.privacy.title') }}</h1>
            <p class="text-xs mb-8" style="color: var(--color-text-tertiary);">{{ __('messages.privacy.subtitle') }}</p>

            <div class="space-y-6">
                @foreach (range(1, 7) as $i)
                    <div>
                        <h2 class="text-sm font-semibold mb-1" style="color: var(--color-text-primary);">
                            {{ __('messages.privacy.section' . $i . '_title') }}
                        </h2>
                        <p class="text-sm leading-relaxed" style="color: var(--color-text-secondary);">
                            {{ __('messages.privacy.section' . $i . '_body') }}
                        </p>
                    </div>
                @endforeach
            </div>
        </div>

        <!-- Footer -->
        <div class="mt-6 text-center">
            <p class="text-xs" style="color: var(--color-text-tertiary);">{{ __('messages.app.title') }}</p>
        </div>

    </div>

    <div id="cookie-consent-app"></div>
</body>
</html>
