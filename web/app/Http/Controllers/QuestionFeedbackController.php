<?php

namespace App\Http\Controllers;

use App\Http\Requests\StoreQuestionFeedbackRequest;
use App\Models\Question;
use App\Models\QuestionFeedback;
use App\Services\QuestionFeedbackService;
use Illuminate\Support\Facades\Gate;

class QuestionFeedbackController extends Controller
{
    public function __construct(private QuestionFeedbackService $feedbackService) {}

    public function store(StoreQuestionFeedbackRequest $request, Question $question)
    {
        Gate::authorize('create', [QuestionFeedback::class, $question]);

        $feedback = $this->feedbackService->store(
            $question,
            $request->user()->id,
            $request->validated()
        );

        return response()->json($feedback);
    }
}
