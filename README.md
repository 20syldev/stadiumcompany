# StadiumCompany

Application de gestion de questionnaires d'évaluation développée en double plateforme : desktop et web. Projet réalisé dans le cadre du PPE SLAM (SIO2).

## Fonctionnalités

- Authentification (connexion / inscription)
- Gestion CRUD des questionnaires, questions et réponses
- Questions vrai/faux ou choix multiples avec pondération
- Publication et fork de questionnaires
- Mode joueur pour répondre aux quiz avec scoring
- Génération PDF des questionnaires
- Thème clair/sombre
- Internationalisation (français / anglais)

## Stack technique

| Composant | Technologie |
|-----------|-------------|
| Base de données | PostgreSQL |
| Desktop | C# / .NET 8.0, Avalonia UI |
| Web | PHP 8.2, Laravel 12, Tailwind CSS |

## Base de données

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

### MCD

```
users (0,n) ──── crée ──── (1,1) questionnaires
themes (0,n) ──── catégorise ──── (1,1) questionnaires
questionnaires (1,1) ──── contient ──── (0,n) questions
questions (1,1) ──── propose ──── (0,n) answers
users (1,1) ──── possède ──── (0,1) user_preferences
```

Le schéma détaillé des tables se trouve dans `schema.sql`.

## Structure du projet

```
stadiumcompany/
├── desktop/       # Application desktop (C# / Avalonia)
├── web/           # Application web (Laravel)
├── schema.sql     # Script de création BDD
└── insert.sql     # Données initiales
```

## Documentation

- [README Desktop](desktop/README.md) — Installation et détails de l'application desktop
- [README Web](web/README.md) — Installation et détails de l'application web
