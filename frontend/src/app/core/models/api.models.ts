// Shapes of the API's JSON responses (see backend Application/*Dtos.cs).

export type TrackStatus = 'Draft' | 'Submitted' | 'Distributed';
export type DistributionStatus = 'Pending' | 'Live' | 'Rejected';

export interface UserDto {
  id: number;
  fullName: string;
  email: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string; // ISO date-time, UTC
  user: UserDto;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

/** RFC 7807 error body returned by the API for every error. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  /** Present on 400 validation errors: field name -> messages. */
  errors?: Record<string, string[]>;
}

export const TRACK_STATUSES: readonly TrackStatus[] = ['Draft', 'Submitted', 'Distributed'];

/** Row in GET /api/tracks. */
export interface TrackDto {
  id: number;
  title: string;
  artistId: number;
  artistName: string;
  isrc: string;
  releaseDate: string; // yyyy-MM-dd
  genre: string;
  status: TrackStatus;
}

/** Optional filters for GET /api/tracks; empty values are not sent. */
export interface TrackFilter {
  artistId?: number;
  genre?: string;
  status?: TrackStatus;
}

/** GET /api/tracks/{id}: the track plus where it has been distributed. */
export interface TrackDetailsDto extends TrackDto {
  distributions: TrackDistributionDto[];
  createdAt: string; // ISO date-time, UTC
  createdBy: string;
  updatedAt: string | null;
  updatedBy: string | null;
}

export interface TrackDistributionDto {
  id: number;
  dspId: number;
  dspName: string;
  submittedAt: string; // ISO date-time, UTC
  status: DistributionStatus;
}

export type AuditAction = 'Created' | 'Updated' | 'Deleted' | 'Registered' | 'LoggedIn';

/** One changed field in an audit entry: old is null on create, new is null on delete. */
export interface FieldChange {
  old: unknown;
  new: unknown;
}

/** GET /api/tracks/{id}/history: one audit entry, newest first. */
export interface AuditLogDto {
  id: number;
  timestamp: string; // ISO date-time, UTC
  userId: number | null;
  userEmail: string;
  action: AuditAction;
  entityName: string; // "Track" or "TrackDistribution"
  entityId: string;
  changes: Record<string, FieldChange> | null;
}

/** GET /api/artists */
export interface ArtistDto {
  id: number;
  name: string;
  email: string;
  country: string; // ISO 3166-1 alpha-2, upper case
  trackCount: number;
}

/** Body of POST /api/artists and PUT /api/artists/{id}. */
export interface SaveArtistRequest {
  name: string;
  email: string;
  country: string;
}

/** Body of POST /api/tracks and PUT /api/tracks/{id}. */
export interface SaveTrackRequest {
  artistId: number;
  title: string;
  isrc: string;
  releaseDate: string; // yyyy-MM-dd
  genre: string;
}

/** GET /api/dsps */
export interface DspDto {
  id: number;
  name: string;
}

/** The status a track can move to next, or null when it is already Distributed. */
export function nextStatus(status: TrackStatus): TrackStatus | null {
  const index = TRACK_STATUSES.indexOf(status);
  return TRACK_STATUSES[index + 1] ?? null;
}
