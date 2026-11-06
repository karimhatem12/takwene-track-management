import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ArtistDto, SaveArtistRequest } from '../../core/models/api.models';

/** Calls the /api/artists endpoints. The auth interceptor adds the Bearer token. */
@Injectable({ providedIn: 'root' })
export class ArtistService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/artists`;

  getAll(): Observable<ArtistDto[]> {
    return this.http.get<ArtistDto[]>(this.baseUrl);
  }

  create(request: SaveArtistRequest): Observable<ArtistDto> {
    return this.http.post<ArtistDto>(this.baseUrl, request);
  }

  update(id: number, request: SaveArtistRequest): Observable<ArtistDto> {
    return this.http.put<ArtistDto>(`${this.baseUrl}/${id}`, request);
  }
}
