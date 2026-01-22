namespace Konta.Reporting.Models.Analytics;

/// <summary>
/// Représente un Indicateur Clé de Performance (KPI) pour l'affichage graphique.
/// </summary>
public class DashboardKpi
{
    /// <summary> Identifiant unique du KPI </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary> Libellé affiché (ex: Chiffre d'Affaires) </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary> Valeur actuelle </summary>
    public decimal Value { get; set; }

    /// <summary> Valeur de la période précédente pour calcul de tendance </summary>
    public decimal PreviousValue { get; set; }
    
    /// <summary> Tendance (up, down, stable) </summary>
    public string Trend { get; set; } = "stable";

    /// <summary> Format d'affichage (currency, percentage, number) </summary>
    public string Format { get; set; } = "currency";
    
    /// <summary> Couleur thématique (hex ou nom) </summary>
    public string Color { get; set; } = "#3b82f6";
}
