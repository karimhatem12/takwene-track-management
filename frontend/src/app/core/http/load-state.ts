import { HttpErrorResponse } from '@angular/common/http';
import { catchError, map, Observable, of, startWith } from 'rxjs';
import { toErrorMessage } from './problem-details';

/** What a page shows while it waits for one API call. */
export type LoadState<T> =
  | { kind: 'loading' }
  | { kind: 'loaded'; data: T }
  | { kind: 'error'; message: string; status: number };

/** Wraps an API call so it emits loading -> loaded | error, and never errors out itself. */
export function toLoadState<T>(source: Observable<T>): Observable<LoadState<T>> {
  return source.pipe(
    map((data): LoadState<T> => ({ kind: 'loaded', data })),
    catchError((error: unknown) =>
      of<LoadState<T>>({
        kind: 'error',
        message: toErrorMessage(error),
        status: error instanceof HttpErrorResponse ? error.status : 0,
      }),
    ),
    startWith<LoadState<T>>({ kind: 'loading' }),
  );
}
