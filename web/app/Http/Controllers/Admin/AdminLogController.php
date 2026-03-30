<?php

namespace App\Http\Controllers\Admin;

use App\Http\Controllers\Controller;
use App\Models\ActivityLog;
use Illuminate\Http\Request;
use Illuminate\Support\Carbon;

class AdminLogController extends Controller
{
    public function index(Request $request)
    {
        $query = ActivityLog::with('user');

        if ($search = $request->query('search')) {
            $query->where(function ($q) use ($search) {
                $q->where('details', 'ilike', "%{$search}%")
                  ->orWhere('action', 'ilike', "%{$search}%")
                  ->orWhereHas('user', function ($uq) use ($search) {
                      $uq->where('email', 'ilike', "%{$search}%")
                          ->orWhere('first_name', 'ilike', "%{$search}%")
                          ->orWhere('last_name', 'ilike', "%{$search}%");
                  });
            });
        }

        if ($action = $request->query('action')) {
            $query->where('action', $action);
        }

        if ($from = $request->query('from')) {
            $query->where('created_at', '>=', $from);
        }

        if ($to = $request->query('to')) {
            $query->where('created_at', '<=', Carbon::parse($to)->endOfDay());
        }

        $logs = $query->orderByDesc('created_at')->paginate(25)->withQueryString();

        $actions = ActivityLog::select('action')->distinct()->orderBy('action')->pluck('action');

        return view('admin.logs.index', compact('logs', 'actions'));
    }
}
