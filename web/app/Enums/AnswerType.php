<?php

namespace App\Enums;

enum AnswerType: string
{
    case TRUE_FALSE = 'TRUE_FALSE';
    case MULTIPLE_CHOICE = 'MULTIPLE_CHOICE';

    public function label(): string
    {
        return match ($this) {
            self::TRUE_FALSE => __('messages.question.type_truefalse'),
            self::MULTIPLE_CHOICE => __('messages.question.type_multiple'),
        };
    }
}
