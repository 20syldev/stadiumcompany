<?php

use App\Http\Controllers\Admin\AdminLogController;
use App\Http\Controllers\Admin\AdminRankingController;
use App\Http\Controllers\DashboardController;
use App\Http\Controllers\QuestionnaireController;
use App\Http\Controllers\QuizController;
use App\Http\Controllers\PdfController;
use App\Http\Controllers\UserPreferencesController;
use App\Http\Controllers\ThemeController;
use App\Http\Controllers\QuestionFeedbackController;
use Illuminate\Support\Facades\Route;

Route::get('/', function () {
    return redirect()->route('dashboard');
});

Route::middleware(['auth'])->group(function () {
    // Dashboard
    Route::get('/dashboard', [DashboardController::class, 'index'])->name('dashboard');

    // Questionnaire CRUD — export must come before the resource (avoids {questionnaire} match)
    Route::get('questionnaires/export/csv', [QuestionnaireController::class, 'exportCsv'])->name('questionnaires.export.csv');
    Route::resource('questionnaires', QuestionnaireController::class)->except(['index']);

    // Fork & Publish
    Route::post('questionnaires/{questionnaire}/fork', [QuestionnaireController::class, 'fork'])->name('questionnaires.fork');
    Route::patch('questionnaires/{questionnaire}/publish', [QuestionnaireController::class, 'togglePublish'])->name('questionnaires.publish');

    // Quiz
    Route::get('quiz/{questionnaire}', [QuizController::class, 'play'])->name('quiz.play');
    Route::post('quiz/{questionnaire}/score', [QuizController::class, 'score'])->name('quiz.score');
    Route::get('quiz/review/{submission}', [QuizController::class, 'review'])->name('quiz.review');

    // PDF
    Route::get('questionnaires/{questionnaire}/pdf', [PdfController::class, 'generate'])->name('questionnaires.pdf');

    // Preferences
    Route::post('preferences/toggle-theme', [UserPreferencesController::class, 'toggleTheme'])->name('preferences.toggle-theme');
    Route::post('preferences/toggle-language', [UserPreferencesController::class, 'toggleLanguage'])->name('preferences.toggle-language');

    // Feedback
    Route::post('questions/{question}/feedback', [QuestionFeedbackController::class, 'store'])->name('feedback.store');

    // Themes
    Route::post('themes', [ThemeController::class, 'store'])->name('themes.store');

    // Admin
    Route::middleware(['admin'])->prefix('admin')->name('admin.')->group(function () {
        Route::get('logs', [AdminLogController::class, 'index'])->name('logs.index');
        Route::get('rankings', [AdminRankingController::class, 'index'])->name('rankings.index');
        Route::get('rankings/api/questionnaires', [AdminRankingController::class, 'apiQuestionnaires'])->name('rankings.api.questionnaires');
        Route::get('rankings/api/{questionnaire}/leaderboard', [AdminRankingController::class, 'apiLeaderboard'])->name('rankings.api.leaderboard');
    });
});

require __DIR__.'/auth.php';
