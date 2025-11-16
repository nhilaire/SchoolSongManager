import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Api, PingResponse } from './services/api';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected readonly apiStatus = signal<string>('Checking...');

  constructor(private api: Api) {}

  ngOnInit() {
    this.testApiConnection();
  }

  private testApiConnection() {
    this.apiStatus.set('Testing connection...');

    this.api.ping().subscribe({
      next: () => {
        this.apiStatus.set('✅ API Connected');
      },
      error: () => {
        this.apiStatus.set('❌ API Connection Failed');
      }
    });
  }
}
