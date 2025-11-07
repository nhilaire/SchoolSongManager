export interface NurseryRhyme {
  id: string;
  title: string;
  lyrics: string;
  audioUrl?: string;
  imageUrl?: string;
  theme: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface NurseryRhymeAssignment {
  id: string;
  nurseryRhymeId: string;
  schoolYear: string;
  period: string;
  assignedDate: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface NurseryRhymeAssignmentWithDetails {
  id: string;
  nurseryRhymeId: string;
  nurseryRhyme?: NurseryRhyme;
  schoolYear: string;
  period: string;
  assignedDate: Date;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateAssignmentRequest {
  nurseryRhymeId: string;
  schoolYear: string;
  period: string;
  assignedDate: Date;
}

export interface UpdateAssignmentRequest {
  nurseryRhymeId?: string;
  schoolYear?: string;
  period?: string;
  assignedDate?: Date;
}

export type SchoolPeriod = 'P1' | 'P2' | 'P3' | 'P4' | 'P5';

export const SCHOOL_PERIODS: SchoolPeriod[] = ['P1', 'P2', 'P3', 'P4', 'P5'];

export const PERIOD_LABELS: Record<SchoolPeriod, string> = {
  'P1': 'Septembre - Octobre',
  'P2': 'Novembre - Décembre',
  'P3': 'Janvier - Février',
  'P4': 'Mars - Avril',
  'P5': 'Mai - Juin'
};