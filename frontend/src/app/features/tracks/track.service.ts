import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuditLogDto,
  DspDto,
  SaveTrackRequest,
  TrackDetailsDto,
  TrackDto,
  TrackFilter,
  TrackStatus,
} from '../../core/models/api.models';

/** Calls the /api/tracks endpoints. The auth interceptor adds the Bearer token. */
@Injectable({ providedIn: 'root' })
export class TrackService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/tracks`;

  getAll(filter: TrackFilter = {}): Observable<TrackDto[]> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<TrackDto[]>(this.baseUrl, { params });
  }

  getById(id: number): Observable<TrackDetailsDto> {
    return this.http.get<TrackDetailsDto>(`${this.baseUrl}/${id}`);
  }

  getHistory(id: number): Observable<AuditLogDto[]> {
    return this.http.get<AuditLogDto[]>(`${this.baseUrl}/${id}/history`);
  }

  create(request: SaveTrackRequest): Observable<TrackDetailsDto> {
    return this.http.post<TrackDetailsDto>(this.baseUrl, request);
  }

  update(id: number, request: SaveTrackRequest): Observable<TrackDetailsDto> {
    return this.http.put<TrackDetailsDto>(`${this.baseUrl}/${id}`, request);
  }

  distribute(id: number, dspIds: number[]): Observable<TrackDetailsDto> {
    return this.http.post<TrackDetailsDto>(`${this.baseUrl}/${id}/distribute`, { dspIds });
  }

  changeStatus(id: number, status: TrackStatus): Observable<TrackDetailsDto> {
    return this.http.patch<TrackDetailsDto>(`${this.baseUrl}/${id}/status`, { status });
  }

  getDsps(): Observable<DspDto[]> {
    return this.http.get<DspDto[]>(`${environment.apiUrl}/dsps`);
  }

  /** The allowed genres; the API rejects any other value. */
  getGenres(): Observable<string[]> {
    return this.http.get<string[]>(`${environment.apiUrl}/genres`);
  }
}
