<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    public function up(): void
    {
        if (!Schema::hasColumn('users', 'is_admin')) {
            Schema::table('users', function (Blueprint $table) {
                $table->boolean('is_admin')->default(false)->after('first_name');
            });
        }

        if (!Schema::hasTable('activity_logs')) {
            Schema::create('activity_logs', function (Blueprint $table) {
                $table->id();
                $table->foreignId('user_id')->nullable()->constrained('users')->nullOnDelete();
                $table->string('action', 100);
                $table->string('entity_type', 50)->nullable();
                $table->integer('entity_id')->nullable();
                $table->text('details')->nullable();
                $table->string('source', 10)->default('desktop');
                $table->timestamp('created_at')->useCurrent();

                $table->index('user_id', 'idx_activity_logs_user');
                $table->index('action', 'idx_activity_logs_action');
                $table->index('created_at', 'idx_activity_logs_created_at');
            });
        }
}

    public function down(): void
    {
        Schema::dropIfExists('activity_logs');

        if (Schema::hasColumn('users', 'is_admin')) {
            Schema::table('users', function (Blueprint $table) {
                $table->dropColumn('is_admin');
            });
        }
    }
};
