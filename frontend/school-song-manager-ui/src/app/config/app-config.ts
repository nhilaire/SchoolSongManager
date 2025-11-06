import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AppConfig {
  
  private getEnvironmentVariable(name: string, defaultValue: string): string {
    // Try to get from build-time environment variables
    if (typeof window !== 'undefined' && (window as any).__env && (window as any).__env[name]) {
      return (window as any).__env[name];
    }
    
    // Fallback to default value
    return defaultValue;
  }

  get apiUrl(): string {
    // Priorité 1: Variable d'environnement (build-time ou runtime)
    const envApiUrl = this.getEnvironmentVariable('API_URL', '');
    if (envApiUrl) {
      return envApiUrl;
    }
    
    // Priorité 2: Développement local
    if (window.location.hostname === 'localhost') {
      return 'https://localhost:7159';
    }
    
    // Priorité 3: Erreur - aucune configuration trouvée
    throw new Error('API_URL not configured. Please set the API_URL environment variable.');
  }

  get isProduction(): boolean {
    return this.getEnvironmentVariable('NODE_ENV', 'development') === 'production';
  }
}
