<?php

namespace App\Http\Controllers;

use App\Http\Requests\StoreQuestionnaireRequest;
use App\Http\Requests\UpdateQuestionnaireRequest;
use App\Models\Questionnaire;
use App\Models\Theme;
use App\Services\QuestionnaireService;
use Illuminate\Http\Request;

class QuestionnaireController extends Controller
{
    public function __construct(private QuestionnaireService $questionnaireService) {}

    public function create()
    {
        $themes = Theme::orderBy('name')->get();
        return view('questionnaires.editor', [
            'questionnaire' => null,
            'themes' => $themes,
            'readonly' => false,
        ]);
    }

    public function store(StoreQuestionnaireRequest $request)
    {
        $this->questionnaireService->create($request->validated(), $request->user()->id);

        return redirect()->route('dashboard')->with('success', __('messages.editor.saved') . ' !');
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

    public function edit(Questionnaire $questionnaire)
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

    public function update(UpdateQuestionnaireRequest $request, Questionnaire $questionnaire)
    {
        $this->authorize('update', $questionnaire);

        $this->questionnaireService->update($questionnaire, $request->validated());

        return redirect()->route('dashboard')->with('success', __('messages.editor.saved') . ' !');
    }

    public function destroy(Questionnaire $questionnaire)
    {
        $this->authorize('delete', $questionnaire);
        $questionnaire->delete();

        return redirect()->route('dashboard')->with('success', __('messages.main.action_delete') . ' !');
    }

    public function fork(Questionnaire $questionnaire, Request $request)
    {
        $this->authorize('fork', $questionnaire);

        $this->questionnaireService->fork($questionnaire, $request->user()->id);

        return redirect()->route('dashboard', ['tab' => 'mine'])
            ->with('success', __('messages.main.fork_success_message'));
    }

    public function togglePublish(Questionnaire $questionnaire)
    {
        $this->authorize('update', $questionnaire);

        $this->questionnaireService->togglePublish($questionnaire);

        return back();
    }
}
