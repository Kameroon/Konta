/**
 * Interfaces pour l'authentification et l'utilisateur.
 * Synchronisé avec Konta.Identity.DTOs.
 */

export interface UserInfo {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    tenantId: string;
    isActive: boolean;
    createdAt: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}

/**
 * Correspond à TokenResponse du backend.
 */
export interface TokenResponse {
    token: string;
    refreshToken: string;
    expiration: string;
    user: UserInfo;
}

export interface RefreshRequest {
    token: string;
    refreshToken: string;
}

export interface UpdateProfileRequest {
    firstName: string;
    lastName: string;
}

export interface RegisterRequest {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    tenantName: string;
    plan: string;
}
