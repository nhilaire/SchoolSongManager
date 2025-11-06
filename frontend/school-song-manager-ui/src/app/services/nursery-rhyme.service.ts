import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app-config';

export interface NurseryRhyme {
  id: string;
  title: string;
  imageFileName?: string;
  url?: string;
  audioFileName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateNurseryRhymeRequest {
  title: string;
  url?: string;
}

export interface UpdateNurseryRhymeRequest {
  title: string;
  url?: string;
}

@Injectable({
  providedIn: 'root',
})
export class NurseryRhymeService {
  private readonly baseUrl: string;

  constructor(private http: HttpClient, private appConfig: AppConfig) {
    this.baseUrl = this.appConfig.apiUrl;
  }

  getAllRhymes(): Observable<NurseryRhyme[]> {
    return this.http.get<NurseryRhyme[]>(`${this.baseUrl}/api/nursery-rhymes`);
  }

  getRhyme(id: string): Observable<NurseryRhyme> {
    return this.http.get<NurseryRhyme>(`${this.baseUrl}/api/nursery-rhymes/${id}`);
  }

  createRhyme(rhyme: CreateNurseryRhymeRequest, audioFile?: File | null, imageFile?: File | null): Observable<NurseryRhyme> {
    const formData = new FormData();
    formData.append('title', rhyme.title);
    
    if (rhyme.url) {
      formData.append('url', rhyme.url);
    }
    
    if (audioFile) {
      formData.append('audioFile', audioFile);
    }
    
    if (imageFile) {
      formData.append('imageFile', imageFile);
    }

    return this.http.post<NurseryRhyme>(`${this.baseUrl}/api/nursery-rhymes`, formData);
  }

  updateRhyme(id: string, rhyme: UpdateNurseryRhymeRequest, audioFile?: File | null, imageFile?: File | null): Observable<NurseryRhyme> {
    const formData = new FormData();
    formData.append('title', rhyme.title);
    
    if (rhyme.url) {
      formData.append('url', rhyme.url);
    }
    
    if (audioFile) {
      formData.append('audioFile', audioFile);
    }
    
    if (imageFile) {
      formData.append('imageFile', imageFile);
    }

    return this.http.put<NurseryRhyme>(`${this.baseUrl}/api/nursery-rhymes/${id}`, formData);
  }

  deleteRhyme(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/api/nursery-rhymes/${id}`);
  }

  getAudioUrl(fileName: string): string {
    return `${this.baseUrl}/api/nursery-rhymes/audio/${fileName}`;
  }

  getImageUrl(fileName: string): string {
    return `${this.baseUrl}/api/nursery-rhymes/images/${fileName}`;
  }
}