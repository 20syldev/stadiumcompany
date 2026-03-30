<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        if (!Schema::hasTable('question_feedbacks')) {
            Schema::create('question_feedbacks', function (Blueprint $table) {
                $table->id();
                $table->foreignId('user_id')->constrained('users')->cascadeOnDelete();
                $table->foreignId('question_id')->constrained('questions')->cascadeOnDelete();
                $table->unsignedTinyInteger('rating');
                $table->text('comment')->nullable();
                $table->timestamp('created_at')->useCurrent();

                $table->index('user_id', 'idx_feedback_user');
                $table->index('question_id', 'idx_feedback_question');
            });
        }
    }

    public function down(): void
    {
        Schema::dropIfExists('question_feedbacks');
    }
};
