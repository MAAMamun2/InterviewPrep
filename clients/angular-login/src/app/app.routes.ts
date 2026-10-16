import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

// LAZY LOADING via loadComponent: each route's component (and everything it imports) is only
// fetched when the user navigates there, instead of all being bundled into the initial download --
// a real answer to "how would you keep initial bundle size down in a bigger Angular app."
export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'members',
    loadComponent: () => import('./features/members/members.component').then((m) => m.MembersComponent),
    canActivate: [authGuard],
  },
];
