<?php

namespace App\Http\Controllers;

use App\Models\UserPreference;
use Illuminate\Http\Request;

class UserPreferencesController extends Controller
{
    public function toggleTheme(Request $request)
    {
        $current = session('theme', 'Light');
        $newTheme = $current === 'Light' ? 'Dark' : 'Light';

        UserPreference::updateOrCreate(
            ['user_id' => $request->user()->id],
            ['theme' => $newTheme]
        );
        session(['theme' => $newTheme]);

        return back();
    }

    public function toggleLanguage(Request $request)
    {
        $current = session('locale', 'fr');
        $newLang = $current === 'fr' ? 'en' : 'fr';

        UserPreference::updateOrCreate(
            ['user_id' => $request->user()->id],
            ['language' => $newLang]
        );
        session(['locale' => $newLang]);
        app()->setLocale($newLang);

        return back();
    }
}
