import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserDto } from '../models/api.models';

interface Session {
  token: string;
  expiresAt: string;
  user: UserDto;
}

/**
 * Holds the signed-in session.
 *
 * The token is kept in sessionStorage (not localStorage): it survives a page refresh,
 * but is gone when the tab is closed, so a user coming back later signs in again.
 * Any script on the page could still read it (XSS), which is the accepted trade-off
 * for a bearer-token SPA; the short token lifetime limits the damage.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private static readonly storageKey = 'takwene.session';

  private readonly http = inject(HttpClient);
  private readonly session = signal<Session | null>(AuthService.restore());
  private expiryTimer?: ReturnType<typeof setTimeout>;

  readonly user = computed(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  constructor() {
    const session = this.session();
    if (session) this.scheduleExpiry(session.expiresAt);
  }

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(tap((response) => this.start(response)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/register`, request)
      .pipe(tap((response) => this.start(response)));
  }

  logout(): void {
    clearTimeout(this.expiryTimer);
    this.session.set(null);
    try {
      sessionStorage.removeItem(AuthService.storageKey);
    } catch {
      // Storage can be unavailable (e.g. blocked by the browser); the in-memory session is already cleared.
    }
  }

  private start(response: AuthResponse): void {
    const session: Session = { token: response.token, expiresAt: response.expiresAt, user: response.user };
    this.session.set(session);
    try {
      sessionStorage.setItem(AuthService.storageKey, JSON.stringify(session));
    } catch {
      // Without storage the user simply signs in again after a refresh.
    }
    this.scheduleExpiry(session.expiresAt);
  }

  /** Signs the user out the moment the token expires, which brings the login popup back. */
  private scheduleExpiry(expiresAt: string): void {
    clearTimeout(this.expiryTimer);
    const msLeft = Date.parse(expiresAt) - Date.now();
    this.expiryTimer = setTimeout(() => this.logout(), Math.max(msLeft, 0));
  }

  private static restore(): Session | null {
    try {
      const raw = sessionStorage.getItem(AuthService.storageKey);
      if (!raw) return null;

      const session = JSON.parse(raw) as Session;
      if (!session.token || Date.parse(session.expiresAt) <= Date.now()) {
        sessionStorage.removeItem(AuthService.storageKey);
        return null;
      }
      return session;
    } catch {
      return null;
    }
  }
}
