import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/// FUNCTIONAL INTERCEPTOR (Angular 15+): the modern replacement for the old class-based
/// `HttpInterceptor` + `HTTP_INTERCEPTORS` multi-provider dance. This is the closest frontend
/// analogue to the backend's ASP.NET Core middleware: every outgoing request passes through here
/// first (attach the bearer token), and every response/error passes back through here too
/// (react to 401 globally, in ONE place, instead of every component catching it separately).
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.getToken();

  const authorizedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authorizedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // The token is missing/expired/invalid -- clear local state and bounce to login.
        // A `403` is deliberately NOT handled the same way: that means "you ARE authenticated,
        // you're just not ALLOWED" (e.g. a Staff token hitting an Admin-only endpoint), which
        // should surface as an in-page error, not a forced logout.
        auth.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
