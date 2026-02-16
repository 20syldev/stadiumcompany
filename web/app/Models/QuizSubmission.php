<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class QuizSubmission extends Model
{
    protected $table = 'quiz_submissions';

    const UPDATED_AT = null;

    protected $fillable = [
        'user_id',
        'questionnaire_id',
        'score',
        'max_score',
    ];

    protected function casts(): array
    {
        return [
            'score' => 'decimal:2',
            'max_score' => 'decimal:2',
            'created_at' => 'datetime',
        ];
    }

    public function user()
    {
        return $this->belongsTo(User::class);
    }

    public function questionnaire()
    {
        return $this->belongsTo(Questionnaire::class);
    }

    public function quizAnswers()
    {
        return $this->hasMany(QuizAnswer::class);
    }
}
