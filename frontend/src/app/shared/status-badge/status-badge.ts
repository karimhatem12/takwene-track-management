import { Component, computed, input } from '@angular/core';
import { DistributionStatus, TrackStatus } from '../../core/models/api.models';

/** Coloured pill for a track status or a DSP distribution status. */
@Component({
  selector: 'app-status-badge',
  template: `<span [class]="'badge ' + tone()">{{ status() }}</span>`,
  styles: `
    .badge {
      display: inline-block;
      padding: 2px 10px;
      border-radius: 999px;
      font-size: 0.8rem;
      font-weight: 600;
      white-space: nowrap;
    }
    .draft { color: var(--status-draft); background: var(--status-draft-bg); }
    .submitted { color: var(--status-submitted); background: var(--status-submitted-bg); }
    .distributed { color: var(--status-distributed); background: var(--status-distributed-bg); }
    .rejected { color: var(--status-rejected); background: var(--status-rejected-bg); }
  `,
})
export class StatusBadge {
  readonly status = input.required<TrackStatus | DistributionStatus>();

  /** Pending looks like Submitted (waiting), Live like Distributed (done). */
  protected readonly tone = computed(() => {
    switch (this.status()) {
      case 'Draft':
        return 'draft';
      case 'Submitted':
      case 'Pending':
        return 'submitted';
      case 'Distributed':
      case 'Live':
        return 'distributed';
      case 'Rejected':
        return 'rejected';
    }
  });
}
