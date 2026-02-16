<?php

namespace App\Http\Controllers;

use App\Models\Questionnaire;
use Barryvdh\DomPDF\Facade\Pdf;
use Illuminate\Http\Request;

class PdfController extends Controller
{
    public function generate(Questionnaire $questionnaire, Request $request)
    {
        if (!$questionnaire->published && $questionnaire->user_id !== $request->user()->id) {
            abort(403);
        }

        $questionnaire->load('questions.answers', 'theme');

        $pdf = Pdf::loadView('pdf.questionnaire', compact('questionnaire'));
        $prefix = __('messages.pdf.filename_prefix');
        $filename = "{$prefix}_{$questionnaire->name}_" . now()->format('Ymd') . '.pdf';

        return $pdf->download($filename);
    }
}
