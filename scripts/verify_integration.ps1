# Test d'Intégration Global Konta ERP

$services = @(
    @{ Name = "Gateway"; Port = 5000; Path = "src/Konta.Gateway" },
    @{ Name = "Identity"; Port = 5001; Path = "src/Konta.Identity" },
    @{ Name = "Tenant"; Port = 5002; Path = "src/Konta.Tenant" },
    @{ Name = "Billing"; Port = 5003; Path = "src/Konta.Billing" },
    @{ Name = "Finance"; Port = 5004; Path = "src/Konta.Finance" },
    @{ Name = "Ocr"; Port = 5005; Path = "src/Konta.Ocr" },
    @{ Name = "FinanceCore"; Port = 5006; Path = "src/Konta.Finance.Core" },
    @{ Name = "Reporting"; Port = 5007; Path = "src/Konta.Reporting" }
)

Write-Host "🚀 Lancement de l'audit système Konta..." -ForegroundColor Cyan

foreach ($service in $services) {
    Write-Host "Vérification du build pour $($service.Name)..."
    dotnet build $service.Path --quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Échec du build pour $($service.Name)"
        exit 1
    }
}

Write-Host "✅ Tous les services sont valides et compilent." -ForegroundColor Green
Write-Host "Prêt pour le déploiement local simulé." -ForegroundColor Yellow
