<?php

namespace App\Models;

use App\Enums\AnswerType;
use Illuminate\Database\Eloquent\Model;

class Question extends Model
{
    protected $table = 'questions';

    public $timestamps = false;

    protected $fillable = [
        'questionnaire_id',
        'number',
        'label',
        'answer_type',
    ];

    protected function casts(): array
    {
        return [
            'answer_type' => AnswerType::class,
        ];
    }

    public function questionnaire()
    {
        return $this->belongsTo(Questionnaire::class);
    }

    public function answers()
    {
        return $this->hasMany(Answer::class)->orderBy('id');
    }
}
