<?php

namespace App\Http\Controllers;

use App\Enums\AnswerType;
use App\Models\Answer;
use App\Models\Question;
use App\Models\Questionnaire;
use App\Models\Theme;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\DB;

class QuestionnaireController extends Controller
{
    public function create()
    {
        $themes = Theme::orderBy('name')->get();
        return view('questionnaires.editor', [
            'questionnaire' => null,
            'themes' => $themes,
            'readonly' => false,
        ]);
    }

    public function store(Request $request)
    {
        $request->validate([
            'name' => 'required|string|max:50',
            'theme_id' => 'required|exists:themes,id',
            'published' => 'boolean',
            'questions' => 'nullable|array',
            'questions.*.label' => 'required|string|max:250',
            'questions.*.answer_type' => 'required|in:TRUE_FALSE,MULTIPLE_CHOICE',
            'questions.*.answers' => 'nullable|array',
            'questions.*.answers.*.label' => 'required|string|max:250',
            'questions.*.answers.*.is_correct' => 'nullable',
            'questions.*.answers.*.weight' => 'nullable|numeric',
        ]);

        DB::transaction(function () use ($request) {
            $questionnaire = Questionnaire::create([
                'name' => $request->name,
                'theme_id' => $request->theme_id,
                'user_id' => $request->user()->id,
                'published' => $request->boolean('published'),
                'question_count' => count($request->input('questions', [])),
            ]);

            $this->saveQuestions($questionnaire, $request->input('questions', []));
        });

        return redirect()->route('dashboard')->with('success', __('messages.editor.save') . ' !');
    }

    public function show(Questionnaire $questionnaire)
    {
        $questionnaire->load('questions.answers', 'theme', 'user');
        $themes = Theme::orderBy('name')->get();

        return view('questionnaires.editor', [
            'questionnaire' => $questionnaire,
            'themes' => $themes,
            'readonly' => true,
        ]);
    }

    public function edit(Questionnaire $questionnaire, Request $request)
    {
        $this->authorize('update', $questionnaire);

        $questionnaire->load('questions.answers', 'theme');
        $themes = Theme::orderBy('name')->get();

        return view('questionnaires.editor', [
            'questionnaire' => $questionnaire,
            'themes' => $themes,
            'readonly' => false,
        ]);
    }

    public function update(Request $request, Questionnaire $questionnaire)
    {
        $this->authorize('update', $questionnaire);

        $request->validate([
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
        ]);

        DB::transaction(function () use ($request, $questionnaire) {
            $questionnaire->update([
                'name' => $request->name,
                'theme_id' => $request->theme_id,
                'published' => $request->boolean('published'),
                'question_count' => count($request->input('questions', [])),
            ]);

            // Delete existing questions (cascade deletes answers)
            $questionnaire->questions()->delete();

            $this->saveQuestions($questionnaire, $request->input('questions', []));
        });

        return redirect()->route('dashboard')->with('success', __('messages.editor.save') . ' !');
    }

    public function destroy(Questionnaire $questionnaire, Request $request)
    {
        $this->authorize('delete', $questionnaire);
        $questionnaire->delete();

        return redirect()->route('dashboard')->with('success', __('messages.main.action_delete') . ' !');
    }

    public function fork(Questionnaire $questionnaire, Request $request)
    {
        $this->authorize('fork', $questionnaire);

        DB::transaction(function () use ($questionnaire, $request) {
            $suffix = __('messages.main.fork_suffix');

            $new = Questionnaire::create([
                'name' => "{$questionnaire->name} {$suffix}",
                'theme_id' => $questionnaire->theme_id,
                'user_id' => $request->user()->id,
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

        return redirect()->route('dashboard', ['tab' => 'mine'])
            ->with('success', __('messages.main.fork_success_message'));
    }

    public function togglePublish(Questionnaire $questionnaire, Request $request)
    {
        $this->authorize('update', $questionnaire);

        $questionnaire->update(['published' => !$questionnaire->published]);

        return back();
    }

    private function saveQuestions(Questionnaire $questionnaire, array $questionsData): void
    {
        foreach ($questionsData as $index => $qData) {
            $question = $questionnaire->questions()->create([
                'number' => $index + 1,
                'label' => $qData['label'],
                'answer_type' => $qData['answer_type'],
            ]);

            $answers = $qData['answers'] ?? [];
            foreach ($answers as $aData) {
                $question->answers()->create([
                    'label' => $aData['label'],
                    'is_correct' => isset($aData['is_correct']) && $aData['is_correct'],
                    'weight' => $aData['weight'] ?? 1,
                ]);
            }
        }
    }
}
