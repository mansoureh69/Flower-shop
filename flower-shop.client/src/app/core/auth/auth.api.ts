import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { AuthResponse, LoginRequest, RegisterRequest } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApi
{
    private readonly http = inject(HttpClient);

    login(request: LoginRequest)
    {
        return this.http.post<AuthResponse>('/api/auth/login', request);
    }

    register(request: RegisterRequest)
    {
        return this.http.post<AuthResponse>('/api/auth/register', request);
    }
}
