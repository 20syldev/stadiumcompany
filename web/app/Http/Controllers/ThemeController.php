<?php

namespace App\Http\Controllers;

use App\Models\Theme;
use Illuminate\Http\Request;

class ThemeController extends Controller
{
    public function store(Request $request)
    {
        $request->validate(['name' => 'required|string|max:50|unique:themes']);
        $theme = Theme::create(['name' => $request->name]);

        if ($request->wantsJson()) {
            return response()->json($theme);
        }

        return back();
    }
}
