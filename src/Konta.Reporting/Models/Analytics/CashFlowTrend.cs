namespace Konta.Reporting.Models.Analytics;

/// <summary>
/// Point de données pour l'évolution temporelle de la trésorerie.
/// </summary>
public class CashFlowTrend
{
    /// <summary> Date du point de donnée </summary>
    public DateTime Date { get; set; }
    
    /// <summary> Solde total cumulé à cette date </summary>
    public decimal Balance { get; set; }
    
    /// <summary> Flux entrant net sur la période </summary>
    public decimal Inflow { get; set; }
    
    /// <summary> Flux sortant net sur la période </summary>
    public decimal Outflow { get; set; }
}
