import { Component, signal, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NurseryRhymeService, NurseryRhyme } from '../../services/nursery-rhyme.service';
import { ThemeService } from '../../services/theme.service';
import { Theme } from '../../models/theme.model';
import { ThemeSelectorComponent } from '../../components/theme-selector/theme-selector.component';

@Component({
  selector: 'app-library',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ThemeSelectorComponent],
  templateUrl: './library.component.html',
  styleUrl: './library.component.scss'
})
export class LibraryComponent implements OnInit {
  protected readonly nurseryRhymes = signal<NurseryRhyme[]>([]);
  protected readonly themes = signal<Theme[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly showModal = signal(false);
  protected readonly editingRhyme = signal<NurseryRhyme | null>(null);
  protected readonly saving = signal(false);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly selectedImageFile = signal<File | null>(null);
  protected readonly lightboxImage = signal<{ url: string; title: string } | null>(null);
  
  private nurseryRhymeService = inject(NurseryRhymeService) as NurseryRhymeService;
  private themeService = inject(ThemeService) as ThemeService;
  private formBuilder = inject(FormBuilder) as FormBuilder;
  
  protected rhymeForm: FormGroup = this.formBuilder.group({
    title: ['', [Validators.required, Validators.minLength(2)]],
    url: [''],
    themeIds: [[]]
  });

  ngOnInit() {
    this.loadRhymes();
    this.loadThemes();
  }

  private loadRhymes() {
    this.loading.set(true);
    this.error.set(null);
    
    this.nurseryRhymeService.getAllRhymes().subscribe({
      next: (rhymes: NurseryRhyme[]) => {
        this.nurseryRhymes.set(rhymes);
        this.loading.set(false);
      },
      error: (err: any) => {
        this.error.set('Erreur lors du chargement des comptines: ' + err.message);
        this.loading.set(false);
      }
    });
  }

  private loadThemes() {
    this.themeService.getThemes().subscribe({
      next: (themes: Theme[]) => {
        this.themes.set(themes);
      },
      error: (err: any) => {
        console.error('Erreur lors du chargement des thèmes:', err);
      }
    });
  }

  openAddModal() {
    this.editingRhyme.set(null);
    this.rhymeForm.reset({
      title: '',
      url: '',
      themeIds: []
    });
    this.selectedFile.set(null);
    this.selectedImageFile.set(null);
    this.showModal.set(true);
  }

  editRhyme(rhyme: NurseryRhyme) {
    this.editingRhyme.set(rhyme);
    this.rhymeForm.patchValue({
      title: rhyme.title,
      url: rhyme.url || '',
      themeIds: rhyme.themeIds || []
    });
    this.selectedFile.set(null);
    this.selectedImageFile.set(null);
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.editingRhyme.set(null);
    this.selectedFile.set(null);
    this.selectedImageFile.set(null);
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
    }
  }

  onImageSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedImageFile.set(file);
    }
  }

  saveRhyme() {
    if (this.rhymeForm.invalid) {
      return;
    }

    this.saving.set(true);
    
    const rhymeData = {
      title: this.rhymeForm.value.title,
      url: this.rhymeForm.value.url || '',
      themeIds: this.rhymeForm.value.themeIds || []
    };

    const operation = this.editingRhyme() 
      ? this.nurseryRhymeService.updateRhyme(this.editingRhyme()!.id, rhymeData, this.selectedFile(), this.selectedImageFile())
      : this.nurseryRhymeService.createRhyme(rhymeData, this.selectedFile(), this.selectedImageFile());

    operation.subscribe({
      next: () => {
        this.saving.set(false);
        this.closeModal();
        this.loadRhymes();
      },
      error: (err: any) => {
        this.saving.set(false);
        this.error.set('Erreur lors de l\'enregistrement: ' + err.message);
      }
    });
  }

  deleteRhyme(id: string) {
    if (confirm('Êtes-vous sûr de vouloir supprimer cette comptine ?')) {
      this.nurseryRhymeService.deleteRhyme(id).subscribe({
        next: () => {
          this.loadRhymes();
        },
        error: (err: any) => {
          this.error.set('Erreur lors de la suppression: ' + err.message);
        }
      });
    }
  }

  getAudioUrl(fileName: string): string {
    return this.nurseryRhymeService.getAudioUrl(fileName);
  }

  getImageUrl(fileName: string): string {
    return this.nurseryRhymeService.getImageUrl(fileName);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('fr-FR');
  }

  getThemesForRhyme(themeIds: string[]): Theme[] {
    return this.themes().filter(theme => themeIds.includes(theme.id));
  }

  openLightbox(imageFileName: string, title: string) {
    this.lightboxImage.set({
      url: this.getImageUrl(imageFileName),
      title: title
    });
  }

  closeLightbox() {
    this.lightboxImage.set(null);
  }
}