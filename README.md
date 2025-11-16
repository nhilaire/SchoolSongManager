# School Song Manager

Système de gestion de comptines pour classes de maternelle.

## Fonctionnalités

- ✅ Connexion API avec indicateur de statut
- 📚 **Bibliothèque de comptines** avec CRUD complet
- 🎵 **Gestion des fichiers audio** pour chaque comptine
- �️ **Gestion des images** associées aux comptines
- 🔗 **Liens externes** optionnels vers des ressources en ligne
- �🔒 **Stockage sécurisé** dans le répertoire data d'Azure Web App
- 🔄 **Repository centralisé** avec verrouillage de fichiers pour éviter les conflits

## Architecture

### Frontend (Angular)
- **Framework** : Angular 18+ avec standalone components
- **Routing** : Navigation entre page d'accueil et bibliothèque
- **Services** : Service dédié pour l'API des comptines
- **UI** : Interface responsive avec modales pour l'édition

### Backend (.NET 9)
- **API** : Contrôleur REST pour la gestion des comptines
- **Stockage** : JSON pour les métadonnées + fichiers audio séparés
- **Repository** : Pattern repository avec SemaphoreSlim pour la concurrence
- **CORS** : Configuration pour permettre l'accès depuis le frontend

## Structure des données

### Comptine (NurseryRhyme)
```json
{
  "id": "guid",
  "title": "Titre de la comptine",
  "imageFileName": "nom_image.jpg",
  "url": "https://example.com/comptine",
  "audioFileName": "nom_fichier.mp3",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

### Stockage
- **Métadonnées** : `/data/nursery-rhymes.json`
- **Fichiers audio** : `/data/audio/`
- **Images** : `/data/images/`

## Installation et développement

### Prérequis
- .NET 9 SDK
- Node.js 18+
- Angular CLI

### Backend
```bash
cd backend/SchoolSongManager.Api
dotnet restore
dotnet run
```
L'API sera disponible sur `https://localhost:7xxx`

### Frontend
```bash
cd frontend/school-song-manager-ui
npm install
ng serve
```
L'application sera disponible sur `http://localhost:4200`

## Déploiement Azure

### Configuration requise
- Azure Web App avec .NET 9
- Répertoire `/home/data` accessible en écriture
- Variables d'environnement :
  - `DataPath` : `/home/data` (par défaut)
  - `CORS:AllowedOrigins` : URLs autorisées

### Processus
1. Build du frontend : `ng build --prod`
2. Copie des fichiers statiques dans `wwwroot`
3. Publication du backend : `dotnet publish`
4. Déploiement via Azure CLI ou GitHub Actions

## API Endpoints

### Comptines
- `GET /api/nursery-rhymes` - Liste toutes les comptines
- `GET /api/nursery-rhymes/{id}` - Récupère une comptine par ID
- `POST /api/nursery-rhymes` - Crée une nouvelle comptine (avec upload audio et image)
- `PUT /api/nursery-rhymes/{id}` - Met à jour une comptine (avec upload audio et image)
- `DELETE /api/nursery-rhymes/{id}` - Supprime une comptine et ses fichiers
- `GET /api/nursery-rhymes/audio/{fileName}` - Télécharge un fichier audio
- `GET /api/nursery-rhymes/images/{fileName}` - Télécharge une image

### Ping (monitoring)
- `GET /api/ping` - Test de connectivité
- `GET /api/ping/health` - Vérification de santé

## Sécurité

- Validation des entrées côté backend
- Sanitization des noms de fichiers
- Limitation des types de fichiers audio acceptés
- Gestion des erreurs avec messages appropriés
- Verrouillage concurrent pour éviter les corruptions de données

## Développement futur

### Fonctionnalités envisagées
- 🔍 Recherche et filtrage des comptines
- 📂 Catégorisation par thème/âge
- 🎨 Upload d'images associées aux comptines
- 📊 Statistiques d'utilisation
- 👥 Gestion multi-utilisateurs
- 🔄 Sauvegarde/synchronisation cloud
- 📱 Version mobile/PWA

### Améliorations techniques
- Cache Redis pour les performances
- Base de données relationnelle (PostgreSQL)
- Tests unitaires et d'intégration
- CI/CD complet avec GitHub Actions
- Monitoring et logs centralisés
- Authentification OAuth2/JWT

A web application for managing nursery rhymes and songs for kindergarten classes. This application helps teachers organize, categorize, and generate PDF collections of songs for different periods and school years.

## Features

### Core Functionality
- **Song Library Management**: Manage a comprehensive library of nursery rhymes with titles, optional URLs, audio excerpts, and images
- **Theme-based Organization**: Categorize songs using flexible theme tags for easy filtering and organization
- **Period-based Selection**: Generate PDF collections based on school year and period
- **Historical Tracking**: Maintain history of song usage across years to ensure variety and prevent repetition
- **Smart Filtering**: Exclude songs from specific years when planning new collections

### PDF Generation
- Generate formatted PDF documents containing selected songs
- Include song titles and associated images
- Customizable layout for classroom use

## Architecture

### Frontend
- **Framework**: Angular (latest version)
- **Hosting**: Azure Static Web Apps (Free tier)
- **Features**: Responsive design, song management interface, PDF generation controls

### Backend
- **Framework**: .NET 9 Web API
- **Hosting**: Azure Web Apps (Free tier)
- **Data Storage**: File-based storage in persistent `data` directory
- **Features**: RESTful API, CORS support for cross-origin requests

### Deployment
- **CI/CD**: GitHub Actions for automated deployment
- **Infrastructure**: Azure cloud services
- **Data Persistence**: Backend data directory preserved across deployments

## Development Setup

### Prerequisites
- Node.js (latest LTS version)
- .NET 9 SDK
- Visual Studio or Visual Studio Code
- Git

### Backend Setup
1. Navigate to the backend directory:
   ```bash
   cd backend
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run in development mode:
   ```bash
   dotnet run
   ```
   Or debug with Visual Studio by opening the solution file.

### Frontend Setup
1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Start development server:
   ```bash
   npm start
   ```

### Local Development
- Backend runs on `https://localhost:7000` (HTTPS) and `http://localhost:5000` (HTTP)
- Frontend runs on `http://localhost:4200`
- CORS is configured to allow frontend-backend communication

## Deployment

### Automatic Deployment
The application uses GitHub Actions for continuous deployment:
- **Frontend**: Automatically deployed to Azure Static Web Apps on push to main branch
- **Backend**: Automatically deployed to Azure Web Apps on push to main branch

### Manual Deployment
Refer to the `.github/workflows` directory for deployment configuration details.

## Data Structure

### Song Entity
```json
{
  "id": "string",
  "title": "string",
  "url": "string (optional)",
  "audioFile": "string (path to audio excerpt)",
  "imageFile": "string (path to image)",
  "themes": ["string"],
  "createdDate": "datetime",
  "lastUsed": "datetime"
}
```

### Usage History
```json
{
  "songId": "string",
  "year": "number",
  "period": "string",
  "usageDate": "datetime"
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions, please open an issue in the GitHub repository.