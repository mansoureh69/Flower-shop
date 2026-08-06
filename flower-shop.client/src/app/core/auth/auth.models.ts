export interface LoginRequest
{
    email: string;
    password: string;
}

export interface RegisterRequest
{
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
}

export interface AuthResponse
{
    userId: string;
    email: string;
    token: string;
    expiresAtUtc: string;
}
