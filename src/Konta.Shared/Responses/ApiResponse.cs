namespace Konta.Shared.Responses;

/// <summary>
/// Structure de réponse standard pour toutes les API du système.
/// </summary>
/// <typeparam name="T">Type des données retournées.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indique si l'opération a réussi.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message informatif ou descriptif de l'opération.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Données utiles retournées par l'API.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Liste des messages d'erreur en cas d'échec.
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Crée une réponse de succès.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string message = "")
    {
        return new ApiResponse<T> { Success = true, Data = data, Message = message };
    }

    /// <summary>
    /// Crée une réponse d'échec.
    /// </summary>
    public static ApiResponse<T> Fail(string message, List<string>? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
}

/// <summary>
/// Version non générique de ApiResponse pour les opérations sans données de retour.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Crée une réponse de succès simple.
    /// </summary>
    public static ApiResponse Ok(string message = "")
    {
        return new ApiResponse { Success = true, Message = message };
    }
}
