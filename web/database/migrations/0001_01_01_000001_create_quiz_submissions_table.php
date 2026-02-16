<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        if (!Schema::hasTable('quiz_submissions')) {
            Schema::create('quiz_submissions', function (Blueprint $table) {
                $table->id();
                $table->foreignId('user_id')->constrained('users')->cascadeOnDelete();
                $table->foreignId('questionnaire_id')->constrained('questionnaires')->cascadeOnDelete();
                $table->decimal('score', 10, 2)->default(0);
                $table->decimal('max_score', 10, 2)->default(0);
                $table->timestamp('created_at')->useCurrent();

                $table->index('user_id', 'idx_quiz_submission_user');
                $table->index('questionnaire_id', 'idx_quiz_submission_questionnaire');
            });
        }

        if (!Schema::hasTable('quiz_answers')) {
            Schema::create('quiz_answers', function (Blueprint $table) {
                $table->id();
                $table->foreignId('quiz_submission_id')->constrained('quiz_submissions')->cascadeOnDelete();
                $table->foreignId('question_id')->constrained('questions')->cascadeOnDelete();
                $table->foreignId('answer_id')->constrained('answers')->cascadeOnDelete();

                $table->index('quiz_submission_id', 'idx_quiz_answer_submission');
            });
        }
    }

    public function down(): void
    {
        Schema::dropIfExists('quiz_answers');
        Schema::dropIfExists('quiz_submissions');
    }
};
