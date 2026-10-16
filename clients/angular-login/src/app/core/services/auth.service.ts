import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, finalize, of, tap } from 'rxjs';
import { LoginRequest, LoginResponse } from '../models/auth.model';

const TOKEN_STORAGE_KEY = 'interviewprep.token';

/// SERVICE + DEPENDENCY INJECTION: `@Injectable({ providedIn: 'root' })` registers ONE singleton
/// instance app-wide via Angular's DI container -- any component that injects AuthService gets
/// the same instance, so auth state is naturally shared without a separate state-management library.
@Injectable({ providedIn: 'root' })
export class AuthService {
  // SIGNALS: Angular 22's reactive primitive. `_token` is the private, writable source of truth;
  // everything else is derived. Unlike a plain class field, reading a signal inside a template or
  // inside `computed()` automatically subscribes that consumer to future changes -- no manual
  // subscribe/unsubscribe, no OnPush change-detection gotchas.
  private readonly _token = signal<string | null>(this.readStoredToken());
  private readonly _username = signal<string | null>(null);
  private readonly _role = signal<'Admin' | 'Staff' | null>(null);

  // COMPUTED SIGNAL: derives from `_token` and automatically recalculates whenever it changes --
  // the Angular analogue of a memoized selector. Never set directly; it's read-only on purpose.
  readonly isAuthenticated = computed(() => this._token() !== null);
  readonly username = this._username.asReadonly();
  readonly role = this._role.asReadonly();

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse | null> {
    // RxJS operators chained on the HttpClient Observable:
    //  - tap(): side effect (store the token) without altering the emitted value
    //  - catchError(): turn an HTTP error into a safe fallback value (null) instead of letting
    //    the Observable error out silently un-handled by whoever subscribes in the component
    //  - finalize(): runs on BOTH success and error paths, the RxJS equivalent of a `finally` block
    return this.http.post<LoginResponse>('/api/auth/login', request).pipe(
      tap((response) => this.setSession(response)),
      catchError(() => of(null)),
      finalize(() => {
        /* component owns the loading-state signal; nothing shared needed here */
      })
    );
  }

  logout(): void {
    this._token.set(null);
    this._username.set(null);
    this._role.set(null);
    localStorage.removeItem(TOKEN_STORAGE_KEY);
  }

  getToken(): string | null {
    return this._token();
  }

  private setSession(response: LoginResponse): void {
    this._token.set(response.token);
    this._username.set(response.username);
    this._role.set(response.role);
    localStorage.setItem(TOKEN_STORAGE_KEY, response.token);
  }

  private readStoredToken(): string | null {
    // localStorage is per-viewer, synchronous, and survives a refresh -- fine for a demo JWT;
    // a production app would weigh XSS exposure (readable by any script on the page) against
    // an httpOnly cookie (not readable by JS, but then needs CSRF protection instead). Worth
    // raising unprompted if asked "where would you store the token?"
    try {
      return localStorage.getItem(TOKEN_STORAGE_KEY);
    } catch {
      return null;
    }
  }
}
