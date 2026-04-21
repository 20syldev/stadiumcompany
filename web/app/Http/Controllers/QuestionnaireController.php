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

    public function exportCsv(Request $request)
    {
        $user = $request->user();
        $questionnaires = Questionnaire::where('user_id', $user->id)
            ->with(['theme', 'user'])
            ->withCount('questions')
            ->withCount('submissions')
            ->orderBy('name')
            ->get();

        ob_start();
        $handle = fopen('php://output', 'w');
        fwrite($handle, "\xEF\xBB\xBF"); // UTF-8 BOM for Excel
        fputcsv($handle, ['ID', 'Name', 'Theme', 'Questions', 'Published', 'Submissions', 'Created']);
        foreach ($questionnaires as $q) {
            fputcsv($handle, [
                $q->id,
                $q->name,
                $q->theme?->name ?? '',
                $q->questions_count,
                $q->published ? 'Yes' : 'No',
                $q->submissions_count,
                $q->created_at?->format('Y-m-d') ?? '',
            ]);
        }
        fclose($handle);
        $csv = ob_get_clean();

        return response($csv, 200, [
            'Content-Type' => 'text/csv; charset=UTF-8',
            'Content-Disposition' => 'attachment; filename="questionnaires.csv"',
        ]);
    }
}
