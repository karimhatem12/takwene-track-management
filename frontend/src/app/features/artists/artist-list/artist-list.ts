import { Component, computed, inject, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { LoadState, toLoadState } from '../../../core/http/load-state';
import { ArtistDto } from '../../../core/models/api.models';
import { ArtistForm } from '../artist-form/artist-form';
import { ArtistService } from '../artist.service';

/** Artists page: every artist with their track count, plus add and edit. */
@Component({
  selector: 'app-artist-list',
  imports: [RouterLink, ArtistForm],
  templateUrl: './artist-list.html',
  styleUrl: './artist-list.scss',
})
export class ArtistList {
  private readonly artistService = inject(ArtistService);

  private readonly reload = signal(0);
  private readonly state = toSignal(
    toObservable(this.reload).pipe(switchMap(() => toLoadState(this.artistService.getAll()))),
    { initialValue: { kind: 'loading' } as LoadState<ArtistDto[]> },
  );

  protected readonly artists = computed(() => {
    const s = this.state();
    return s.kind === 'loaded' ? s.data : null;
  });

  protected readonly errorMessage = computed(() => {
    const s = this.state();
    return s.kind === 'error' ? s.message : null;
  });

  /** undefined = no popup, null = adding, an artist = editing that artist. */
  protected readonly editing = signal<ArtistDto | null | undefined>(undefined);

  protected onSaved(): void {
    this.editing.set(undefined);
    this.reload.update((n) => n + 1);
  }

  protected retry(): void {
    this.reload.update((n) => n + 1);
  }
}
