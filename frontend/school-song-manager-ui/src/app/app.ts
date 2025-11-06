import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Api, PingResponse, HealthResponse } from './services/api';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly title = signal('School Song Manager');
  protected readonly apiStatus = signal<string>('Checking...');
  protected readonly pingResponse = signal<PingResponse | null>(null);
  protected readonly healthResponse = signal<HealthResponse | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor(private api: Api) {}

  ngOnInit() {
    this.testApiConnection();
  }

  testApiConnection() {
    this.error.set(null);
    this.apiStatus.set('Testing connection...');

    // Test ping endpoint
    this.api.ping().subscribe({
      next: (response) => {
        this.pingResponse.set(response);
        this.apiStatus.set('✅ API Connected');
      },
      error: (err) => {
        console.error('Ping failed:', err);
        this.error.set(`Ping failed: ${err.message || 'Unknown error'}`);
        this.apiStatus.set('❌ API Connection Failed');
      }
    });

    // Test health endpoint
    this.api.health().subscribe({
      next: (response) => {
        this.healthResponse.set(response);
      },
      error: (err) => {
        console.error('Health check failed:', err);
      }
    });
  }

  retryConnection() {
    this.testApiConnection();
  }
}
