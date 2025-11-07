import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppConfig } from '../config/app-config';
import { 
  NurseryRhymeAssignment, 
  NurseryRhymeAssignmentWithDetails, 
  CreateAssignmentRequest,
  UpdateAssignmentRequest 
} from '../models/nursery-rhyme-assignment.model';

@Injectable({
  providedIn: 'root'
})
export class NurseryRhymeAssignmentService {
  private readonly baseUrl: string;

  constructor(
    private http: HttpClient,
    private appConfig: AppConfig
  ) {
    this.baseUrl = `${this.appConfig.apiUrl}/api/nurseryrhymeassignments`;
  }

  getAllAssignments(): Observable<NurseryRhymeAssignmentWithDetails[]> {
    return this.http.get<NurseryRhymeAssignmentWithDetails[]>(this.baseUrl)
      .pipe(
        map(assignments => this.convertDates(assignments))
      );
  }

  getAssignmentById(id: string): Observable<NurseryRhymeAssignmentWithDetails | null> {
    return this.http.get<NurseryRhymeAssignmentWithDetails>(`${this.baseUrl}/${id}`)
      .pipe(
        map(assignment => assignment ? this.convertDatesForSingle(assignment) : null)
      );
  }

  getAssignmentsBySchoolYear(schoolYear: string): Observable<NurseryRhymeAssignmentWithDetails[]> {
    return this.http.get<NurseryRhymeAssignmentWithDetails[]>(`${this.baseUrl}/school-year/${encodeURIComponent(schoolYear)}`)
      .pipe(
        map(assignments => this.convertDates(assignments))
      );
  }

  getAssignmentsByPeriod(schoolYear: string, period: string): Observable<NurseryRhymeAssignmentWithDetails[]> {
    return this.http.get<NurseryRhymeAssignmentWithDetails[]>(`${this.baseUrl}/period/${encodeURIComponent(schoolYear)}/${encodeURIComponent(period)}`)
      .pipe(
        map(assignments => this.convertDates(assignments))
      );
  }

  getAvailableSchoolYears(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/school-years`);
  }

  createAssignment(request: CreateAssignmentRequest): Observable<NurseryRhymeAssignmentWithDetails> {
    return this.http.post<NurseryRhymeAssignmentWithDetails>(this.baseUrl, request)
      .pipe(
        map(assignment => this.convertDatesForSingle(assignment))
      );
  }

  updateAssignment(id: string, request: UpdateAssignmentRequest): Observable<NurseryRhymeAssignmentWithDetails | null> {
    return this.http.put<NurseryRhymeAssignmentWithDetails>(`${this.baseUrl}/${id}`, request)
      .pipe(
        map(assignment => assignment ? this.convertDatesForSingle(assignment) : null)
      );
  }

  deleteAssignment(id: string): Observable<boolean> {
    return this.http.delete(`${this.baseUrl}/${id}`, { observe: 'response' })
      .pipe(
        map(response => response.status === 204)
      );
  }

  deleteAllAssignmentsForPeriod(schoolYear: string, period: string): Observable<{ deletedCount: number, message: string }> {
    return this.http.delete<{ deletedCount: number, message: string }>(`${this.baseUrl}/period/${encodeURIComponent(schoolYear)}/${encodeURIComponent(period)}`);
  }

  generatePdf(request: { nurseryRhymeIds: string[], schoolYear: string, period: string }): Observable<ArrayBuffer> {
    return this.http.post(`${this.baseUrl}/generate-pdf`, request, { 
      responseType: 'arraybuffer' 
    });
  }

  generateSchoolYearOptions(): string[] {
    const currentYear = new Date().getFullYear();
    const schoolYears: string[] = [];
    
    // Générer 10 années scolaires (5 passées, actuelle, 4 futures)
    for (let i = -5; i <= 4; i++) {
      const startYear = currentYear + i;
      const endYear = startYear + 1;
      schoolYears.push(`${startYear}/${endYear}`);
    }
    
    return schoolYears;
  }

  getCurrentSchoolYear(): string {
    const now = new Date();
    const currentYear = now.getFullYear();
    const currentMonth = now.getMonth(); // 0-11
    
    // L'année scolaire commence en septembre (mois 8)
    if (currentMonth >= 8) {
      // Septembre à décembre = début de l'année scolaire
      return `${currentYear}/${currentYear + 1}`;
    } else {
      // Janvier à août = fin de l'année scolaire précédente
      return `${currentYear - 1}/${currentYear}`;
    }
  }

  private convertDates(assignments: NurseryRhymeAssignmentWithDetails[]): NurseryRhymeAssignmentWithDetails[] {
    return assignments.map(assignment => this.convertDatesForSingle(assignment));
  }

  private convertDatesForSingle(assignment: NurseryRhymeAssignmentWithDetails): NurseryRhymeAssignmentWithDetails {
    return {
      ...assignment,
      assignedDate: new Date(assignment.assignedDate),
      createdAt: new Date(assignment.createdAt),
      updatedAt: new Date(assignment.updatedAt)
    };
  }
}