import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/api.models';

/**
 * Turns an HTTP error into one readable message for the UI.
 * The API always answers with ProblemDetails, so we prefer its validation
 * messages, then its detail, then its title.
 */
export function toErrorMessage(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'Something went wrong. Please try again.';
  }

  if (error.status === 0) {
    return 'Cannot reach the API. Is the backend running on https://localhost:7142?';
  }

  const problem = error.error as ProblemDetails | null;

  if (problem?.errors) {
    return Object.values(problem.errors).flat().join(' ');
  }

  return problem?.detail ?? problem?.title ?? `Request failed (${error.status}).`;
}
