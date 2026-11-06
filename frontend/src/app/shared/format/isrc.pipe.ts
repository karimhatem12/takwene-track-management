import { Pipe, PipeTransform } from '@angular/core';

/** Shows a stored ISRC ("EGTKW2400001") in its readable form ("EG-TKW-24-00001"). */
@Pipe({ name: 'isrc' })
export class IsrcPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    if (!value) return '';
    const match = /^([A-Z]{2})([A-Z0-9]{3})(\d{2})(\d{5})$/.exec(value);
    return match ? match.slice(1).join('-') : value;
  }
}
