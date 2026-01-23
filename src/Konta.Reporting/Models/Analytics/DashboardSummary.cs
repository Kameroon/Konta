namespace Konta.Reporting.Models.Analytics;

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class DashboardSummary
{
    public int TotalDocuments { get; set; }
    public int TotalCompanies { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    
    // Tendances
    public double DocumentsTrend { get; set; }
    public double CompaniesTrend { get; set; }
    public double RevenueTrend { get; set; }

    // Séries pour les graphiques
    public List<ChartDataPoint> MonthlyDocuments { get; set; } = new();
    public List<ChartDataPoint> MonthlyRevenue { get; set; } = new();
    public List<ChartDataPoint> DocumentTypes { get; set; } = new();
}
