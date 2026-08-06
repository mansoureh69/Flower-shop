import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AuthApi } from '../../../core/auth/auth.api';

@Component({
    selector: 'app-login-page',
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    templateUrl: './login-page.html',
    styleUrl: './login-page.css',
})
export class LoginPage
{
    private readonly formBuilder = inject(FormBuilder);
    private readonly authApi = inject(AuthApi);

    protected readonly isSubmitting = signal(false);
    protected readonly errorMessage = signal<string | null>(null);
    protected readonly successMessage = signal<string | null>(null);

    protected readonly form = this.formBuilder.nonNullable.group({
        email: ['', [Validators.required, Validators.email]],
        password: ['', Validators.required],
    });

    protected submit(): void
    {
        if (this.form.invalid)
        {
            this.form.markAllAsTouched();
            return;
        }

        this.isSubmitting.set(true);
        this.errorMessage.set(null);
        this.successMessage.set(null);

        const request = this.form.getRawValue();

        this.authApi.login(request).subscribe({
            next: (response) =>
            {
                localStorage.setItem('authToken', response.token);
                localStorage.setItem('authExpiresAtUtc', response.expiresAtUtc);
                localStorage.setItem('authEmail', response.email);

                this.successMessage.set(`Welcome back, ${ response.email }.`);
                this.form.reset();
            },
            error: (error: HttpErrorResponse) =>
            {
                this.errorMessage.set(error.error?.error ?? 'Unable to login. Please try again.');
            },
            complete: () =>
            {
                this.isSubmitting.set(false);
            },
        });
    }
}
