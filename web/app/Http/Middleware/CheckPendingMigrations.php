<?php

namespace App\Http\Middleware;

use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Log;
use Symfony\Component\HttpFoundation\Response;

class CheckPendingMigrations
{
    public function handle(Request $request, Closure $next): Response
    {
        $pending = Cache::remember('pending_migrations', 30, function () {
            try {
                $migrator = app('migrator');
                $migrator->setConnection(config('database.default'));
                $files = $migrator->getMigrationFiles($migrator->paths());
                $files = array_merge($files, $migrator->getMigrationFiles(database_path('migrations')));
                $ran = $migrator->getRepository()->getRan();

                return array_values(array_diff(array_keys($files), $ran));
            } catch (\Throwable) {
                return [];
            }
        });

        if (!empty($pending)) {
            $count = count($pending);

            Log::warning("[migrations] {$count} pending migration(s) — run: php artisan migrate");

            if (app()->environment('local')) {
                view()->share('pendingMigrations', $count);
            }
        }

        return $next($request);
    }
}
