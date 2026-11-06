import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tracks' },
  {
    path: 'tracks',
    title: 'Tracks · Takwene',
    loadComponent: () => import('./features/tracks/track-list/track-list').then((m) => m.TrackList),
  },
  { path: '**', redirectTo: 'tracks' },
];
