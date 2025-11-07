import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NurseryRhymeAssignmentService } from '../../services/nursery-rhyme-assignment.service';
import { NurseryRhymeService, NurseryRhyme } from '../../services/nursery-rhyme.service';
import { 
  NurseryRhymeAssignmentWithDetails,
  CreateAssignmentRequest,
  SCHOOL_PERIODS, 
  PERIOD_LABELS,
  SchoolPeriod 
} from '../../models/nursery-rhyme-assignment.model';

@Component({
  selector: 'app-assignment-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './assignment-dashboard.component.html',
  styleUrl: './assignment-dashboard.component.scss'
})
export class AssignmentDashboardComponent implements OnInit {
  assignments = signal<NurseryRhymeAssignmentWithDetails[]>([]);
  schoolYears = signal<string[]>([]);
  selectedSchoolYear = signal<string>('');
  selectedPeriod = signal<string>('');
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  // Nouvelle section de génération
  allNurseryRhymes = signal<NurseryRhyme[]>([]);
  targetSchoolYear = '';
  targetPeriod = '';
  selectedRhymeIds = signal<string[]>([]);
  generationLoading = signal<boolean>(false);
  generationMessage = signal<string>('');
  deletionLoading = signal<boolean>(false);

  // Gestion de l'accordéon
  expandedSections = signal<Set<string>>(new Set(['assignments', 'generator']));

  // Expose constants to template
  schoolPeriods = SCHOOL_PERIODS;
  periodLabels = PERIOD_LABELS;

  // Computed values
  filteredAssignments = computed(() => {
    let filtered = this.assignments();
    
    if (this.selectedSchoolYear()) {
      filtered = filtered.filter(a => a.schoolYear === this.selectedSchoolYear());
    }
    
    if (this.selectedPeriod()) {
      filtered = filtered.filter(a => a.period === this.selectedPeriod());
    }
    
    return filtered.sort((a, b) => {
      // Sort by school year desc first, then by period
      if (a.schoolYear !== b.schoolYear) {
        return b.schoolYear.localeCompare(a.schoolYear);
      }
      // Sort periods P1, P2, P3, P4, P5
      return a.period.localeCompare(b.period);
    });
  });

  currentSchoolYear = computed(() => this.assignmentService.getCurrentSchoolYear());

  constructor(
    private assignmentService: NurseryRhymeAssignmentService,
    private nurseryRhymeService: NurseryRhymeService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    
    try {
      // Load available school years
      const schoolYears = await this.assignmentService.getAvailableSchoolYears().toPromise();
      this.schoolYears.set(schoolYears || []);
      
      // Ne pas définir de filtre par défaut - laisser l'utilisateur choisir
      // L'utilisateur verra toutes les assignations au démarrage
      
      // Load all assignments
      const assignments = await this.assignmentService.getAllAssignments().toPromise();
      this.assignments.set(assignments || []);
      
      // Load all nursery rhymes for the generator
      const nurseryRhymes = await this.nurseryRhymeService.getAllRhymes().toPromise();
      this.allNurseryRhymes.set(nurseryRhymes || []);
      
    } catch (error) {
      console.error('Error loading assignment data:', error);
      this.error.set('Erreur lors du chargement des données');
    } finally {
      this.loading.set(false);
    }
  }

  onSchoolYearChange(schoolYear: string): void {
    this.selectedSchoolYear.set(schoolYear);
  }

  onPeriodChange(period: string): void {
    this.selectedPeriod.set(period);
  }

  clearFilters(): void {
    this.selectedSchoolYear.set('');
    this.selectedPeriod.set('');
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  }

  getPeriodLabel(period: string): string {
    return PERIOD_LABELS[period as SchoolPeriod] || period;
  }

  getYearRowClass(schoolYear: string): string {
    // Créer une alternance de couleurs basée sur un hash simple de l'année scolaire
    const yearHash = schoolYear.split('/')[0]; // Prendre la première année (ex: "2024" de "2024/2025")
    const yearNumber = parseInt(yearHash, 10);
    return yearNumber % 2 === 0 ? 'year-even' : 'year-odd';
  }

  // Méthodes pour l'accordéon
  toggleSection(sectionName: string): void {
    const expanded = this.expandedSections();
    const newExpanded = new Set(expanded);
    
    if (expanded.has(sectionName)) {
      newExpanded.delete(sectionName);
    } else {
      newExpanded.add(sectionName);
    }
    
    this.expandedSections.set(newExpanded);
  }

  isExpanded(sectionName: string): boolean {
    return this.expandedSections().has(sectionName);
  }

  // Méthodes pour la sélection des comptines
  isRhymeSelected(rhymeId: string): boolean {
    return this.selectedRhymeIds().includes(rhymeId);
  }

  getImageUrl(imageFileName: string): string {
    return this.nurseryRhymeService.getImageUrl(imageFileName);
  }

  toggleRhymeSelection(rhymeId: string, event: any): void {
    const isChecked = event.target.checked;
    const currentSelection = this.selectedRhymeIds();
    
    if (isChecked) {
      // Ajouter la comptine à la sélection
      this.selectedRhymeIds.set([...currentSelection, rhymeId]);
    } else {
      // Retirer la comptine de la sélection
      this.selectedRhymeIds.set(currentSelection.filter(id => id !== rhymeId));
    }
  }

