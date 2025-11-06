import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app-config';

export interface Theme {
  id: string;
  name: string;
  color: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateThemeRequest {
  name: string;
  color: string;
}

export interface UpdateThemeRequest {
  name: string;
  color: string;
}

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly baseUrl: string;

  constructor(private http: HttpClient, private appConfig: AppConfig) {
    this.baseUrl = this.appConfig.apiUrl;
  }

  getAllThemes(): Observable<Theme[]> {
    return this.http.get<Theme[]>(`${this.baseUrl}/api/themes`);
  }

  getThemes(): Observable<Theme[]> {
    return this.http.get<Theme[]>(`${this.baseUrl}/api/themes`);
  }

  getTheme(id: string): Observable<Theme> {
    return this.http.get<Theme>(`${this.baseUrl}/api/themes/${id}`);
  }

  createTheme(theme: CreateThemeRequest): Observable<Theme> {
    return this.http.post<Theme>(`${this.baseUrl}/api/themes`, theme);
  }

  updateTheme(id: string, theme: UpdateThemeRequest): Observable<Theme> {
    return this.http.put<Theme>(`${this.baseUrl}/api/themes/${id}`, theme);
  }

  deleteTheme(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/themes/${id}`);
  }
}