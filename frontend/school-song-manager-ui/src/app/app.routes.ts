import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'library',
    loadComponent: () => import('./pages/library/library.component').then(m => m.LibraryComponent)
  },
  {
    path: 'themes',
    loadComponent: () => import('./themes/themes.component').then(m => m.ThemesComponent)
  },
  {
    path: 'assignments',
    children: [
      {
        path: '',
        redirectTo: 'list',
        pathMatch: 'full'
      },
      {
        path: 'list',
        loadComponent: () => import('./components/assignment-list/assignment-list.component').then(m => m.AssignmentListComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
