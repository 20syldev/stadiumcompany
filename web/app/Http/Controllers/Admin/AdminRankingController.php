<?php

namespace App\Http\Controllers\Admin;

use App\Http\Controllers\Controller;
use App\Models\Questionnaire;
use App\Models\QuizSubmission;
use Illuminate\Http\Request;

class AdminRankingController extends Controller
{
    public function index()
    {
        return view('admin.rankings.index');
    }

    public function apiQuestionnaires(Request $request)
    {
        $search = trim($request->query('search', ''));

        $query = Questionnaire::where('published', true)
            ->with('theme')
            ->withCount('submissions')
            ->addSelect([
                'avg_score_percent' => QuizSubmission::selectRaw(
                    'ROUND(AVG(score / NULLIF(max_score, 0) * 100))'
                )->whereColumn('questionnaire_id', 'questionnaires.id'),
            ]);

        if ($search !== '') {
            $query->where('name', 'ilike', "%{$search}%");
        }

        $questionnaires = $query->orderByDesc('submissions_count')->paginate(15);

        return response()->json([
            'data' => $questionnaires->map(fn($q) => [
                'id' => $q->id,
                'name' => $q->name,
                'theme' => $q->theme?->name,
                'submissions_count' => $q->submissions_count,
                'avg_score_percent' => $q->avg_score_percent,
            ]),
            'current_page' => $questionnaires->currentPage(),
            'last_page' => $questionnaires->lastPage(),
        ]);
    }

    public function apiLeaderboard(Request $request, Questionnaire $questionnaire)
    {
        $submissions = QuizSubmission::where('questionnaire_id', $questionnaire->id)
            ->with('user')
            ->orderByDesc('score')
            ->orderBy('created_at')
            ->paginate(20);

        $rank = 0;
        $prevScore = null;
        $data = $submissions->map(function ($sub) use (&$rank, &$prevScore) {
            if ($sub->score != $prevScore) {
                $rank++;
                $prevScore = $sub->score;
            }
            $maxScore = $sub->max_score > 0 ? $sub->max_score : 1;
            return [
                'rank' => $rank,
                'user_name' => $sub->user ? ($sub->user->full_name ?? $sub->user->email) : '?',
                'user_email' => $sub->user?->email,
                'score' => $sub->score,
                'max_score' => $sub->max_score,
                'percent' => round($sub->score / $maxScore * 100),
                'date' => $sub->created_at->format('d/m/Y H:i'),
            ];
        });

        return response()->json([
            'questionnaire' => [
                'id' => $questionnaire->id,
                'name' => $questionnaire->name,
                'theme' => $questionnaire->theme?->name,
            ],
            'data' => $data,
            'current_page' => $submissions->currentPage(),
            'last_page' => $submissions->lastPage(),
        ]);
    }
}
