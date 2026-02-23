<?php

namespace App\Services;

use App\Models\Questionnaire;
use App\Models\QuizSubmission;
use Illuminate\Support\Facades\DB;

class QuizService
{
    public function persistSubmission(
        Questionnaire $questionnaire,
        int $userId,
        array $selectedAnswers,
        array $result
    ): QuizSubmission {
        return DB::transaction(function () use ($questionnaire, $userId, $selectedAnswers, $result) {
            $submission = QuizSubmission::create([
                'user_id' => $userId,
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

            return $submission;
        });
    }
}
