import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { catchError, of, switchMap } from 'rxjs';
import { LoadState, toLoadState } from '../../../core/http/load-state';
import { TRACK_STATUSES, TrackDetailsDto, TrackDto, TrackStatus } from '../../../core/models/api.models';
import { IsrcPipe } from '../../../shared/format/isrc.pipe';
import { StatusBadge } from '../../../shared/status-badge/status-badge';
import { ArtistService } from '../../artists/artist.service';
import { TrackForm } from '../track-form/track-form';
import { TrackService } from '../track.service';

/**
 * Track List view: every track with its artist, genre and status, filterable by artist and status.
 * Filters live in the URL (?status=Submitted&artistId=2), so refresh, back/forward and shared links keep them.
 */
@Component({
  selector: 'app-track-list',
  imports: [RouterLink, DatePipe, StatusBadge, IsrcPipe, TrackForm],
  templateUrl: './track-list.html',
  styleUrl: './track-list.scss',
})
export class TrackList {
  private readonly trackService = inject(TrackService);
  private readonly router = inject(Router);

  protected readonly statuses = TRACK_STATUSES;

  /** Bound from the ?status= and ?artistId= query parameters (withComponentInputBinding). */
  readonly status = input<string>();
  readonly artistId = input<string>();

  /** Unknown values in the URL (e.g. ?status=foo) are treated as "All" instead of sending them to the API. */
  protected readonly selectedStatus = computed<TrackStatus | undefined>(() => {
    const value = this.status();
    return TRACK_STATUSES.find((s) => s.toLowerCase() === value?.toLowerCase());
  });

  protected readonly selectedArtistId = computed<number | undefined>(() => {
    const value = Number(this.artistId());
    return Number.isInteger(value) && value > 0 ? value : undefined;
  });

  /** Artists for the filter drop-down. If they fail to load, the list still works without that filter. */
  protected readonly artists = toSignal(inject(ArtistService).getAll().pipe(catchError(() => of([]))), {
    initialValue: [],
  });

  protected readonly filterSummary = computed(() => {
    const artist = this.artists().find((a) => a.id === this.selectedArtistId());
    const status = this.selectedStatus();
    return (artist ? ` by ${artist.name}` : '') + (status ? ` with status ${status}` : '');
  });

  private readonly reload = signal(0);

  private readonly state = toSignal(
    toObservable(
      computed(() => ({ status: this.selectedStatus(), artistId: this.selectedArtistId(), reload: this.reload() })),
    ).pipe(
      // switchMap cancels the previous request when the filters change quickly.
      switchMap(({ status, artistId }) => toLoadState(this.trackService.getAll({ status, artistId }))),
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

  protected readonly adding = signal(false);

  protected onFilterChange(name: 'status' | 'artistId', value: string): void {
    this.router.navigate([], {
      queryParams: { [name]: value || null },
      queryParamsHandling: 'merge',
    });
  }

  protected clearFilters(): void {
    this.router.navigate([], { queryParams: {} });
  }

  /** A new track opens straight away, so it can be sent to DSPs next. */
  protected onTrackAdded(track: TrackDetailsDto): void {
    this.adding.set(false);
    this.router.navigate(['/tracks', track.id]);
  }

  protected retry(): void {
    this.reload.update((n) => n + 1);
  }
}
