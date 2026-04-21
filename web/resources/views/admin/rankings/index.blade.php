@extends('layouts.app')

@section('content')
<div id="rankings-app"
     data-questionnaires-url="{{ route('admin.rankings.api.questionnaires') }}"
     data-leaderboard-base-url="{{ url('admin/rankings/api') }}">
</div>
@endsection
