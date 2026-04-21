<?php

namespace App\Http\Controllers;

use App\Models\Questionnaire;
use Barryvdh\DomPDF\Facade\Pdf;

class PdfController extends Controller
{
    public function generate(Questionnaire $questionnaire)
    {
        $this->authorize('generatePdf', $questionnaire);

        $questionnaire->load('questions.answers', 'theme');

        $pdf = Pdf::loadView('pdf.questionnaire', compact('questionnaire'));
        $prefix = __('messages.pdf.filename_prefix');
        $slug = \Str::slug($questionnaire->name);
        $filename = "{$prefix}_{$slug}_" . now()->format('Ymd') . '.pdf';

        return $pdf->download($filename);
    }
}
