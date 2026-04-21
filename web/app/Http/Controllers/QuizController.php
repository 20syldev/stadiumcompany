<?php

namespace App\Http\Controllers;

use App\Models\QuestionFeedback;
use App\Models\Questionnaire;
use App\Models\QuizSubmission;
use App\Services\ActivityLogService;
use App\Services\QuizScoringService;
use App\Services\QuizService;
use App\Services\RankingService;
use Illuminate\Http\Request;

class QuizController extends Controller
{
    public function __construct(
        private QuizScoringService $scoringService,
        private QuizService $quizService,
        private RankingService $rankingService,
    ) {}

    public function play(Request $request, Questionnaire $questionnaire)
    {
        $this->authorize('play', $questionnaire);

        $isDemoMode = !$questionnaire->published;

        $questionnaire->load('questions.answers', 'theme');

        if ($questionnaire->questions->isEmpty()) {
            return back()->with('error', __('messages.main.quiz_impossible_message'));
        }

        $questionIds = $questionnaire->questions->pluck('id');
        $alreadySentFeedback = QuestionFeedback::where('user_id', $request->user()->id)
            ->whereIn('question_id', $questionIds)
            ->pluck('question_id')
            ->unique()
            ->values()
            ->toArray();

        return view('quiz.player', compact('questionnaire', 'isDemoMode', 'alreadySentFeedback'));
    }

    public function score(Questionnaire $questionnaire, Request $request)
    {
        $questionnaire->load('questions.answers');
        $selectedAnswers = $request->input('answers', []);

        $result = $this->scoringService->calculate($questionnaire, $selectedAnswers);

        $submission = $this->quizService->persistSubmission($questionnaire, $request->user()->id, $selectedAnswers, $result);

        ActivityLogService::log(
            $request->user()->id,
            'quiz.complete',
            'questionnaire',
            $questionnaire->id,
            "{$questionnaire->name} — {$result['score']}/{$result['maxScore']}"
        );

        $ranking = $this->rankingService->getRankForSubmission($submission);

        return response()->json([...$result, 'submissionId' => $submission->id, ...$ranking]);
    }

    public function review(QuizSubmission $submission)
    {
        abort_unless($submission->user_id === auth()->id(), 403);

        $submission->load([
            'questionnaire.theme',
            'questionnaire.questions.answers',
            'quizAnswers',
        ]);

        $questionnaire = $submission->questionnaire;
        $userAnswerIds = $submission->quizAnswers->pluck('answer_id')->toArray();

        $questionsData = $questionnaire->questions->sortBy('number')->map(function ($question) use ($userAnswerIds) {
            $correctAnswers = $question->answers->where('is_correct', true)->pluck('label')->toArray();

            return [
                'number' => $question->number,
                'label' => $question->label,
                'answer_type' => $question->answer_type,
                'answers' => $question->answers->map(fn ($a) => [
                    'label' => $a->label,
                    'is_correct' => $a->is_correct,
                    'was_selected' => in_array($a->id, $userAnswerIds),
                ])->toArray(),
                'correct_labels' => $correctAnswers,
                'all_labels' => $question->answers->pluck('label')->toArray(),
            ];
        });

        $ranking = $this->rankingService->getRankForSubmission($submission);

        return view('quiz.review', compact('submission', 'questionnaire', 'questionsData', 'ranking'));
    }
}
