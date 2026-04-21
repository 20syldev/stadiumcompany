<?php

namespace App\Http\Controllers;

use App\Models\Questionnaire;
use App\Models\QuizSubmission;
use App\Models\Theme;
use Illuminate\Http\Request;

class DashboardController extends Controller
{
    public function index(Request $request)
    {
        $user = $request->user();
        $tab = $request->query('tab', 'published');
        $search = trim($request->query('search', ''));
        $themeId = $request->query('theme_id');
        $sort = $request->query('sort', 'date');
        $viewMode = $request->query('view', 'grid');

        $themes = Theme::orderBy('name')->get();

        if ($tab === 'results') {
            $submissions = QuizSubmission::where('user_id', $user->id)
                ->with('questionnaire.theme')
                ->orderByDesc('created_at')
                ->paginate(12)
                ->withQueryString();

            if ($request->ajax()) {
                return view('dashboard._cards', compact('tab', 'submissions', 'viewMode'));
            }

            return view('dashboard', compact('tab', 'themes', 'submissions', 'viewMode', 'search', 'themeId', 'sort'));
        }

        $query = $tab === 'mine'
            ? Questionnaire::where('user_id', $user->id)->with('theme')->withCount('questions')
            : Questionnaire::where('published', true)->where('user_id', '!=', $user->id)->with(['theme', 'user'])->withCount('questions');

        $query->withCount('submissions')
            ->addSelect([
                'avg_score_percent' => QuizSubmission::selectRaw('ROUND(AVG(score / NULLIF(max_score, 0) * 100))')
                    ->whereColumn('questionnaire_id', 'questionnaires.id'),
            ]);

        if ($search !== '') {
            $query->where('name', 'ilike', "%{$search}%");
        }

        if ($themeId) {
            $query->where('theme_id', $themeId);
        }

        match ($sort) {
            'popularity' => $query->orderByDesc('submissions_count'),
            'name'       => $query->orderBy('name'),
            default      => $query->orderByDesc('id'),
        };

        $items = $query->paginate(12)->withQueryString();

        if ($request->ajax()) {
            return view('dashboard._cards', compact('tab', 'items', 'viewMode'));
        }

        // Still load all three for the full page (non-AJAX)
        $myQuestionnaires = null;
        $publishedQuestionnaires = null;
        $submissions = null;

        return view('dashboard', compact(
            'tab', 'themes', 'items',
            'viewMode', 'search', 'themeId', 'sort',
            'myQuestionnaires', 'publishedQuestionnaires', 'submissions'
        ));
    }
}
