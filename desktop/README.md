# StadiumCompany — Desktop

Application desktop cross-platform de gestion de questionnaires, construite avec C# et Avalonia UI.

## Stack technique

| Composant | Version |
|-----------|---------|
| .NET | 8.0 |
| Avalonia UI | 11.2.5 |
| FluentAvalonia | 2.4.1 |
| Npgsql | 8.0.5 |
| BCrypt.Net | 4.0.3 |
| QuestPDF | 2025.12.3 |

## Architecture

Le projet suit une architecture **MVC** avec une couche d'accès aux données (DAL) basée sur le pattern Repository.

```
desktop/
├── Models/          # Entités (User, Questionnaire, Question, Answer, Theme, UserPreferences, ActivityLog)
├── DAL/             # Repositories + Database.cs (+ appels procédures stockées)
├── Views/           # Vues Avalonia (Login, Main, Editor, QuizPlayer, AdminLogs, AdminUsers)
├── Services/        # LocalizationManager, PdfGenerator, ActivityLogger
└── Resources/       # Fichiers de traduction (fr/en)
```

## Prérequis

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL (voir le [README racine](../README.md) pour la configuration BDD)

## Installation

```bash
cd desktop

# Restaurer les dépendances
dotnet restore

# Lancer l'application
dotnet run
```

Ou avec hot-reload :

```bash
dotnet watch
```

> La chaîne de connexion PostgreSQL se configure dans `DAL/Database.cs`.
