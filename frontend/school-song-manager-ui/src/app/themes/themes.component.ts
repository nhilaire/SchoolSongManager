import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ThemeService } from '../services/theme.service';
import { Theme } from '../models/theme.model';

@Component({
  selector: 'app-themes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './themes.component.html',
  styleUrl: './themes.component.scss'
})
export class ThemesComponent implements OnInit {
  private themeService = inject(ThemeService);
  private formBuilder = inject(FormBuilder);

  themes: Theme[] = [];
  showCreateForm = false;
  editingTheme: Theme | null = null;
  deleteInProgress: string | null = null;

  themeForm: FormGroup = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    color: ['#007bff', [Validators.required]]
  });

  ngOnInit(): void {
    this.loadThemes();
  }

  loadThemes(): void {
    this.themeService.getThemes().subscribe({
      next: (themes: Theme[]) => {
        this.themes = themes;
      },
      error: (error: any) => {
        console.error('Erreur lors du chargement des thèmes:', error);
      }
    });
  }

  onSubmit(): void {
    if (this.themeForm.valid) {
      const themeData = this.themeForm.value;

      if (this.editingTheme) {
        this.themeService.updateTheme(this.editingTheme.id, themeData).subscribe({
          next: () => {
            this.loadThemes();
            this.cancelForm();
          },
          error: (error: any) => {
            console.error('Erreur lors de la modification du thème:', error);
          }
        });
      } else {
        this.themeService.createTheme(themeData).subscribe({
          next: () => {
            this.loadThemes();
            this.cancelForm();
          },
          error: (error: any) => {
            console.error('Erreur lors de la création du thème:', error);
          }
        });
      }
    }
  }

  editTheme(theme: Theme): void {
    this.editingTheme = theme;
    this.showCreateForm = true;
    this.themeForm.patchValue({
      name: theme.name,
      color: theme.color
    });
  }

  deleteTheme(themeId: string): void {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce thème ?')) {
      this.deleteInProgress = themeId;
      
      this.themeService.deleteTheme(themeId).subscribe({
        next: () => {
          this.loadThemes();
          this.deleteInProgress = null;
        },
        error: (error: any) => {
          console.error('Erreur lors de la suppression du thème:', error);
          this.deleteInProgress = null;
          
          if (error.status === 400) {
            alert('Ce thème ne peut pas être supprimé car il est utilisé par des comptines.');
          }
        }
      });
    }
  }

  cancelForm(): void {
    this.showCreateForm = false;
    this.editingTheme = null;
    this.themeForm.reset({
      name: '',
      color: '#007bff'
    });
  }
}