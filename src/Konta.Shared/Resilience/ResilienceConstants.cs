// Namespace contenant les constantes de résilience partagées par tous les microservices
namespace Konta.Shared.Resilience;

/// <summary>
/// Constantes de configuration pour les stratégies de résilience (Retry et Circuit Breaker).
/// Ces valeurs définissent comment l'application gère les pannes et erreurs transitoires.
/// </summary>
public static class ResilienceConstants // Classe statique pour regrouper toutes les constantes de résilience
{
    // Nom de la politique de résilience par défaut utilisée dans toute l'application
    public const string DefaultPolicy = "DefaultResilience"; // Identifiant unique pour récupérer cette politique via Polly
    
    /// <summary>
    /// Configuration du pattern Retry (Réessai automatique).
    /// Permet de réessayer une opération qui a échoué, en espérant qu'elle réussisse la fois suivante.
    /// </summary>
    public static class Retry // Classe imbriquée pour regrouper les paramètres de retry
    {
        // Nombre maximum de tentatives (3 = 1 tentative initiale + 2 réessais)
        public const int Count = 3; // Si une opération échoue, elle sera réessayée jusqu'à 3 fois au total
        
        // Délai de base entre chaque tentative (2s, puis 4s, puis 8s avec backoff exponentiel)
        public static readonly TimeSpan Delay = TimeSpan.FromSeconds(2); // Premier délai de 2 secondes, doublé à chaque tentative
    }

    /// <summary>
    /// Configuration du pattern Circuit Breaker (Disjoncteur).
    /// Protège le système en bloquant les appels vers un service défaillant pour lui laisser le temps de se rétablir.
    /// </summary>
    public static class CircuitBreaker // Classe imbriquée pour regrouper les paramètres du circuit breaker
    {
        // Seuil de déclenchement : si 50% des requêtes échouent, le circuit s'ouvre (bloque les appels)
        public const double FailureThreshold = 0.5; // 50% d'échec = 0.5 (valeur entre 0.0 et 1.0)
        
        // Durée de la fenêtre d'observation pour calculer le taux d'échec (30 secondes)
        public static readonly TimeSpan SamplingDuration = TimeSpan.FromSeconds(30); // Le taux d'échec est calculé sur les 30 dernières secondes
        
        // Durée pendant laquelle le circuit reste ouvert (bloqué) avant de tester à nouveau le service (15 secondes)
        public static readonly TimeSpan BreakDuration = TimeSpan.FromSeconds(15); // Après ouverture, attendre 15s avant de retester le service
    }
} // Fin de la classe ResilienceConstants

