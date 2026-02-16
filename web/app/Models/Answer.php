<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Answer extends Model
{
    protected $table = 'answers';

    public $timestamps = false;

    protected $fillable = [
        'question_id',
        'label',
        'is_correct',
        'weight',
    ];

    protected function casts(): array
    {
        return [
            'is_correct' => 'boolean',
            'weight' => 'decimal:2',
        ];
    }

    public function question()
    {
        return $this->belongsTo(Question::class);
    }
}
