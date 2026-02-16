<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;

class User extends Authenticatable
{
    use HasFactory, Notifiable;

    protected $table = 'users';

    const UPDATED_AT = null;

    protected $fillable = [
        'email',
        'password',
        'last_name',
        'first_name',
    ];

    protected $hidden = [
        'password',
    ];

    protected function casts(): array
    {
        return [
            'created_at' => 'datetime',
            'password' => 'hashed',
        ];
    }

    public function getFullNameAttribute(): ?string
    {
        $name = trim("{$this->first_name} {$this->last_name}");
        return $name !== '' ? $name : null;
    }

    public function questionnaires()
    {
        return $this->hasMany(Questionnaire::class);
    }

    public function preference()
    {
        return $this->hasOne(UserPreference::class);
    }
}
