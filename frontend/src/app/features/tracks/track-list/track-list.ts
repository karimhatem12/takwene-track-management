import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { LoadState, toLoadState } from '../../../core/http/load-state';
import { TRACK_STATUSES, TrackDto, TrackStatus } from '../../../core/models/api.models';
import { StatusBadge } from '../../../shared/status-badge/status-badge';
import { TrackService } from '../track.service';

/**
 * Track List view: every track with its artist, genre and status, filterable by status.
 * The filter lives in the URL (?status=Submitted), so refresh, back/forward and shared links keep it.
 */
@Component({
  selector: 'app-track-list',
  imports: [RouterLink, DatePipe, StatusBadge],
  templateUrl: './track-list.html',
  styleUrl: './track-list.scss',
})
export class TrackList {
  private readonly trackService = inject(TrackService);
  private readonly router = inject(Router);

  protected readonly statuses = TRACK_STATUSES;

  /** Bound from the ?status= query parameter (withComponentInputBinding). */
  readonly status = input<string>();

  /** Unknown values in the URL (e.g. ?status=foo) are treated as "All" instead of sending them to the API. */
  protected readonly selectedStatus = computed<TrackStatus | undefined>(() => {
    const value = this.status();
    return TRACK_STATUSES.find((s) => s.toLowerCase() === value?.toLowerCase());
  });

  private readonly reload = signal(0);

  private readonly state = toSignal(
    toObservable(computed(() => ({ status: this.selectedStatus(), reload: this.reload() }))).pipe(
      // switchMap cancels the previous request when the filter changes quickly.
      switchMap(({ status }) => toLoadState(this.trackService.getAll({ status }))),
    ),
    { initialValue: { kind: 'loading' } as LoadState<TrackDto[]> },
  );

  protected readonly tracks = computed(() => {
    const s = this.state();
    return s.kind === 'loaded' ? s.data : null;
  });

  protected readonly errorMessage = computed(() => {
    const s = this.state();
    return s.kind === 'error' ? s.message : null;
  });

  protected onStatusChange(value: string): void {
    this.router.navigate([], {
      queryParams: { status: value || null },
      queryParamsHandling: 'merge',
    });
  }

  protected retry(): void {
    this.reload.update((n) => n + 1);
  }
}
