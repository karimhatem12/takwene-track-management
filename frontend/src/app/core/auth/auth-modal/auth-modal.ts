import { AfterViewInit, Component, ElementRef, inject, signal, viewChild } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { toErrorMessage } from '../../http/problem-details';
import { AuthService } from '../auth.service';

type Mode = 'login' | 'register';

/**
 * Sign-in / register popup. It is shown whenever nobody is signed in and cannot be
 * dismissed: the app is only usable after signing in.
 * Client-side rules mirror the API's validators so most mistakes are caught before
 * a request is sent; the API still validates everything.
 */
@Component({
  selector: 'app-auth-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './auth-modal.html',
  styleUrl: './auth-modal.scss',
})
export class AuthModal implements AfterViewInit {
  private readonly auth = inject(AuthService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly firstField = viewChild<ElementRef<HTMLInputElement>>('firstField');

  protected readonly mode = signal<Mode>('login');
  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected readonly loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  protected readonly registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: [
      '',
      [
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(100),
        Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).+$/),
      ],
    ],
  });

  ngAfterViewInit(): void {
    this.focusFirstField();
  }

  protected switchTo(mode: Mode): void {
    this.mode.set(mode);
    this.serverError.set(null);
    setTimeout(() => this.focusFirstField());
  }

  protected submitLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    this.send(() => this.auth.login(this.loginForm.getRawValue()));
  }

  protected submitRegister(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }
    this.send(() => this.auth.register(this.registerForm.getRawValue()));
  }

  private send(request: () => ReturnType<AuthService['login']>): void {
    this.submitting.set(true);
    this.serverError.set(null);
    request()
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        // On success AuthService stores the session and the app hides this popup.
        error: (error: unknown) => this.serverError.set(toErrorMessage(error)),
      });
  }

  private focusFirstField(): void {
    this.firstField()?.nativeElement.focus();
  }
}
