<?php

namespace App\Policies;

use App\Models\Questionnaire;
use App\Models\User;

class QuestionnairePolicy
{
    public function update(User $user, Questionnaire $questionnaire): bool
    {
        return $questionnaire->user_id === $user->id;
    }

    public function delete(User $user, Questionnaire $questionnaire): bool
    {
        return $questionnaire->user_id === $user->id;
    }

    public function fork(User $user, Questionnaire $questionnaire): bool
    {
        return $questionnaire->published && $questionnaire->user_id !== $user->id;
    }

    public function play(User $user, Questionnaire $questionnaire): bool
    {
        return $questionnaire->published || $questionnaire->user_id === $user->id;
    }

    public function generatePdf(User $user, Questionnaire $questionnaire): bool
    {
        return $questionnaire->published || $questionnaire->user_id === $user->id;
    }
}
