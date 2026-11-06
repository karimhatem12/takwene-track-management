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
