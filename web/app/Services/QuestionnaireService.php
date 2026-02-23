<?php

namespace App\Services;

use App\Models\Questionnaire;
use Illuminate\Support\Facades\DB;

class QuestionnaireService
{
    public function create(array $data, int $userId): Questionnaire
    {
        return DB::transaction(function () use ($data, $userId) {
            $questionnaire = Questionnaire::create([
                'name' => $data['name'],
                'theme_id' => $data['theme_id'],
                'user_id' => $userId,
                'published' => $data['published'] ?? false,
                'question_count' => count($data['questions'] ?? []),
            ]);

            $this->saveQuestions($questionnaire, $data['questions'] ?? []);

            return $questionnaire;
        });
    }

    public function update(Questionnaire $questionnaire, array $data): void
    {
        DB::transaction(function () use ($questionnaire, $data) {
            $questionnaire->update([
                'name' => $data['name'],
                'theme_id' => $data['theme_id'],
                'published' => $data['published'] ?? false,
                'question_count' => count($data['questions'] ?? []),
            ]);

            $questionnaire->questions()->delete();

            $this->saveQuestions($questionnaire, $data['questions'] ?? []);
        });
    }

    public function fork(Questionnaire $questionnaire, int $userId): void
    {
        DB::transaction(function () use ($questionnaire, $userId) {
            $suffix = __('messages.main.fork_suffix');

            $new = Questionnaire::create([
                'name' => "{$questionnaire->name} {$suffix}",
                'theme_id' => $questionnaire->theme_id,
                'user_id' => $userId,
                'published' => false,
                'question_count' => $questionnaire->question_count,
            ]);

            foreach ($questionnaire->questions()->with('answers')->get() as $question) {
                $newQuestion = $new->questions()->create([
                    'number' => $question->number,
                    'label' => $question->label,
                    'answer_type' => $question->answer_type->value,
                ]);

                foreach ($question->answers as $answer) {
                    $newQuestion->answers()->create([
                        'label' => $answer->label,
                        'is_correct' => $answer->is_correct,
                        'weight' => $answer->weight,
                    ]);
                }
            }
        });
    }

    public function togglePublish(Questionnaire $questionnaire): void
    {
        $questionnaire->update(['published' => !$questionnaire->published]);
    }

    private function saveQuestions(Questionnaire $questionnaire, array $questionsData): void
    {
        foreach ($questionsData as $index => $qData) {
            $question = $questionnaire->questions()->create([
                'number' => $index + 1,
                'label' => $qData['label'],
                'answer_type' => $qData['answer_type'],
            ]);

            foreach ($qData['answers'] ?? [] as $aData) {
                $question->answers()->create([
                    'label' => $aData['label'],
                    'is_correct' => isset($aData['is_correct']) && $aData['is_correct'],
                    'weight' => $aData['weight'] ?? 1,
                ]);
            }
        }
    }
}
