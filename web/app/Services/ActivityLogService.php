<?php

namespace App\Services;

use Illuminate\Support\Facades\DB;

class ActivityLogService
{
    public static function log(int $userId, string $action, ?string $entityType = null, ?int $entityId = null, ?string $details = null): void
    {
        try {
            DB::table('activity_logs')->insert([
                'user_id' => $userId,
                'action' => $action,
                'entity_type' => $entityType,
                'entity_id' => $entityId,
                'details' => $details,
                'source' => 'web',
                'created_at' => now(),
            ]);
        } catch (\Throwable) {
            // Never crash the app for logging failures
        }
    }
}
