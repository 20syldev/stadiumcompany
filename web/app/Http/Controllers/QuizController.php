<?php

namespace App\Http\Controllers;

use App\Models\Questionnaire;
use App\Models\QuizSubmission;
use App\Services\QuizScoringService;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;

class QuizController extends Controller
{
    public function play(Questionnaire $questionnaire, Request $request)
    {
        $isDemoMode = false;

        if (!$questionnaire->published) {
            if ($questionnaire->user_id !== $request->user()->id) {
                abort(403);
            }
            $isDemoMode = true;
        }

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

        $scorer = new QuizScoringService();
        $result = $scorer->calculate($questionnaire, $selectedAnswers);

        // Persist the submission and answers
        DB::transaction(function () use ($questionnaire, $request, $selectedAnswers, $result) {
            $submission = QuizSubmission::create([
                'user_id' => $request->user()->id,
                'questionnaire_id' => $questionnaire->id,
                'score' => $result['score'],
                'max_score' => $result['maxScore'],
            ]);

            foreach ($selectedAnswers as $questionId => $answerIds) {
                if (!is_array($answerIds)) {
                    $answerIds = [$answerIds];
                }

                foreach ($answerIds as $answerId) {
                    $submission->quizAnswers()->create([
                        'question_id' => $questionId,
                        'answer_id' => $answerId,
                    ]);
                }
            }
        });

        return response()->json($result);
    }
}
