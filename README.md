# StadiumCompany - Gestion de Questionnaires

Application de gestion de questionnaires d'évaluation pour StadiumCompany, développée en C# avec Avalonia UI (cross-platform).

## Fonctionnalités

- Authentification (connexion / inscription)
- Gestion des questionnaires (CRUD)
- Gestion des questions (Vrai/Faux ou Choix multiples)
- Gestion des réponses avec système de poids/points
- Publication des questionnaires
- Mode joueur pour répondre aux questionnaires publiés
- Préférences utilisateur (thème clair/sombre, langue)
- Internationalisation (français / anglais)

## Prérequis

- .NET 8.0 SDK
- PostgreSQL Server

## Installation

### 1. Base de données

Créer l'utilisateur et la base de données PostgreSQL :

```bash
sudo -u postgres psql
```

```sql
CREATE USER stadiumcompany WITH PASSWORD 'stadiumcompany';
CREATE DATABASE stadiumcompany OWNER stadiumcompany;
\q
```

Exécuter les scripts SQL :

```bash
psql -U stadiumcompany -d stadiumcompany -f schema.sql
psql -U stadiumcompany -d stadiumcompany -f insert.sql
```

Si vous recréez les tables en tant que superuser (postgres), accordez les permissions :

```bash
sudo -u postgres psql -d stadiumcompany
```

```sql
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO stadiumcompany;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO stadiumcompany;
```

### 2. Configuration

La chaîne de connexion se trouve dans `DAL/Database.cs` :

```csharp
private const string ConnectionString = "Host=localhost;Database=stadiumcompany;Username=stadiumcompany;Password=stadiumcompany";
```

### 3. Compilation et exécution

```bash
dotnet build
dotnet run
```

Ou avec hot-reload :

```bash
dotnet watch
```

## Structure du projet

```
stadiumcompany/
├── Models/                 # Classes métier
│   ├── User.cs
│   ├── Theme.cs
│   ├── Questionnaire.cs
│   ├── Question.cs
│   ├── Answer.cs
│   └── UserPreferences.cs
├── DAL/                    # Data Access Layer (repositories)
│   ├── Database.cs
│   ├── UserRepository.cs
│   ├── ThemeRepository.cs
│   ├── QuestionnaireRepository.cs
│   ├── QuestionRepository.cs
│   ├── AnswerRepository.cs
│   └── UserPreferencesRepository.cs
├── Views/                  # Vues Avalonia (AXAML)
│   ├── MainWindow.axaml
│   ├── LoginView.axaml
│   ├── MainView.axaml
│   ├── QuestionnaireEditorWindow.axaml
│   ├── QuestionEditorWindow.axaml
│   ├── AnswerEditorWindow.axaml
│   └── QuizPlayerWindow.axaml
├── Services/               # Services applicatifs
│   └── LocalizationManager.cs
├── Resources/              # Fichiers de localisation
│   ├── Strings.fr.json
│   └── Strings.en.json
├── Program.cs              # Point d'entrée
├── App.axaml               # Configuration Avalonia
├── StadiumCompany.csproj   # Configuration du projet
├── schema.sql              # Script de création BDD
└── insert.sql              # Données initiales
```

## Architecture

Le projet suit le pattern **MVC** (Model-View-Controller) :

- **Models/** : Classes représentant les entités métier
- **DAL/** : Couche d'accès aux données (repositories)
- **Views/** : Interface utilisateur (vues Avalonia)
- **Services/** : Services transverses (localisation)
- **Resources/** : Fichiers de traduction (JSON)

## Base de données

### MCD

```
users (0,n) ──── crée ──── (1,1) questionnaires
themes (0,n) ──── catégorise ──── (1,1) questionnaires
questionnaires (1,1) ──── contient ──── (0,n) questions
questions (1,1) ──── propose ──── (0,n) answers
users (1,1) ──── possède ──── (0,1) user_preferences
```

Le schéma détaillé des tables se trouve dans le fichier `schema.sql`.

## Technologies

- **Framework** : .NET 8.0
- **UI** : [Avalonia UI](https://avaloniaui.net/) avec [FluentAvalonia](https://github.com/amwx/FluentAvalonia)
- **Base de données** : PostgreSQL
- **Driver** : Npgsql

## Licence

Projet pédagogique - SIO2 PPE SLAM
