# School Song Manager

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