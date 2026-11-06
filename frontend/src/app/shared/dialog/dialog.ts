import { AfterViewInit, Component, ElementRef, input, OnDestroy, output, viewChild } from '@angular/core';

/**
 * Popup shell used by every form in the app, built on the native <dialog> element:
 * showModal() gives the backdrop, keeps keyboard focus inside the popup and closes on Esc.
 *
 * Usage: render it with @if, put the body inside, and buttons inside an element with [dialog-actions].
 */
@Component({
  selector: 'app-dialog',
  template: `
    <dialog #dialog [attr.aria-labelledby]="titleId" (cancel)="onCancel($event)" (click)="onBackdropClick($event)">
      <div class="panel">
        <header>
          <h2 [id]="titleId">{{ title() }}</h2>
          <button type="button" class="close" aria-label="Close" [disabled]="busy()" (click)="closed.emit()">×</button>
        </header>
        <ng-content />
        <footer>
          <ng-content select="[dialog-actions]" />
        </footer>
      </div>
    </dialog>
  `,
  styleUrl: './dialog.scss',
})
export class Dialog implements AfterViewInit, OnDestroy {
  private static nextId = 0;

  readonly title = input.required<string>();
  /** While true (e.g. a request is running) the popup can't be closed. */
  readonly busy = input(false);
  readonly closed = output<void>();

  protected readonly titleId = `dialog-title-${Dialog.nextId++}`;
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  ngAfterViewInit(): void {
    this.dialog().nativeElement.showModal();
  }

  ngOnDestroy(): void {
    this.dialog().nativeElement.close();
  }

  /** Esc: let the parent decide (it removes the dialog with @if), and never close mid-request. */
  protected onCancel(event: Event): void {
    event.preventDefault();
    if (!this.busy()) this.closed.emit();
  }

  /** A click on the dialog element itself (not its panel) is a click on the backdrop. */
  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === this.dialog().nativeElement && !this.busy()) this.closed.emit();
  }
}
