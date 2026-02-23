<?php

namespace App\Http\Controllers;

use App\Http\Requests\StoreThemeRequest;
use App\Models\Theme;

class ThemeController extends Controller
{
    public function store(StoreThemeRequest $request)
    {
        $theme = Theme::create($request->validated());

        if ($request->wantsJson()) {
            return response()->json($theme);
        }

        return back();
    }
}
