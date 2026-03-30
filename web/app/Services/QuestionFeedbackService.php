<?php

namespace App\Services;

use App\Models\Question;
use App\Models\QuestionFeedback;

class QuestionFeedbackService
{
    public function store(Question $question, int $userId, array $data): QuestionFeedback
    {
        return QuestionFeedback::create([
            'user_id' => $userId,
            'question_id' => $question->id,
            'rating' => $data['rating'],
            'comment' => $data['comment'] ?? null,
        ]);
    }
}
