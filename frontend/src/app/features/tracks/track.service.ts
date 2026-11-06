import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogDto, TrackDetailsDto, TrackDto, TrackFilter } from '../../core/models/api.models';

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
}
