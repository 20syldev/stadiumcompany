<?php

namespace App\Http\Controllers;

use App\Models\Questionnaire;
use App\Services\QuizScoringService;
use App\Services\QuizService;
use Illuminate\Http\Request;

class QuizController extends Controller
{
    public function __construct(
        private QuizScoringService $scoringService,
        private QuizService $quizService,
    ) {}

    public function play(Questionnaire $questionnaire)
    {
        $this->authorize('play', $questionnaire);

        $isDemoMode = !$questionnaire->published;

        $questionnaire->load('questions.answers', 'theme');

        if ($questionnaire->questions->isEmpty()) {
            return back()->with('error', __('messages.main.quiz_impossible_message'));
        }

        return view('quiz.player', compact('questionnaire', 'isDemoMode'));
    }

    public function score(Questionnaire $questionnaire, Request $request)
    {
        $questionnaire->load('questions.answers');
        $selectedAnswers = $request->input('answers', []);

        $result = $this->scoringService->calculate($questionnaire, $selectedAnswers);

        $this->quizService->persistSubmission($questionnaire, $request->user()->id, $selectedAnswers, $result);

        return response()->json($result);
    }
}
