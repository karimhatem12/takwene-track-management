import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

/**
 * - Adds "Authorization: Bearer <token>" to requests for OUR API only,
 *   so the token is never sent to any other host.
 * - A 401 from the API means the token is no longer valid: sign out,
 *   which shows the login popup again.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const isApiRequest = request.url.startsWith(environment.apiUrl);
  const isAuthEndpoint = request.url.startsWith(`${environment.apiUrl}/auth/`);
  const token = auth.token;

  const outgoing =
    isApiRequest && !isAuthEndpoint && token
      ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : request;

  return next(outgoing).pipe(
    catchError((error: unknown) => {
      // A 401 from login itself just means wrong credentials: the popup shows that message.
      if (isApiRequest && !isAuthEndpoint && error instanceof HttpErrorResponse && error.status === 401) {
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
};
