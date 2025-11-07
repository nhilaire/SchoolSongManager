import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AssignmentDashboardComponent } from '../../components/assignment-dashboard/assignment-dashboard.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, AssignmentDashboardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent { }