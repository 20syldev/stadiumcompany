<?php

namespace App\Policies;

use App\Models\Question;
use App\Models\User;

class QuestionFeedbackPolicy
{
    public function create(User $user, Question $question): bool
    {
        return $question->questionnaire->published || $question->questionnaire->user_id === $user->id;
    }
}
