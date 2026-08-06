import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { AuthApi } from '../../../core/auth/auth.api';

@Component({
    selector: 'app-register-page',
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    templateUrl: './register-page.html',
    styleUrl: './register-page.css',
})
export class RegisterPage
{
    private readonly formBuilder = inject(FormBuilder);
    private readonly authApi = inject(AuthApi);
    private readonly router = inject(Router);

    protected readonly isSubmitting = signal(false);
    protected readonly errorMessage = signal<string | null>(null);
    protected readonly successMessage = signal<string | null>(null);

    protected readonly form = this.formBuilder.nonNullable.group({
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        confirmPassword: ['', [Validators.required]],
        firstName: ['', [Validators.required, Validators.maxLength(100)]],
        lastName: ['', [Validators.required, Validators.maxLength(100)]],
    });

    protected submit(): void
    {
        if (this.form.invalid)
        {
            this.form.markAllAsTouched();
            return;
        }

        const password = this.form.controls.password.value;
        const confirmPassword = this.form.controls.confirmPassword.value;

        if (password !== confirmPassword)
        {
            this.errorMessage.set('Passwords do not match.');
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set(null);
        this.successMessage.set(null);

        this.authApi.register(this.form.getRawValue()).subscribe({
            next: (response) =>
            {
                localStorage.setItem('authToken', response.token);
                localStorage.setItem('authExpiresAtUtc', response.expiresAtUtc);
                localStorage.setItem('authEmail', response.email);

                this.successMessage.set(`Account created successfully for ${ response.email }.`);
                this.form.reset();

                window.setTimeout(() =>
                {
                    this.router.navigateByUrl('/');
                }, 800);
            },
            error: (error: HttpErrorResponse) =>
            {
                const apiErrors = error.error?.errors;
                if (Array.isArray(apiErrors) && apiErrors.length > 0)
                {
                    this.errorMessage.set(apiErrors.join(', '));
                    return;
                }

                this.errorMessage.set(error.error?.error ?? 'Unable to create account. Please try again.');
            },
            complete: () =>
            {
                this.isSubmitting.set(false);
            },
        });
    }
}
