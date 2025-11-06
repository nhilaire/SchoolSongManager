import { Component, Input, Output, EventEmitter, OnInit, inject, forwardRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ThemeService } from '../../services/theme.service';
import { Theme } from '../../models/theme.model';

@Component({
  selector: 'app-theme-selector',
  standalone: true,
  imports: [CommonModule, RouterModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ThemeSelectorComponent),
      multi: true
    }
  ],
  templateUrl: './theme-selector.component.html',
  styleUrl: './theme-selector.component.scss'
})
export class ThemeSelectorComponent implements OnInit, ControlValueAccessor {
  private themeService = inject(ThemeService);

  @Input() label: string = 'Thèmes';
  @Input() placeholder: string = 'Sélectionnez un ou plusieurs thèmes';
  
  availableThemes: Theme[] = [];
  selectedThemeIds: string[] = [];

  // ControlValueAccessor properties
  private onChange = (value: string[]) => {};
  private onTouched = () => {};
  disabled = false;

  ngOnInit(): void {
    this.loadThemes();
  }

  loadThemes(): void {
    this.themeService.getThemes().subscribe({
      next: (themes: Theme[]) => {
        this.availableThemes = themes;
      },
      error: (error: any) => {
        console.error('Erreur lors du chargement des thèmes:', error);
      }
    });
  }

  isThemeSelected(themeId: string): boolean {
    return this.selectedThemeIds.includes(themeId);
  }

  toggleTheme(themeId: string): void {
    if (this.disabled) return;

    const index = this.selectedThemeIds.indexOf(themeId);
    if (index > -1) {
      // Retirer le thème
      this.selectedThemeIds = this.selectedThemeIds.filter(id => id !== themeId);
    } else {
      // Ajouter le thème
      this.selectedThemeIds = [...this.selectedThemeIds, themeId];
    }

    this.onChange(this.selectedThemeIds);
    this.onTouched();
  }

  // ControlValueAccessor implementation
  writeValue(value: string[]): void {
    this.selectedThemeIds = value || [];
  }

  registerOnChange(fn: (value: string[]) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }
}