<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration
{
    public function up(): void
    {
        // Create the answer_type enum if it doesn't exist
        DB::statement("DO \$\$ BEGIN
            CREATE TYPE answer_type AS ENUM ('TRUE_FALSE', 'MULTIPLE_CHOICE');
        EXCEPTION
            WHEN duplicate_object THEN null;
        END \$\$;");

        if (!Schema::hasTable('users')) {
            Schema::create('users', function (Blueprint $table) {
                $table->id();
                $table->string('email', 100)->unique();
                $table->string('password', 60);
                $table->string('last_name', 50)->nullable();
                $table->string('first_name', 50)->nullable();
                $table->timestamp('created_at')->useCurrent();
            });
        }

        if (!Schema::hasTable('themes')) {
            Schema::create('themes', function (Blueprint $table) {
                $table->id();
                $table->string('name', 50)->unique();
            });
        }

        if (!Schema::hasTable('questionnaires')) {
            Schema::create('questionnaires', function (Blueprint $table) {
                $table->id();
                $table->string('name', 50);
                $table->foreignId('theme_id')->constrained('themes');
                $table->foreignId('user_id')->constrained('users');
                $table->integer('question_count')->default(0);
                $table->boolean('published')->default(false);
            });

            Schema::table('questionnaires', function (Blueprint $table) {
                $table->index('theme_id', 'idx_questionnaire_theme');
                $table->index('user_id', 'idx_questionnaire_user');
                $table->index('published', 'idx_questionnaire_published');
            });
        }

        if (!Schema::hasTable('questions')) {
            Schema::create('questions', function (Blueprint $table) {
                $table->id();
                $table->foreignId('questionnaire_id')->constrained()->cascadeOnDelete();
                $table->integer('number');
                $table->string('label', 250);
            });

            DB::statement("ALTER TABLE questions ADD COLUMN answer_type answer_type NOT NULL DEFAULT 'TRUE_FALSE'");

            Schema::table('questions', function (Blueprint $table) {
                $table->index('questionnaire_id', 'idx_question_questionnaire');
            });
        }

        if (!Schema::hasTable('answers')) {
            Schema::create('answers', function (Blueprint $table) {
                $table->id();
                $table->foreignId('question_id')->constrained()->cascadeOnDelete();
                $table->string('label', 250);
                $table->boolean('is_correct')->default(false);
                $table->decimal('weight', 10, 2)->default(1);
            });

            Schema::table('answers', function (Blueprint $table) {
                $table->index('question_id', 'idx_answer_question');
            });
        }

        if (!Schema::hasTable('user_preferences')) {
            Schema::create('user_preferences', function (Blueprint $table) {
                $table->unsignedBigInteger('user_id')->primary();
                $table->foreign('user_id')->references('id')->on('users')->cascadeOnDelete();
                $table->string('theme', 10)->default('Light');
                $table->string('language', 5)->default('fr');
            });
        }

        // Laravel infrastructure tables
        if (!Schema::hasTable('sessions')) {
            Schema::create('sessions', function (Blueprint $table) {
                $table->string('id')->primary();
                $table->foreignId('user_id')->nullable()->index();
                $table->string('ip_address', 45)->nullable();
                $table->text('user_agent')->nullable();
                $table->longText('payload');
                $table->integer('last_activity')->index();
            });
        }

        if (!Schema::hasTable('password_reset_tokens')) {
            Schema::create('password_reset_tokens', function (Blueprint $table) {
                $table->string('email')->primary();
                $table->string('token');
                $table->timestamp('created_at')->nullable();
            });
        }

        if (!Schema::hasTable('cache')) {
            Schema::create('cache', function (Blueprint $table) {
                $table->string('key')->primary();
                $table->mediumText('value');
                $table->integer('expiration');
            });

            Schema::create('cache_locks', function (Blueprint $table) {
                $table->string('key')->primary();
                $table->string('owner');
                $table->integer('expiration');
            });
        }
    }

    public function down(): void
    {
        // Intentionally empty - never drop shared tables
    }
};
