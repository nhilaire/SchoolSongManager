import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { NurseryRhymeAssignmentService } from '../../services/nursery-rhyme-assignment.service';
import { 
  NurseryRhymeAssignmentWithDetails,
  SCHOOL_PERIODS, 
  PERIOD_LABELS,
  SchoolPeriod 
} from '../../models/nursery-rhyme-assignment.model';

@Component({
  selector: 'app-assignment-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './assignment-list.component.html',
  styleUrl: './assignment-list.component.scss'
})
export class AssignmentListComponent implements OnInit {
  assignments = signal<NurseryRhymeAssignmentWithDetails[]>([]);
  schoolYears = signal<string[]>([]);
  selectedSchoolYear = signal<string>('');
  selectedPeriod = signal<string>('');
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  deletionLoading = signal<boolean>(false);

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
      // Trier par année scolaire puis par période
      const yearCompare = a.schoolYear.localeCompare(b.schoolYear);
      if (yearCompare !== 0) return yearCompare;
      return a.period.localeCompare(b.period);
    });
  });

  currentSchoolYear = computed(() => this.assignmentService.getCurrentSchoolYear());

  constructor(private assignmentService: NurseryRhymeAssignmentService) {}

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    this.loading.set(true);
    this.error.set(null);

    try {
      // Charger les assignations
      const assignments = await this.assignmentService.getAllAssignments().toPromise();
      this.assignments.set(assignments || []);

      // Charger les années scolaires
      const schoolYears = await this.assignmentService.getAvailableSchoolYears().toPromise();
      this.schoolYears.set(schoolYears || []);

    } catch (error) {
      console.error('Error loading data:', error);
      this.error.set('Erreur lors du chargement des données');
    } finally {
      this.loading.set(false);
    }
  }

  onSchoolYearChange(schoolYear: string) {
    this.selectedSchoolYear.set(schoolYear);
  }

  onPeriodChange(period: string) {
    this.selectedPeriod.set(period);
  }

  clearFilters() {
    this.selectedSchoolYear.set('');
    this.selectedPeriod.set('');
  }

  getPeriodLabel(period: string): string {
    return this.periodLabels[period as SchoolPeriod] || period;
  }

  formatDate(date: Date): string {
    return new Intl.DateTimeFormat('fr-FR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(date);
  }

  getYearRowClass(schoolYear: string): string {
    const currentYear = this.currentSchoolYear();
    if (schoolYear === currentYear) return 'current-year-row';
    
    const [startYear] = schoolYear.split('/').map(y => parseInt(y));
    const [currentStartYear] = currentYear.split('/').map(y => parseInt(y));
    
    if (startYear < currentStartYear) return 'past-year-row';
    if (startYear > currentStartYear) return 'future-year-row';
    
    return '';
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