# Guide de Lancement - Konta

## Problème identifié
Les services démarrent sur des ports aléatoires au lieu des ports configurés (5000-5007).

## Solution : Lancer avec les bons ports

### Option 1 : Via la ligne de commande (RECOMMANDÉ)

```bash
# Gateway (Port 5000)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Gateway
dotnet run --urls "https://localhost:5000"

# Identity (Port 5001)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Identity
dotnet run --urls "https://localhost:5001"

# Tenant (Port 5002)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Tenant
dotnet run --urls "https://localhost:5002"

# Billing (Port 5003)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Billing
dotnet run --urls "https://localhost:5003"

# Finance (Port 5004)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Finance
dotnet run --urls "https://localhost:5004"

# OCR (Port 5005)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Ocr
dotnet run --urls "https://localhost:5005"

# Finance.Core (Port 5006)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Finance.Core
dotnet run --urls "https://localhost:5006"

# Reporting (Port 5007)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Reporting
dotnet run --urls "https://localhost:5007"
```

### Option 2 : Créer des launchSettings.json

Je peux créer des fichiers `launchSettings.json` pour chaque projet avec les bons ports.

### Option 3 : Script PowerShell automatisé

Utiliser le script qui lance tout automatiquement avec les bons ports.

## Test rapide

Pour tester immédiatement, lancez juste la Gateway :
```bash
cd c:\Users\DELL\source\repos\Konta\src\Konta.Gateway
dotnet run --urls "https://localhost:5000"
```

Puis accédez à : `https://localhost:5000/`
