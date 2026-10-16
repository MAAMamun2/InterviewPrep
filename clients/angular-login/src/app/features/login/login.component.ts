import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

/// STANDALONE COMPONENT (Angular 22 default, no NgModule): declares its own `imports` array.
/// REACTIVE FORMS (not template-driven): form state/validation lives in the component class as a
/// typed FormGroup, which is far easier to unit-test than template-driven [(ngModel)] bindings --
/// a common "which form approach and why" interview question.
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  // Plain signals for UI-only state (loading/error) -- no need for the full AuthState union here
  // since the component only cares about its OWN submit lifecycle, not the app-wide auth state.
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // inject() instead of constructor injection: lets `form` be built right here as a field
  // initializer (constructor-injected `fb` wouldn't exist yet at this point -- field initializers
  // run before the constructor body).
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(4)]],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched(); // forces validation messages to show even if untouched
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe((response) => {
      this.isLoading.set(false);
      if (response) {
        this.router.navigate(['/members']);
      } else {
        this.errorMessage.set('Invalid username or password.');
      }
    });
  }

  // Convenience getters keep the template free of repeated `form.controls.x` chains.
  get username() {
    return this.form.controls.username;
  }

  get password() {
    return this.form.controls.password;
  }
}
