<?php

namespace App\Http\Controllers;

use App\Http\Requests\StoreQuestionnaireRequest;
use App\Http\Requests\UpdateQuestionnaireRequest;
use App\Models\Questionnaire;
use App\Models\Theme;
use App\Services\ActivityLogService;
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
        $questionnaire = $this->questionnaireService->create($request->validated(), $request->user()->id);

        ActivityLogService::log($request->user()->id, 'questionnaire.create', 'questionnaire', $questionnaire->id, $questionnaire->name);

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

        ActivityLogService::log($request->user()->id, 'questionnaire.update', 'questionnaire', $questionnaire->id, $questionnaire->name);

        return redirect()->route('dashboard')->with('success', __('messages.editor.saved') . ' !');
    }

    public function destroy(Questionnaire $questionnaire)
    {
        $this->authorize('delete', $questionnaire);

        ActivityLogService::log(auth()->id(), 'questionnaire.delete', 'questionnaire', $questionnaire->id, $questionnaire->name);

        $questionnaire->delete();

        return redirect()->route('dashboard')->with('success', __('messages.main.action_delete') . ' !');
    }

    public function fork(Questionnaire $questionnaire, Request $request)
    {
        $this->authorize('fork', $questionnaire);

        $this->questionnaireService->fork($questionnaire, $request->user()->id);

        ActivityLogService::log($request->user()->id, 'questionnaire.fork', 'questionnaire', $questionnaire->id, $questionnaire->name);

        return redirect()->route('dashboard', ['tab' => 'mine'])
            ->with('success', __('messages.main.fork_success_message'));
    }

    public function togglePublish(Questionnaire $questionnaire)
    {
        $this->authorize('update', $questionnaire);

        $this->questionnaireService->togglePublish($questionnaire);

        $action = $questionnaire->published ? 'questionnaire.publish' : 'questionnaire.unpublish';
        ActivityLogService::log(auth()->id(), $action, 'questionnaire', $questionnaire->id, $questionnaire->name);

        return back();
    }
}
