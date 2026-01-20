# Script PowerShell pour corriger automatiquement les préfixes de schéma PostgreSQL
# dans tous les repositories

$ErrorActionPreference = "Stop"

# Définition des mappings schéma -> tables
$schemaMapping = @{
    "identity" = @("Tenants", "Users", "Roles", "Permissions", "RolePermissions", "RefreshTokens")
    "billing" = @("StripeCustomers", "BillingInvoices", "WebhookEvents")
    "finance" = @("Accounts", "Journals", "JournalEntries")
    "finance_core" = @("Tiers", "BusinessInvoices", "Budgets", "TreasuryAccounts", "FinanceAlerts")
    "reporting" = @("ReportingSnapshots")
}

# Fonction pour corriger un fichier
function Fix-SchemaInFile {
    param(
        [string]$FilePath,
        [string]$Schema,
        [string[]]$Tables
    )
    
    Write-Host "Traitement de $FilePath..." -ForegroundColor Cyan
    
    $content = Get-Content $FilePath -Raw
    $modified = $false
    
    foreach ($table in $Tables) {
        # Patterns à remplacer
        $patterns = @(
            "FROM $table ",
            "FROM $table`r",
            "FROM $table`n",
            "INTO $table ",
            "INTO $table`r",
            "INTO $table`n",
            "UPDATE $table ",
            "UPDATE $table`r",
            "UPDATE $table`n",
            "DELETE FROM $table ",
            "DELETE FROM $table`r",
            "DELETE FROM $table`n",
            "JOIN $table ",
            "JOIN $table`r",
            "JOIN $table`n"
        )
        
        foreach ($pattern in $patterns) {
            $replacement = $pattern -replace $table, "$Schema.$table"
            if ($content -match [regex]::Escape($pattern)) {
                $content = $content -replace [regex]::Escape($pattern), $replacement
                $modified = $true
                Write-Host "  ✓ Corrigé: $pattern -> $replacement" -ForegroundColor Green
            }
        }
    }
    
    if ($modified) {
        Set-Content -Path $FilePath -Value $content -NoNewline
        Write-Host "  ✅ Fichier mis à jour" -ForegroundColor Green
    } else {
        Write-Host "  ⏭️  Aucune modification nécessaire" -ForegroundColor Yellow
    }
}

# Parcourir tous les repositories
$repoPath = "c:\Users\DELL\source\repos\Konta\src"

foreach ($schema in $schemaMapping.Keys) {
    $tables = $schemaMapping[$schema]
    
    Write-Host "`n=== Traitement du schéma: $schema ===" -ForegroundColor Magenta
    
    # Trouver tous les fichiers Repository.cs
    $repositories = Get-ChildItem -Path $repoPath -Recurse -Filter "*Repository.cs" | Where-Object { $_.FullName -notlike "*\bin\*" -and $_.FullName -notlike "*\obj\*" }
    
    foreach ($repo in $repositories) {
        Fix-SchemaInFile -FilePath $repo.FullName -Schema $schema -Tables $tables
    }
}

Write-Host "`n✅ Correction terminée!" -ForegroundColor Green
Write-Host "Veuillez compiler la solution pour vérifier les changements." -ForegroundColor Cyan
