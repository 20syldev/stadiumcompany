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
<body class="font-sans antialiased min-h-screen flex items-center justify-center" style="background-color: var(--color-bg-app); color: var(--color-text-primary);">
    <div class="w-full max-w-md px-6">
        {{ $slot }}
    </div>
    <footer class="fixed bottom-8 left-0 right-0 text-center">
        <a href="{{ route('privacy') }}" class="text-xs hover:underline" style="color: var(--color-text-tertiary);">
            {{ __('messages.privacy.footer_link') }}
        </a>
    </footer>
    <div id="cookie-consent-app"></div>
</body>
</html>
