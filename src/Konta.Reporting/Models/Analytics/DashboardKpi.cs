namespace Konta.Reporting.Models.Analytics;

/// <summary>
/// Représente un Indicateur Clé de Performance (KPI) pour l'affichage graphique.
/// </summary>
public class DashboardKpi
{
    /// <summary> Nom de l'indicateur </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary> Valeur actuelle </summary>
    public decimal Value { get; set; }
    
    /// <summary> Unité (%, €, etc.) </summary>
    public string Unit { get; set; } = "€";
    
    /// <summary> Tendance par rapport à la période précédente </summary>
    public decimal TrendPercentage { get; set; }
    
    /// <summary> Statut de l'indicateur (Ok, Warning, Critical) </summary>
    public string Status { get; set; } = "Ok";
}