  // Méthode combinée pour enregistrer ET générer le PDF
  async saveAndGeneratePdf(): Promise<void> {
    if (!this.targetSchoolYear || !this.targetPeriod || this.selectedRhymeIds().length === 0) {
      return;
    }

    this.generationLoading.set(true);
    this.generationMessage.set('Enregistrement des assignations...');

    try {
      // 1. Enregistrer les assignations
      const assignments: CreateAssignmentRequest[] = this.selectedRhymeIds().map(rhymeId => ({
        nurseryRhymeId: rhymeId,
        schoolYear: this.targetSchoolYear,
        period: this.targetPeriod,
        assignedDate: new Date()
      }));

      // Créer les assignations une par une
      for (const assignment of assignments) {
        await this.assignmentService.createAssignment(assignment).toPromise();
      }

      this.generationMessage.set('Génération du PDF...');

      // 2. Générer le PDF directement avec les IDs
      const response = await this.assignmentService.generatePdf({
        nurseryRhymeIds: this.selectedRhymeIds(),
        schoolYear: this.targetSchoolYear,
        period: this.targetPeriod
      }).toPromise();

      if (response) {
        // Le serveur retourne le PDF en tant que blob
        const blob = new Blob([response], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        
        // Créer un lien de téléchargement
        const link = document.createElement('a');
        link.href = url;
        link.download = `comptines_${this.targetSchoolYear.replace('/', '-')}_${this.targetPeriod}.pdf`;
        link.click();
        
        // Nettoyer l'URL
        window.URL.revokeObjectURL(url);

        this.generationMessage.set('Assignations enregistrées et PDF généré avec succès !');
      }

      // 3. Rafraîchir la liste des assignations
      await this.loadData();

      // 4. Réinitialiser la sélection
      this.selectedRhymeIds.set([]);
      this.targetSchoolYear = '';
      this.targetPeriod = '';
      
      // Effacer le message après 3 secondes
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);

    } catch (error) {
      console.error('Error saving assignments and generating PDF:', error);
      this.generationMessage.set('Erreur lors de l\'enregistrement ou de la génération du PDF');
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);
    }
  }

  // Méthodes séparées conservées pour compatibilité mais non utilisées
  async saveAssignments(): Promise<void> {
    if (!this.targetSchoolYear || !this.targetPeriod || this.selectedRhymeIds().length === 0) {
      return;
    }

    this.generationLoading.set(true);
    this.generationMessage.set('Enregistrement des assignations...');

    try {
      const assignments: CreateAssignmentRequest[] = this.selectedRhymeIds().map(rhymeId => ({
        nurseryRhymeId: rhymeId,
        schoolYear: this.targetSchoolYear,
        period: this.targetPeriod,
        assignedDate: new Date()
      }));

      // Créer les assignations une par une
      for (const assignment of assignments) {
        await this.assignmentService.createAssignment(assignment).toPromise();
      }

      // Rafraîchir la liste des assignations
      await this.loadData();

      // Réinitialiser la sélection
      this.selectedRhymeIds.set([]);
      this.targetSchoolYear = '';
      this.targetPeriod = '';

      this.generationMessage.set('Assignations enregistrées avec succès !');
      
      // Effacer le message après 3 secondes
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);

    } catch (error) {
      console.error('Error saving assignments:', error);
      this.generationMessage.set('Erreur lors de l\'enregistrement des assignations');
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);
    }
  }

  async generatePdf(): Promise<void> {
    if (!this.targetSchoolYear || !this.targetPeriod || this.selectedRhymeIds().length === 0) {
      return;
    }

    this.generationLoading.set(true);
    this.generationMessage.set('Génération du PDF...');

    try {
      // Appeler l'endpoint serveur pour générer le PDF
      const response = await this.assignmentService.generatePdf({
        nurseryRhymeIds: this.selectedRhymeIds(),
        schoolYear: this.targetSchoolYear,
        period: this.targetPeriod
      }).toPromise();

      if (response) {
        // Le serveur retourne le PDF en tant que blob
        const blob = new Blob([response], { type: 'application/pdf' });
        const url = window.URL.createObjectURL(blob);
        
        // Créer un lien de téléchargement
        const link = document.createElement('a');
        link.href = url;
        link.download = `comptines_${this.targetSchoolYear.replace('/', '-')}_${this.targetPeriod}.pdf`;
        link.click();
        
        // Nettoyer l'URL
        window.URL.revokeObjectURL(url);

        this.generationMessage.set('PDF généré et téléchargé avec succès !');
      }
      
      // Effacer le message après 3 secondes
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);

    } catch (error) {
      console.error('Error generating PDF:', error);
      this.generationMessage.set('Erreur lors de la génération du PDF');
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);
    }
  }

  async deleteAssignment(assignmentId: string): Promise<void> {
    // Demander confirmation à l'utilisateur
    const confirmed = confirm(
      'Êtes-vous sûr de vouloir supprimer cette assignation ?\n\nCette action est irréversible.'
    );

    if (!confirmed) {
      return;
    }

    this.deletionLoading.set(true);

    try {
      const success = await this.assignmentService.deleteAssignment(assignmentId).toPromise();

      if (success) {
        console.log('Assignation supprimée avec succès');
        
        // Rafraîchir la liste des assignations
        await this.loadData();
      }

    } catch (error) {
      console.error('Error deleting assignment:', error);
      alert('Erreur lors de la suppression de l\'assignation.');
    } finally {
      this.deletionLoading.set(false);
    }
  }
}