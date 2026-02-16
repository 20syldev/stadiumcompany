<?php

namespace App\Http\Controllers;

use App\Models\Questionnaire;
use Illuminate\Http\Request;

class DashboardController extends Controller
{
    public function index(Request $request)
    {
        $user = $request->user();
        $tab = $request->query('tab', 'mine');

        $myQuestionnaires = Questionnaire::where('user_id', $user->id)
            ->with('theme')
            ->withCount('questions')
            ->orderByDesc('id')
            ->get();

        $publishedQuestionnaires = Questionnaire::where('published', true)
            ->where('user_id', '!=', $user->id)
            ->with(['theme', 'user'])
            ->withCount('questions')
            ->orderByDesc('id')
            ->get();

        return view('dashboard', compact('myQuestionnaires', 'publishedQuestionnaires', 'tab'));
    }
}
