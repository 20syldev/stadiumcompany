# StadiumCompany

Application de gestion de questionnaires d'évaluation développée en double plateforme dans le cadre du PPE du BTS SIO option SLAM. Le projet a été réalisé de décembre 2025 à mars 2026.

Les deux clients — desktop (C# / Avalonia UI) et web (Laravel) — partagent la même base de données PostgreSQL, permettant une utilisation simultanée sur les deux plateformes.

## Fonctionnalités

-   Authentification (connexion / inscription)
-   Gestion CRUD des questionnaires, questions et réponses
-   Questions vrai/faux ou choix multiples avec pondération
-   Publication et fork de questionnaires
-   Mode joueur pour répondre aux quiz avec scoring
-   Synthèse corrigée (procès-verbal) après un quiz
-   Feedback par question (note + commentaire)
-   Génération PDF des questionnaires
-   Supervision admin : journaux d'activité (desktop + web)
-   Gestion des utilisateurs : archivage/désarchivage avec procédures stockées
-   Script de sauvegarde quotidienne de la base de données
-   Thème clair/sombre
-   Internationalisation (français / anglais)

## Stack technique

| Composant       | Technologie                       |
| --------------- | --------------------------------- |
| Base de données | PostgreSQL                        |
| Desktop         | C# / .NET 8.0, Avalonia UI 11.2.5 |
| Web             | PHP 8.2, Laravel 12, Tailwind CSS 4 |

## Base de données

### MCD

```
users (0,n)              ────  crée        ────  (1,1) questionnaires
themes (0,n)             ────  catégorise  ────  (1,1) questionnaires
questionnaires (1,1)     ────  contient    ────  (0,n) questions
questions (1,1)          ────  propose     ────  (0,n) answers
users (1,1)              ────  possède     ────  (0,1) user_preferences
users (0,n)              ────  soumet      ────  (1,1) quiz_submissions
questionnaires (0,n)     ────  évalue      ────  (1,1) quiz_submissions
quiz_submissions (1,1)   ────  détaille    ────  (0,n) quiz_answers
users (0,n)              ────  rédige      ────  (1,1) question_feedbacks
questions (0,n)          ────  reçoit      ────  (1,1) question_feedbacks
users (0,n)              ────  génère      ────  (1,1) activity_logs
```

Le schéma détaillé des tables se trouve dans `schema.sql`.

## Installation

**Prérequis :** [PostgreSQL](https://www.postgresql.org/), [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), [PHP 8.2+](https://www.php.net/), [Composer](https://getcomposer.org/), [Node.js](https://nodejs.org/)

Créer l'utilisateur et la base de données PostgreSQL :

```sql
CREATE USER stadiumcompany WITH PASSWORD 'stadiumcompany';
CREATE DATABASE stadiumcompany OWNER stadiumcompany;
```

Exécuter les scripts SQL situés à la racine du projet :

```bash
psql -U stadiumcompany -d stadiumcompany -f schema.sql
psql -U stadiumcompany -d stadiumcompany -f insert.sql
```

Pour l'installation de chaque plateforme, voir [README Desktop](desktop/README.md#installation) et [README Web](web/README.md#installation).

## Structure du projet

```
stadiumcompany/
├── desktop/       # Application desktop (C# / Avalonia)
├── web/           # Application web (Laravel)
├── scripts/       # Scripts utilitaires (backup.sh)
├── schema.sql     # Script de création BDD (tables, procédures stockées, trigger)
└── insert.sql     # Données initiales
```
