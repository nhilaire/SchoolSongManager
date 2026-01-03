import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NurseryRhymeAssignmentService } from '../../services/nursery-rhyme-assignment.service';
import { NurseryRhymeService, NurseryRhyme } from '../../services/nursery-rhyme.service';
import { ThemeService, Theme } from '../../services/theme.service';
import { 
  CreateAssignmentRequest,
  NurseryRhymeAssignment,
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
  schoolYears = signal<string[]>([]);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  // Section de génération
  allNurseryRhymes = signal<NurseryRhyme[]>([]);
  allAssignments = signal<NurseryRhymeAssignment[]>([]);
  allThemes = signal<Theme[]>([]);
  targetSchoolYear = '';
  targetPeriod = '';
  selectedRhymeIds = signal<string[]>([]);
  generationLoading = signal<boolean>(false);
  generationMessage = signal<string>('');

  // Filtres
  selectedFilters = signal<string[]>([]);
  selectedYears = signal<string[]>([]);
  selectedPeriods = signal<string[]>([]);
  selectedThemes = signal<string[]>([]);

  // Expose constants to template
  schoolPeriods = SCHOOL_PERIODS;
  periodLabels = PERIOD_LABELS;

  constructor(
    private assignmentService: NurseryRhymeAssignmentService,
    private nurseryRhymeService: NurseryRhymeService,
    private themeService: ThemeService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    this.loading.set(true);
    this.error.set(null);

    try {
      // Charger les années scolaires
      const schoolYears = await this.assignmentService.getAvailableSchoolYears().toPromise();
      this.schoolYears.set(schoolYears || []);

      // Charger toutes les comptines
      const nurseryRhymes = await this.nurseryRhymeService.getAllRhymes().toPromise();
      this.allNurseryRhymes.set(nurseryRhymes || []);

      // Charger toutes les assignations existantes
      const assignments = await this.assignmentService.getAllAssignments().toPromise();
      this.allAssignments.set(assignments || []);

      // Charger tous les thèmes
      const themes = await this.themeService.getAllThemes().toPromise();
      this.allThemes.set(themes || []);

    } catch (error) {
      console.error('Error loading data:', error);
      this.error.set('Erreur lors du chargement des données');
    } finally {
      this.loading.set(false);
    }
  }

  getPeriodLabel(period: string): string {
    return this.periodLabels[period as SchoolPeriod] || period;
  }

  // Méthodes pour la sélection des comptines
  isRhymeSelected(rhymeId: string): boolean {
    return this.selectedRhymeIds().includes(rhymeId);
  }

  toggleRhymeSelection(rhymeId: string, event: Event) {
    const target = event.target as HTMLInputElement;
    const currentSelected = this.selectedRhymeIds();
    
    if (target.checked) {
      // Ajouter à la sélection
      this.selectedRhymeIds.set([...currentSelected, rhymeId]);
    } else {
      // Retirer de la sélection
      this.selectedRhymeIds.set(currentSelected.filter(id => id !== rhymeId));
    }
  }

  getImageUrl(imageFileName: string): string {
    // Utiliser le service pour construire l'URL de l'image
    return this.nurseryRhymeService.getImageUrl(imageFileName);
  }

  async saveAndGeneratePdf(): Promise<void> {
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

      this.generationMessage.set('Génération du PDF...');

      // Générer le PDF
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
      }

      // Réinitialiser la sélection
      this.selectedRhymeIds.set([]);
      this.targetSchoolYear = '';
      this.targetPeriod = '';

      // Recharger les données pour mettre à jour l'historique
      await this.loadData();

      this.generationMessage.set('Assignations enregistrées et PDF généré avec succès !');
      
      // Effacer le message après 3 secondes
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);

    } catch (error) {
      console.error('Error saving assignments and generating PDF:', error);
      this.generationMessage.set('Erreur lors de l\'opération');
      setTimeout(() => {
        this.generationLoading.set(false);
        this.generationMessage.set('');
      }, 3000);
    }
  }

  getAssignmentHistory(nurseryRhymeId: string): { year: string; periods: string[] }[] {
    const assignments = this.allAssignments();
    return assignments
      .filter(a => a.nurseryRhymeId === nurseryRhymeId)
      .reduce((acc: { year: string; periods: string[] }[], assignment) => {
        const existing = acc.find(item => item.year === assignment.schoolYear);
        if (existing) {
          if (!existing.periods.includes(assignment.period)) {
            existing.periods.push(assignment.period);
          }
        } else {
          acc.push({ year: assignment.schoolYear, periods: [assignment.period] });
        }
        return acc;
      }, [])
      .sort((a, b) => a.year.localeCompare(b.year));
  }

  // Méthodes de filtrage
  getAvailableYears(): string[] {
    const assignments = this.allAssignments();
    const years = new Set<string>();
    
    assignments.forEach(assignment => {
      years.add(assignment.schoolYear);
    });
    
    return Array.from(years).sort();
  }

  getAvailableFilters(): { key: string; year: string; period: string }[] {
    const assignments = this.allAssignments();
    const filters = new Map<string, { year: string; period: string }>();

    assignments.forEach(assignment => {
      const key = `${assignment.schoolYear}_${assignment.period}`;
      if (!filters.has(key)) {
        filters.set(key, { year: assignment.schoolYear, period: assignment.period });
      }
    });

    return Array.from(filters.entries()).map(([key, value]) => ({
      key,
      ...value
    })).sort((a, b) => {
      if (a.year !== b.year) {
        return a.year.localeCompare(b.year);
      }
      return a.period.localeCompare(b.period);
    });
  }

  isFilterSelected(filterKey: string): boolean {
    return this.selectedFilters().includes(filterKey);
  }

  isYearFilterSelected(year: string): boolean {
    return this.selectedYears().includes(year);
  }

  isPeriodFilterSelected(period: string): boolean {
    return this.selectedPeriods().includes(period);
  }

  isThemeFilterSelected(themeId: string): boolean {
    return this.selectedThemes().includes(themeId);
  }

  toggleFilter(filterKey: string): void {
    const currentFilters = this.selectedFilters();
    if (currentFilters.includes(filterKey)) {
      this.selectedFilters.set(currentFilters.filter(f => f !== filterKey));
    } else {
      this.selectedFilters.set([...currentFilters, filterKey]);
    }
  }

  toggleYearFilter(year: string): void {
    const currentYears = this.selectedYears();
    if (currentYears.includes(year)) {
      this.selectedYears.set(currentYears.filter(y => y !== year));
    } else {
      this.selectedYears.set([...currentYears, year]);
    }
  }

  togglePeriodFilter(period: string): void {
    const currentPeriods = this.selectedPeriods();
    if (currentPeriods.includes(period)) {
      this.selectedPeriods.set(currentPeriods.filter(p => p !== period));
    } else {
      this.selectedPeriods.set([...currentPeriods, period]);
    }
  }

  toggleThemeFilter(themeId: string): void {
    const currentThemes = this.selectedThemes();
    if (currentThemes.includes(themeId)) {
      this.selectedThemes.set(currentThemes.filter(t => t !== themeId));
    } else {
      this.selectedThemes.set([...currentThemes, themeId]);
    }
  }

  clearFilters(): void {
    this.selectedFilters.set([]);
    this.selectedYears.set([]);
    this.selectedPeriods.set([]);
    this.selectedThemes.set([]);
  }

  getFilteredRhymes(): NurseryRhyme[] {
    const allRhymes = this.allNurseryRhymes();
    const filters = this.selectedFilters();
    const selectedYears = this.selectedYears();
    const selectedPeriods = this.selectedPeriods();
    const selectedThemes = this.selectedThemes();

    let result: NurseryRhyme[];

    // Si aucun filtre n'est actif, afficher toutes les comptines
    if (filters.length === 0 && selectedYears.length === 0 && selectedPeriods.length === 0 && selectedThemes.length === 0) {
      result = [...allRhymes];
    } else {
      result = allRhymes.filter(rhyme => {
        const history = this.getAssignmentHistory(rhyme.id);

        // Si le filtre "jamais utilisées" est sélectionné et que la comptine n'a pas d'historique
        if (filters.includes('never-used') && history.length === 0) {
          // Vérifier aussi les filtres de thèmes même pour les comptines jamais utilisées
          if (selectedThemes.length > 0) {
            return rhyme.themeIds.some(themeId => selectedThemes.includes(themeId));
          }
          return true;
        }

        // Si la comptine n'a pas d'historique et que d'autres filtres sont actifs (hors thèmes)
        if (history.length === 0 && (selectedYears.length > 0 || selectedPeriods.length > 0)) {
          return false;
        }

        // Vérifier les filtres d'années
        const yearMatch = selectedYears.length === 0 || history.some(h => selectedYears.includes(h.year));

        // Vérifier les filtres de périodes
        const periodMatch = selectedPeriods.length === 0 || history.some(h =>
          h.periods.some(period => selectedPeriods.includes(period))
        );

        // Vérifier les filtres de thèmes
        const themeMatch = selectedThemes.length === 0 || rhyme.themeIds.some(themeId => selectedThemes.includes(themeId));

        return yearMatch && periodMatch && themeMatch;
      });
    }

    // Trier par ordre alphabétique (insensible à la casse)
    return result.sort((a, b) => a.title.localeCompare(b.title, 'fr', { sensitivity: 'base' }));
  }

  getThemeById(themeId: string): Theme | undefined {
    return this.allThemes().find(theme => theme.id === themeId);
  }

  isRecentlyUsed(nurseryRhymeId: string): boolean {
    const history = this.getAssignmentHistory(nurseryRhymeId);
    if (history.length === 0) {
      return false;
    }

    const currentYear = new Date().getFullYear();
    const twoYearsAgo = currentYear - 2;

    return history.some(h => {
      // Extraire l'année de début de l'année scolaire (ex: "2023/2024" -> 2023)
      const yearParts = h.year.split('/');
      const startYear = parseInt(yearParts[0], 10);
      return startYear >= twoYearsAgo;
    });
  }

  getCardBackgroundClass(nurseryRhymeId: string): string {
    return this.isRecentlyUsed(nurseryRhymeId) ? 'recently-used' : 'not-recently-used';
  }
}