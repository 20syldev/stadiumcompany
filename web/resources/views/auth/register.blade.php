<x-guest-layout>
    <div class="rounded-xl p-8 shadow-sm" style="background-color: var(--color-bg-card); border: 1px solid var(--color-border-card);">
        <div class="text-center mb-6">
            <h1 class="text-2xl font-bold" style="color: var(--color-accent);">{{ __('messages.app.title') }}</h1>
            <p class="mt-2 text-sm" style="color: var(--color-text-secondary);">{{ __('messages.login.register_title') }}</p>
        </div>

        <form method="POST" action="{{ route('register') }}">
            @csrf
            <div class="mb-4">
                <label for="email" class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.login.email') }}</label>
                <input id="email" type="email" name="email" value="{{ old('email', request('email')) }}" required autofocus
                       class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                       style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                       placeholder="{{ __('messages.login.email_placeholder') }}">
                @error('email')
                    <p class="mt-1 text-sm" style="color: var(--color-danger);">{{ $message }}</p>
                @enderror
            </div>

            <div class="grid grid-cols-2 gap-4 mb-4">
                <div>
                    <label for="first_name" class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.login.firstname') }}</label>
                    <input id="first_name" type="text" name="first_name" value="{{ old('first_name') }}"
                           class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                           style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                           placeholder="{{ __('messages.login.firstname_placeholder') }}">
                </div>
                <div>
                    <label for="last_name" class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.login.lastname') }}</label>
                    <input id="last_name" type="text" name="last_name" value="{{ old('last_name') }}"
                           class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                           style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                           placeholder="{{ __('messages.login.lastname_placeholder') }}">
                </div>
            </div>

            <div class="mb-4">
                <label for="password" class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.login.password') }}</label>
                <input id="password" type="password" name="password" required
                       class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                       style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                       placeholder="{{ __('messages.login.password_placeholder') }}">
                @error('password')
                    <p class="mt-1 text-sm" style="color: var(--color-danger);">{{ $message }}</p>
                @enderror
            </div>

            <div class="mb-6">
                <label for="password_confirmation" class="block text-sm font-medium mb-1" style="color: var(--color-text-secondary);">{{ __('messages.login.password') }}</label>
                <input id="password_confirmation" type="password" name="password_confirmation" required
                       class="w-full rounded-lg px-3 py-2 text-sm border focus:outline-none focus:ring-2"
                       style="background-color: var(--color-bg-app); color: var(--color-text-primary); border-color: var(--color-border-subtle); --tw-ring-color: var(--color-accent);"
                       placeholder="{{ __('messages.login.password_placeholder') }}">
            </div>

            <button type="submit" class="w-full rounded-lg px-4 py-2.5 text-sm font-semibold text-white transition hover:opacity-90" style="background-color: var(--color-accent);">
                {{ __('messages.login.register') }}
            </button>
        </form>

        <p class="mt-4 text-center text-sm" style="color: var(--color-text-tertiary);">
            {{ __('messages.login.toggle_login') }}
            <a id="toggle-link" href="{{ route('login') }}" class="font-medium hover:underline" style="color: var(--color-accent);">{{ __('messages.login.submit') }}</a>
        </p>
    </div>

    <script>
        const emailInput = document.getElementById('email');
        const toggleLink = document.getElementById('toggle-link');
        const baseUrl = '{{ route('login') }}';
        emailInput.addEventListener('input', () => {
            toggleLink.href = emailInput.value ? baseUrl + '?email=' + encodeURIComponent(emailInput.value) : baseUrl;
        });
        if (emailInput.value) toggleLink.href = baseUrl + '?email=' + encodeURIComponent(emailInput.value);
    </script>
</x-guest-layout>
