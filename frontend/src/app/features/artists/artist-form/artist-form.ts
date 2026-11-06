import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { toErrorMessage } from '../../../core/http/problem-details';
import { ArtistDto } from '../../../core/models/api.models';
import { Dialog } from '../../../shared/dialog/dialog';
import { ArtistService } from '../artist.service';

/**
 * Add / edit artist popup. Pass an artist to edit it, or nothing to add a new one.
 * The rules mirror the API's validators; the API still checks everything (e.g. unique email).
 */
@Component({
  selector: 'app-artist-form',
  imports: [ReactiveFormsModule, Dialog],
  templateUrl: './artist-form.html',
})
export class ArtistForm implements OnInit {
  private readonly artists = inject(ArtistService);
  private readonly fb = inject(NonNullableFormBuilder);

  readonly artist = input<ArtistDto | null>(null);
  readonly saved = output<ArtistDto>();
  readonly closed = output<void>();

  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    country: ['', [Validators.required, Validators.pattern(/^[A-Za-z]{2}$/)]],
  });

  ngOnInit(): void {
    const artist = this.artist();
    if (artist) this.form.setValue({ name: artist.name, email: artist.email, country: artist.country });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = { ...value, country: value.country.toUpperCase() };
    const artist = this.artist();

    this.submitting.set(true);
    this.serverError.set(null);
    (artist ? this.artists.update(artist.id, request) : this.artists.create(request))
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (result) => this.saved.emit(result),
        error: (error: unknown) => this.serverError.set(toErrorMessage(error)),
      });
  }
}
