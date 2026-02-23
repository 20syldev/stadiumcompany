<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

class UpdateQuestionnaireRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }

    public function rules(): array
    {
        return [
            'name' => 'required|string|max:50',
            'theme_id' => 'required|exists:themes,id',
            'published' => 'boolean',
            'questions' => 'nullable|array',
            'questions.*.id' => 'nullable|integer',
            'questions.*.label' => 'required|string|max:250',
            'questions.*.answer_type' => 'required|in:TRUE_FALSE,MULTIPLE_CHOICE',
            'questions.*.answers' => 'nullable|array',
            'questions.*.answers.*.id' => 'nullable|integer',
            'questions.*.answers.*.label' => 'required|string|max:250',
            'questions.*.answers.*.is_correct' => 'nullable',
            'questions.*.answers.*.weight' => 'nullable|numeric',
        ];
    }
}
