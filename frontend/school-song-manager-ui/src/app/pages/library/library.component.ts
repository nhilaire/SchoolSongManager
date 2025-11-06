import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NurseryRhymeService, NurseryRhyme } from '../../services/nursery-rhyme.service';

@Component({
  selector: 'app-library',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './library.component.html',
  styleUrl: './library.component.scss'
})
export class LibraryComponent implements OnInit {
  protected readonly nurseryRhymes = signal<NurseryRhyme[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly showModal = signal(false);
  protected readonly editingRhyme = signal<NurseryRhyme | null>(null);
  protected readonly saving = signal(false);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly selectedImageFile = signal<File | null>(null);
  
  protected formData = {
    title: '',
    url: ''
  };

  constructor(private nurseryRhymeService: NurseryRhymeService) {}

  ngOnInit() {
    this.loadRhymes();
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

  openAddModal() {
    this.editingRhyme.set(null);
    this.formData = { title: '', url: '' };
    this.selectedFile.set(null);
    this.selectedImageFile.set(null);
    this.showModal.set(true);
  }

  editRhyme(rhyme: NurseryRhyme) {
    this.editingRhyme.set(rhyme);
    this.formData = { title: rhyme.title, url: rhyme.url || '' };
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
    if (!this.formData.title) {
      return;
    }

    this.saving.set(true);
    
    const rhymeData = {
      title: this.formData.title,
      url: this.formData.url
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
}