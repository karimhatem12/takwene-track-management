import { DatePipe, formatDate } from '@angular/common';
import { Component, computed, inject, input, LOCALE_ID, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize, of, skip, switchMap } from 'rxjs';
import { LoadState, toLoadState } from '../../../core/http/load-state';
import { toErrorMessage } from '../../../core/http/problem-details';
import { AuditLogDto, nextStatus, TrackDetailsDto, TrackStatus } from '../../../core/models/api.models';
import { Dialog } from '../../../shared/dialog/dialog';
import { IsrcPipe } from '../../../shared/format/isrc.pipe';
import { StatusBadge } from '../../../shared/status-badge/status-badge';
import { DistributeDialog } from '../distribute-dialog/distribute-dialog';
import { TrackForm } from '../track-form/track-form';
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
  imports: [RouterLink, DatePipe, StatusBadge, IsrcPipe, Dialog, TrackForm, DistributeDialog],
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

  /** silent = refresh after an action: keep showing the page instead of "Loading…". */
  private readonly reload = signal({ count: 0, silent: false });
  private readonly request = computed(() => ({ id: this.trackId(), ...this.reload() }));

  private readonly detailState = toSignal(
    toObservable(this.request).pipe(
      switchMap(({ id, silent }) =>
        id === null
          ? of<LoadState<TrackDetailsDto>>({ kind: 'error', message: 'Track not found.', status: 404 })
          : toLoadState(this.trackService.getById(id)).pipe(skip(silent ? 1 : 0)),
      ),
    ),
    { initialValue: { kind: 'loading' } as LoadState<TrackDetailsDto> },
  );

  // History is loaded separately: if it fails, the track itself still shows.
  private readonly historyState = toSignal(
    toObservable(this.request).pipe(
      switchMap(({ id, silent }) =>
        id === null
          ? of<LoadState<AuditLogDto[]>>({ kind: 'loading' })
          : toLoadState(this.trackService.getHistory(id)).pipe(skip(silent ? 1 : 0)),
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

  // ---- Actions: edit, send to DSPs, move status forward ----

  protected readonly editing = signal(false);
  protected readonly distributing = signal(false);
  protected readonly confirmingStatus = signal(false);
  protected readonly statusSubmitting = signal(false);
  protected readonly statusError = signal<string | null>(null);

  protected readonly nextStatus = computed(() => {
    const track = this.track();
    return track ? nextStatus(track.status) : null;
  });

  /** Moving to Distributed needs a Live DSP; Live/Rejected come from the DSPs, not from this app. */
  protected readonly blockedReason = computed(() =>
    this.nextStatus() === 'Distributed' && this.liveCount() === 0
      ? 'A track can only be marked Distributed once it is Live on at least one DSP. ' +
        'Live and Rejected are reported by the DSPs, so this button will work once one of them goes live.'
      : null,
  );

  protected retry(): void {
    this.reload.update((r) => ({ count: r.count + 1, silent: false }));
  }

  /** After a successful action: refresh the track and its history without a loading flash. */
  protected afterAction(): void {
    this.editing.set(false);
    this.distributing.set(false);
    this.confirmingStatus.set(false);
    this.reload.update((r) => ({ count: r.count + 1, silent: true }));
  }

  protected openStatusDialog(): void {
    this.statusError.set(null);
    this.confirmingStatus.set(true);
  }

  protected changeStatus(status: TrackStatus): void {
    const track = this.track();
    if (!track) return;

    this.statusSubmitting.set(true);
    this.statusError.set(null);
    this.trackService
      .changeStatus(track.id, status)
      .pipe(finalize(() => this.statusSubmitting.set(false)))
      .subscribe({
        next: () => this.afterAction(),
        error: (error: unknown) => this.statusError.set(toErrorMessage(error)),
      });
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
