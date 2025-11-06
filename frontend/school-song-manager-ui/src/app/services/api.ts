import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app-config';

export interface PingResponse {
  message: string;
  timestamp: string;
  version: string;
}

export interface HealthResponse {
  status: string;
  service: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root',
})
export class Api {
  private readonly baseUrl: string;

  constructor(private http: HttpClient, private appConfig: AppConfig) {
    this.baseUrl = this.appConfig.apiUrl;
  }

  ping(): Observable<PingResponse> {
    return this.http.get<PingResponse>(`${this.baseUrl}/api/ping`);
  }

  health(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(`${this.baseUrl}/api/ping/health`);
  }
}
