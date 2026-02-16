<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Symfony\Component\HttpFoundation\Response;

class ApplyUserPreferences
{
    public function handle(Request $request, Closure $next): Response
    {
        $locale = session('locale', config('app.locale'));
        app()->setLocale($locale);

        view()->share('currentTheme', session('theme', 'Light'));
        view()->share('currentLocale', $locale);

        return $next($request);
    }
}
