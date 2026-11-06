import { DatePipe, formatDate } from '@angular/common';
import { Component, computed, inject, input, LOCALE_ID, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { of, switchMap } from 'rxjs';
import { LoadState, toLoadState } from '../../../core/http/load-state';
import { AuditLogDto, TrackDetailsDto } from '../../../core/models/api.models';
import { IsrcPipe } from '../../../shared/format/isrc.pipe';
import { StatusBadge } from '../../../shared/status-badge/status-badge';
import { TrackService } from '../track.service';

/** One audit entry, ready to display. */
interface HistoryItem {
  id: number;
  timestamp: string;
  userEmail: string;
  summary: string;
  changes: { field: string; old: string | null; new: string | null }[];
}

/** Ids that only link rows together; they mean nothing to someone reading the history. */
const HIDDEN_FIELDS = new Set(['TrackId', 'DspId', 'ArtistId']);

const ISO_DATE_TIME = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/;
const ISO_DATE = /^\d{4}-\d{2}-\d{2}$/;

/** "SubmittedAt" -> "Submitted at", "Isrc" -> "ISRC". */
function toLabel(field: string): string {
  if (field === 'Isrc') return 'ISRC';
  const words = field.replace(/([a-z])([A-Z])/g, '$1 $2').toLowerCase();
  return words.charAt(0).toUpperCase() + words.slice(1);
}

const TRACK_SUMMARY: Record<string, string> = {
  Created: 'Track created',
  Updated: 'Track updated',
  Deleted: 'Track deleted',
};

/**
 * Track Detail view: the track's full info, its DSP distributions,
 * and its change history (who changed what, and when) from the audit trail.
 */
@Component({
  selector: 'app-track-detail',
  imports: [RouterLink, DatePipe, StatusBadge, IsrcPipe],
  templateUrl: './track-detail.html',
  styleUrl: './track-detail.scss',
})
export class TrackDetail {
  private readonly trackService = inject(TrackService);
  private readonly locale = inject(LOCALE_ID);
  private readonly isrc = new IsrcPipe();

  /** Bound from the :id route parameter. */
  readonly id = input.required<string>();

  /** A non-numeric id (e.g. /tracks/abc) can't exist, so it isn't sent to the API. */
  private readonly trackId = computed(() => {
    const value = Number(this.id());
    return Number.isInteger(value) && value > 0 ? value : null;
  });

  private readonly reload = signal(0);
  private readonly request = computed(() => ({ id: this.trackId(), reload: this.reload() }));

  private readonly detailState = toSignal(
    toObservable(this.request).pipe(
      switchMap(({ id }) =>
        id === null
          ? of<LoadState<TrackDetailsDto>>({ kind: 'error', message: 'Track not found.', status: 404 })
          : toLoadState(this.trackService.getById(id)),
      ),
    ),
    { initialValue: { kind: 'loading' } as LoadState<TrackDetailsDto> },
  );

  // History is loaded separately: if it fails, the track itself still shows.
  private readonly historyState = toSignal(
    toObservable(this.request).pipe(
      switchMap(({ id }) =>
        id === null
          ? of<LoadState<AuditLogDto[]>>({ kind: 'loading' })
          : toLoadState(this.trackService.getHistory(id)),
      ),
    ),
    { initialValue: { kind: 'loading' } as LoadState<AuditLogDto[]> },
  );

  protected readonly track = computed(() => {
    const s = this.detailState();
    return s.kind === 'loaded' ? s.data : null;
  });

  protected readonly error = computed(() => {
    const s = this.detailState();
    return s.kind === 'error' ? s : null;
  });

  protected readonly liveCount = computed(
    () => this.track()?.distributions.filter((d) => d.status === 'Live').length ?? 0,
  );

  protected readonly historyError = computed(() => {
    const s = this.historyState();
    return s.kind === 'error' ? s.message : null;
  });

  protected readonly history = computed<HistoryItem[] | null>(() => {
    const s = this.historyState();
    const track = this.track();
    if (s.kind !== 'loaded' || !track) return null;

    // Distribution entries only carry the distribution id; show the DSP name instead.
    const dspByDistributionId = new Map(track.distributions.map((d) => [String(d.id), d.dspName]));
    return s.data.map((log) => this.toHistoryItem(log, dspByDistributionId));
  });

  protected retry(): void {
    this.reload.update((n) => n + 1);
  }

  private toHistoryItem(log: AuditLogDto, dspByDistributionId: Map<string, string>): HistoryItem {
    let summary: string;
    if (log.entityName === 'TrackDistribution') {
      const dsp = dspByDistributionId.get(log.entityId) ?? `Distribution #${log.entityId}`;
      summary =
        log.action === 'Created'
          ? `Sent to ${dsp}`
          : log.action === 'Deleted'
            ? `${dsp} distribution removed`
            : `${dsp} distribution updated`;
    } else {
      summary = TRACK_SUMMARY[log.action] ?? log.action;
    }

    const changes = Object.entries(log.changes ?? {})
      .filter(([field]) => !HIDDEN_FIELDS.has(field))
      .map(([field, change]) => ({
        field: toLabel(field),
        old: this.formatValue(field, change.old),
        new: this.formatValue(field, change.new),
      }));

    return { id: log.id, timestamp: log.timestamp, userEmail: log.userEmail, summary, changes };
  }

  private formatValue(field: string, value: unknown): string | null {
    if (value === null || value === undefined) return null;
    if (typeof value === 'string') {
      if (field === 'Isrc') return this.isrc.transform(value);
      if (ISO_DATE_TIME.test(value)) return formatDate(value, 'medium', this.locale);
      // Plain dates are read as local dates by formatDate, so they never shift a day.
      if (ISO_DATE.test(value)) return formatDate(value, 'mediumDate', this.locale);
      return value;
    }
    return typeof value === 'object' ? JSON.stringify(value) : String(value);
  }
}
