import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tracks' },
  {
    path: 'tracks',
    title: 'Tracks · Takwene',
    loadComponent: () => import('./features/tracks/track-list/track-list').then((m) => m.TrackList),
  },
  {
    path: 'tracks/:id',
    title: 'Track · Takwene',
    loadComponent: () => import('./features/tracks/track-detail/track-detail').then((m) => m.TrackDetail),
  },
  { path: '**', redirectTo: 'tracks' },
];
