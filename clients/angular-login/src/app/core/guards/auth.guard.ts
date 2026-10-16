import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/// FUNCTIONAL ROUTE GUARD: runs before the router activates a route. Returning `true` lets
/// navigation proceed; returning a `UrlTree` (via `router.createUrlTree`) redirects instead --
/// the modern equivalent of the old class-based `CanActivate` interface. This is what actually
/// stops someone from typing `/members` into the address bar without logging in first; the
/// interceptor's 401 handling above is the SECOND line of defense (in case a token expires
/// mid-session), not the first.
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
