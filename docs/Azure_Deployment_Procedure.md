# Azure Deployment Procedure - Konta ERP

> **Last Updated:** February 2026  
> **Version:** 2.0 - Post-Deployment

## Table des Matières
1. [Infrastructure Déployée](#infrastructure-déployée)
2. [Procédure de Déploiement](#procédure-de-déploiement)
3. [Configuration des Services](#configuration-des-services)
4. [Maintenance et Mises à jour](#maintenance-et-mises-à-jour)

---

## Infrastructure Déployée

### Ressources Azure
| Ressource | Nom | Région | SKU |
| :--- | :--- | :--- | :--- |
| Resource Group | `konta-rg` | West Europe | - |
| Container Registry | `kontaacr.azurecr.io` | West Europe | Basic |
| Container Apps Env | `konta-env` | West Europe | Consumption |
| PostgreSQL (prévu) | `konta-postgres` | West Europe | B1ms |

### Container Apps Déployés (Architecture Consolidée)
| Service | Rôle | URL / Ingress | Replicas |
| :--- | :--- | :--- | :--- |
| **Gateway** | Ocelot Gateway | konta-gateway.(...) - External | 0-1 |
| **Identity** | Core API (Auth, Tenant, Billing) | konta-identity.internal - Internal | 0-1 |
| **Finance** | Finance API (Accounting, Core) | konta-finance.internal - Internal | 0-1 |
| **Reporting** | Reporting & Analytics | konta-reporting.internal - Internal | 0-1 |
| **OCR** | OCR Document Processing | konta-ocr.internal - Internal | 0-1 |

---

## Procédure de Déploiement

### Prérequis
```bash
# Installer Azure CLI
winget install Microsoft.AzureCLI

# Se connecter
az login --use-device-code
```

### 1. Créer l'Infrastructure
```bash
# Resource Group
az group create --name konta-rg --location westeurope

# Container Registry
az provider register --namespace Microsoft.ContainerRegistry --wait
az acr create --resource-group konta-rg --name kontaacr --sku Basic
az acr update --name kontaacr --admin-enabled true

# Container Apps Environment
az extension add --name containerapp --upgrade -y
az provider register --namespace Microsoft.App --wait
az containerapp env create --name konta-env --resource-group konta-rg --location westeurope
```

### 2. Build et Push des Images
```bash
# Login au registry
az acr login --name kontaacr

# Build et push (pour chaque service)
# Build et push (5 Services)
docker build -f src/Konta.Gateway/Dockerfile -t kontaacr.azurecr.io/konta-gateway:latest .
docker build -f src/Konta.Identity/Dockerfile -t kontaacr.azurecr.io/konta-identity:latest .
docker build -f src/Konta.Finance/Dockerfile -t kontaacr.azurecr.io/konta-finance:latest .
docker build -f src/Konta.Reporting/Dockerfile -t kontaacr.azurecr.io/konta-reporting:latest .
docker build -f src/Konta.Ocr/Dockerfile -t kontaacr.azurecr.io/konta-ocr:latest .

# Push vers Azure
docker push kontaacr.azurecr.io/konta-gateway:latest
docker push kontaacr.azurecr.io/konta-identity:latest
docker push kontaacr.azurecr.io/konta-finance:latest
docker push kontaacr.azurecr.io/konta-reporting:latest
docker push kontaacr.azurecr.io/konta-ocr:latest
```

### 3. Déployer les Container Apps
```bash
# Gateway (externe)
az containerapp create --name konta-gateway --resource-group konta-rg --environment konta-env \
  --image kontaacr.azurecr.io/konta-gateway:latest --registry-server kontaacr.azurecr.io \
  --target-port 8080 --ingress external --min-replicas 0 --max-replicas 1 --cpu 0.25 --memory 0.5Gi

# Services internes (même commande avec --ingress internal)
```

---

## Configuration des Services

### Variables d'Environnement Requises
Chaque Container App nécessite les variables suivantes :

```bash
# Connection String PostgreSQL
ConnectionStrings__DefaultConnection=Host=konta-postgres.postgres.database.azure.com;Database=konta_db;Username=kontaadmin;Password=<PASSWORD>;SslMode=Require

# JWT Configuration (Gateway uniquement)
Jwt__Key=<SECRET_KEY>
Jwt__Issuer=Konta
Jwt__Audience=KontaUsers

# Environnement
ASPNETCORE_ENVIRONMENT=Azure
```

### Configurer une Variable
```bash
az containerapp update --name konta-identity --resource-group konta-rg \
  --set-env-vars ConnectionStrings__DefaultConnection="<CONNECTION_STRING>"
```

---

## Maintenance et Mises à jour

### Redéployer un Service
```bash
# Rebuild et push
docker build -f src/Konta.Identity/Dockerfile -t kontaacr.azurecr.io/konta-identity:latest .
docker push kontaacr.azurecr.io/konta-identity:latest

# Update le Container App
az containerapp update --name konta-identity --resource-group konta-rg \
  --image kontaacr.azurecr.io/konta-identity:latest
```

### Voir les Logs
```bash
az containerapp logs show --name konta-gateway --resource-group konta-rg --follow
```

### Supprimer Toutes les Ressources
```bash
az group delete --name konta-rg --yes --no-wait
```

---

## Coûts Estimés

Avec **Scale-to-Zero** activé :
- **Container Apps** : 0€ au repos, ~0.10€/heure en exécution
- **PostgreSQL B1ms** : Gratuit 12 mois, puis ~15€/mois
- **Static Web Apps** : Gratuit (tier Free)

**Total estimé** : 0-20€/mois selon l'utilisation
