import { Component, computed, inject, input, OnInit, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { toErrorMessage } from '../../../core/http/problem-details';
import { ArtistDto, TrackDetailsDto } from '../../../core/models/api.models';
import { Dialog } from '../../../shared/dialog/dialog';
import { IsrcPipe } from '../../../shared/format/isrc.pipe';
import { ArtistForm } from '../../artists/artist-form/artist-form';
import { ArtistService } from '../../artists/artist.service';
import { TrackService } from '../track.service';

/** Same check as the API: CC-XXX-YY-NNNNN, dashes and spaces optional, any case. */
const ISRC_PATTERN = /^[A-Za-z]{2}[A-Za-z0-9]{3}\d{7}$/;
function isValidIsrc(value: string): boolean {
  return ISRC_PATTERN.test(value.replace(/[-\s]/g, ''));
}

/**
 * Add / edit track popup. Pass a track to edit it, or nothing to add a new one.
 * Once a track has left Draft its artist and ISRC are locked (the API enforces the same rule),
 * because the DSPs already know the recording by them.
 */
@Component({
  selector: 'app-track-form',
  imports: [ReactiveFormsModule, Dialog, ArtistForm],
  templateUrl: './track-form.html',
})
export class TrackForm implements OnInit {
  private readonly tracks = inject(TrackService);
  private readonly artistService = inject(ArtistService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly isrcPipe = new IsrcPipe();

  readonly track = input<TrackDetailsDto | null>(null);
  readonly saved = output<TrackDetailsDto>();
  readonly closed = output<void>();

  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);
  protected readonly artists = signal<ArtistDto[] | null>(null);
  protected readonly artistsError = signal<string | null>(null);
  protected readonly addingArtist = signal(false);
  protected readonly genres = signal<string[] | null>(null);
  protected readonly genresError = signal<string | null>(null);

  /** Artist and ISRC can only change while the track is a Draft. */
  protected readonly identityLocked = computed(() => {
    const track = this.track();
    return track !== null && track.status !== 'Draft';
  });

  protected readonly form = this.fb.group({
    artistId: [0, [Validators.required, Validators.min(1)]],
    title: ['', [Validators.required, Validators.maxLength(200)]],
    isrc: ['', [Validators.required, (c: { value: string }) => (isValidIsrc(c.value) ? null : { isrc: true })]],
    releaseDate: ['', Validators.required],
    genre: ['', Validators.required],
  });

  ngOnInit(): void {
    const track = this.track();
    if (track) {
      this.form.setValue({
        artistId: track.artistId,
        title: track.title,
        isrc: this.isrcPipe.transform(track.isrc),
        releaseDate: track.releaseDate,
        genre: track.genre,
      });
      if (this.identityLocked()) {
        this.form.controls.artistId.disable();
        this.form.controls.isrc.disable();
      }
    }
    this.loadArtists();
    this.loadGenres();
  }

  protected onArtistAdded(artist: ArtistDto): void {
    this.addingArtist.set(false);
    this.artists.update((list) => [...(list ?? []), artist].sort((a, b) => a.name.localeCompare(b.name)));
    this.form.controls.artistId.setValue(artist.id);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // getRawValue includes the locked fields, which the API expects unchanged.
    const request = this.form.getRawValue();
    const track = this.track();

    this.submitting.set(true);
    this.serverError.set(null);
    (track ? this.tracks.update(track.id, request) : this.tracks.create(request))
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (result) => this.saved.emit(result),
        error: (error: unknown) => this.serverError.set(toErrorMessage(error)),
      });
  }

  private loadGenres(): void {
    this.tracks.getGenres().subscribe({
      next: (genres) => {
        // An older track could have a genre that is no longer in the list: keep it selectable
        // so opening the form doesn't silently change it (the API will ask for a valid one on save).
        const current = this.track()?.genre;
        this.genres.set(current && !genres.includes(current) ? [current, ...genres] : genres);
      },
      error: (error: unknown) => this.genresError.set(toErrorMessage(error)),
    });
  }

  private loadArtists(): void {
    this.artistService.getAll().subscribe({
      next: (artists) => this.artists.set(artists),
      error: (error: unknown) => this.artistsError.set(toErrorMessage(error)),
    });
  }
}
