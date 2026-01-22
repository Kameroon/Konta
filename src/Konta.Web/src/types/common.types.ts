/**
 * Structure de réponse standard pour toutes les API du système.
 * Correspond à Konta.Shared.Responses.ApiResponse<T>
 */
export interface ApiResponse<T = any> {
    success: boolean;
    message: string;
    data: T | null;
    errors: string[] | null;
}
