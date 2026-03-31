# StadiumCompany — Web

Application web de gestion de questionnaires, construite avec Laravel et Tailwind CSS.

## Stack technique

| Composant | Version |
|-----------|---------|
| PHP | ^8.2 |
| Laravel | ^12.0 |
| Tailwind CSS | ^4.1.18 |
| Alpine.js | ^3.4.2 |
| Vite | ^7.0.7 |
| DomPDF | ^3.1 |
| Laravel Breeze | ^2.3 |

## Architecture

Le projet suit une architecture **MVC** avec une couche service pour la logique métier.

```
web/
├── app/
│   ├── Http/
│   │   ├── Controllers/       # Contrôleurs (Dashboard, Questionnaire, Quiz, Pdf, Theme, UserPreferences, QuestionFeedback)
│   │   ├── Controllers/Admin/ # Contrôleurs admin (AdminLogController)
│   │   ├── Middleware/        # ApplyUserPreferences, CheckPendingMigrations, EnsureIsAdmin
│   │   └── Requests/          # Form Request validation
│   ├── Models/                # Modèles Eloquent (+ ActivityLog, QuestionFeedback)
│   ├── Services/              # QuestionnaireService, QuizService, QuizScoringService, ActivityLogService, QuestionFeedbackService
│   ├── Enums/                 # Enums de types
│   └── Policies/              # Politiques d'autorisation
├── resources/views/           # Templates Blade (+ quiz/review, admin/logs)
├── routes/                    # web.php, auth.php
└── database/migrations/       # Migrations
```

## Prérequis

- PHP 8.2+
- [Composer](https://getcomposer.org/)
- [Node.js](https://nodejs.org/) (pour Vite et Tailwind)
- PostgreSQL (voir le [README racine](../README.md) pour la configuration BDD)

## Installation

```bash
cd web

# Installation complète (composer install, .env, key:generate, migrations, npm install, npm build)
composer setup
```

> Configurer la connexion PostgreSQL dans le fichier `.env` avant de lancer `composer setup`.

## Lancement

```bash
# Démarre le serveur Laravel, la queue, les logs et Vite en parallèle
composer dev
```

## Tests

```bash
composer test
```
