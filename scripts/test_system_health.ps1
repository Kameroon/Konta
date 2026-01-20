
# Test de Sante du Systeme Konta ERP

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

# Desactiver la verification SSL pour les certificats auto-signes
add-type @"
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    public class TrustAllCertsPolicy : ICertificatePolicy {
        public bool CheckValidationResult(
            ServicePoint srvPoint, X509Certificate certificate,
            WebRequest request, int certificateProblem) {
            return true;
        }
    }
"@
[System.Net.ServicePointManager]::CertificatePolicy = New-Object TrustAllCertsPolicy

Write-Host "--- Debut du test d'integration Konta ---" -ForegroundColor Cyan

$processes = @()

try {
    # 1. Build
    foreach ($service in $services) {
        Write-Host "Compilation de $($service.Name)..."
        dotnet build $service.Path -v q
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Echec de la compilation pour $($service.Name)"
            return
        }
    }
    Write-Host "Compilation reussie pour tous les services." -ForegroundColor Green

    # 2. Run in background
    foreach ($service in $services) {
        Write-Host "Demarrage de $($service.Name) sur le port $($service.Port)..."
        $url = "https://localhost:$($service.Port)"
        # On utilise Start-Process pour lancer en arrière-plan
        $proc = Start-Process dotnet -ArgumentList "run --project $($service.Path) --urls $url" -PassThru -NoNewWindow
        $processes += @{ Name = $service.Name; Process = $proc; Port = $service.Port }
    }

    Write-Host "Attente du demarrage des services (30 secondes)..."
    Start-Sleep -Seconds 30

    # 3. Health Check
    $allHealthy = $true
    foreach ($proc in $processes) {
        Write-Host "Verification de $($proc.Name)..."
        try {
            $response = Invoke-WebRequest -Uri "https://localhost:$($proc.Port)/swagger/index.html" -TimeoutSec 10 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Host "... $($proc.Name) est OK." -ForegroundColor Green
            } else {
                Write-Host "... $($proc.Name) erreur statut: $($response.StatusCode)" -ForegroundColor Yellow
                $allHealthy = $false
            }
        } catch {
            Write-Host "... $($proc.Name) est INJOIGNABLE sur le port $($proc.Port)." -ForegroundColor Red
            $allHealthy = $false
        }
    }

    # 4. Gateway Routing Check
    Write-Host "Verification du routage via la Gateway..."
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:5000/gateway/identity/auth/login" -Method Post -Body '{}' -ContentType "application/json" -TimeoutSec 10 -ErrorAction SilentlyContinue
        Write-Host "... Routage Gateway -> Identity OK (Statut: $($response.StatusCode))." -ForegroundColor Green
    } catch {
        Write-Host "... Echec du routage Gateway vers Identity." -ForegroundColor Red
    }

} finally {
    # 5. Cleanup
    Write-Host "Arret des services..."
    foreach ($proc in $processes) {
        if ($proc.Process -and -not $proc.Process.HasExited) {
            Write-Host "... Arret de $($proc.Name) (PID: $($proc.Process.Id))"
            Stop-Process -Id $proc.Process.Id -Force -ErrorAction SilentlyContinue
        }
    }
    Write-Host "--- Audit termine ---"
}
