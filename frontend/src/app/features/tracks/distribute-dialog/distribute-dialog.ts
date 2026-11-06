import { Component, computed, inject, input, OnInit, output, signal } from '@angular/core';
import { finalize } from 'rxjs';
import { toErrorMessage } from '../../../core/http/problem-details';
import { DspDto, TrackDetailsDto } from '../../../core/models/api.models';
import { Dialog } from '../../../shared/dialog/dialog';
import { TrackService } from '../track.service';

/**
 * "Send to DSPs" popup. DSPs the track was already sent to are shown but can't be picked again
 * (each DSP gets a track once). Sending a Draft track moves it to Submitted.
 */
@Component({
  selector: 'app-distribute-dialog',
  imports: [Dialog],
  templateUrl: './distribute-dialog.html',
})
export class DistributeDialog implements OnInit {
  private readonly tracks = inject(TrackService);

  readonly track = input.required<TrackDetailsDto>();
  readonly saved = output<TrackDetailsDto>();
  readonly closed = output<void>();

  protected readonly dsps = signal<DspDto[] | null>(null);
  protected readonly selected = signal<ReadonlySet<number>>(new Set());
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly alreadySent = computed(() => new Set(this.track().distributions.map((d) => d.dspId)));

  ngOnInit(): void {
    this.tracks.getDsps().subscribe({
      next: (dsps) => this.dsps.set(dsps),
      error: (error: unknown) => this.error.set(toErrorMessage(error)),
    });
  }

  protected toggle(dspId: number, checked: boolean): void {
    this.selected.update((current) => {
      const next = new Set(current);
      if (checked) next.add(dspId);
      else next.delete(dspId);
      return next;
    });
  }

  protected submit(): void {
    if (this.selected().size === 0) {
      this.error.set('Choose at least one DSP.');
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.tracks
      .distribute(this.track().id, [...this.selected()])
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (result) => this.saved.emit(result),
        error: (error: unknown) => this.error.set(toErrorMessage(error)),
      });
  }
}
