<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class QuizAnswer extends Model
{
    protected $table = 'quiz_answers';

    public $timestamps = false;

    protected $fillable = [
        'quiz_submission_id',
        'question_id',
        'answer_id',
    ];

    public function quizSubmission()
    {
        return $this->belongsTo(QuizSubmission::class);
    }

    public function question()
    {
        return $this->belongsTo(Question::class);
    }

    public function answer()
    {
        return $this->belongsTo(Answer::class);
    }
}
