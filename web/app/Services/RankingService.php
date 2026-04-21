<?php

namespace App\Services;

use App\Models\QuizSubmission;

class RankingService
{
    public function getRankForSubmission(QuizSubmission $submission): array
    {
        $questionnaireId = $submission->questionnaire_id;
        $score = $submission->score;

        // Number of distinct users who scored strictly higher
        $higherCount = QuizSubmission::where('questionnaire_id', $questionnaireId)
            ->where('score', '>', $score)
            ->distinct('user_id')
            ->count('user_id');

        $rank = $higherCount + 1;

        // Total distinct participants
        $total = QuizSubmission::where('questionnaire_id', $questionnaireId)
            ->distinct('user_id')
            ->count('user_id');

        return ['rank' => $rank, 'total' => $total];
    }
}
