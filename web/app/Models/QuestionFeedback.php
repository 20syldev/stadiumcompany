<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class QuestionFeedback extends Model
{
    protected $table = 'question_feedbacks';

    const UPDATED_AT = null;

    protected $fillable = [
        'user_id',
        'question_id',
        'rating',
        'comment',
    ];

    protected function casts(): array
    {
        return [
            'rating' => 'integer',
            'created_at' => 'datetime',
        ];
    }

    public function user()
    {
        return $this->belongsTo(User::class);
    }

    public function question()
    {
        return $this->belongsTo(Question::class);
    }
}
