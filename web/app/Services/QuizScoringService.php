<?php

namespace App\Services;

use App\Models\Questionnaire;

class QuizScoringService
{
    public function calculate(Questionnaire $questionnaire, array $selectedAnswers): array
    {
        $totalScore = 0;
        $maxPossibleScore = 0;

        foreach ($questionnaire->questions as $question) {
            $selected = $selectedAnswers[$question->id] ?? [];
            if (!is_array($selected)) {
                $selected = [$selected];
            }

            foreach ($question->answers as $answer) {
                if ($answer->weight > 0) {
                    $maxPossibleScore += (float) $answer->weight;
                }

                if (in_array($answer->id, $selected)) {
                    $totalScore += (float) $answer->weight;
                }
            }
        }

        $score = max(0, $totalScore);
        $percent = $maxPossibleScore > 0 ? round(($score / $maxPossibleScore) * 100) : 0;

        return [
            'score' => $score,
            'maxScore' => $maxPossibleScore,
            'percent' => $percent,
        ];
    }
}
