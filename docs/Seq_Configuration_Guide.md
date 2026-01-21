# Guide Complet : Configuration de Seq pour les Logs Konta

## ✅ Étape 1 : Installation de Seq

### **Option A : Via Docker (Recommandé)**

```powershell
# Démarrer Docker Desktop d'abord, puis :
docker run -d --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest
```

### **Option B : Installation Windows**

1. Télécharger depuis https://datalust.co/download
2. Installer et démarrer le service

### **Vérification**

Ouvrez votre navigateur : **http://localhost:5341**

Vous devriez voir l'interface Seq.

---

## ✅ Étape 2 : Configuration Effectuée

### **Packages Installés dans Konta.Shared**

- ✅ Serilog.AspNetCore
- ✅ Serilog.Sinks.Console
- ✅ Serilog.Sinks.File
- ✅ Serilog.Sinks.Seq
- ✅ Serilog.Enrichers.Environment

### **Extension Créée**

`Konta.Shared/Extensions/SerilogExtensions.cs` - Configuration centralisée pour tous les microservices.

### **Microservices Configurés**

- ✅ Konta.Identity

---

## 📝 Étape 3 : Configuration des Autres Microservices

Pour chaque microservice, ajoutez cette ligne dans `Program.cs` :

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuration de Serilog
builder.AddSerilogLogging("Konta.NomDuService");
```

### **Liste des Microservices à Configurer**

```csharp
// Konta.Tenant/Program.cs
builder.AddSerilogLogging("Konta.Tenant");

// Konta.Finance/Program.cs
builder.AddSerilogLogging("Konta.Finance");

// Konta.Billing/Program.cs
builder.AddSerilogLogging("Konta.Billing");

// Konta.Gateway/Program.cs
builder.AddSerilogLogging("Konta.Gateway");

// Konta.Ocr/Program.cs
builder.AddSerilogLogging("Konta.Ocr");

// Konta.Reporting/Program.cs
builder.AddSerilogLogging("Konta.Reporting");

// Konta.Finance.Core/Program.cs
builder.AddSerilogLogging("Konta.Finance.Core");
```

---

## ⚙️ Étape 4 : Configuration Optionnelle

### **appsettings.json** (Optionnel)

Si vous voulez personnaliser l'URL de Seq ou ajouter une clé API :

```json
{
  "Seq": {
    "ServerUrl": "http://localhost:5341",
    "ApiKey": null
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

---

## 🚀 Étape 5 : Utilisation

### **Démarrer un Microservice**

```powershell
cd src/Konta.Identity
dotnet run
```

### **Logs Générés**

#### **1. Console** (Temps réel)
```
[22:51:33 INF] [Konta.Identity] Application started
[22:51:34 DBG] [Konta.Identity] DEBUG SQL EXECUTION: SELECT * FROM ...
```

#### **2. Fichiers** (Persistants)
```
src/Konta.Identity/logs/konta.identity-20260120.txt
```

#### **3. Seq** (Interface Web)
Ouvrez **http://localhost:5341** pour voir tous les logs en temps réel.

---

## 🔍 Utilisation de Seq

### **Interface Principale**

1. **Ouvrez** http://localhost:5341
2. Vous verrez tous les logs en temps réel

### **Filtres Puissants**

#### **Filtrer par Service**
```
ServiceName = "Konta.Identity"
```

#### **Filtrer par Niveau**
```
@Level = "Error"
```

#### **Recherche de Texte**
```
@Message like "%PostgreSQL%"
```

#### **Filtrer par Période**
```
@Timestamp > Now() - 1h
```

#### **Combinaisons**
```
ServiceName = "Konta.Identity" and @Level = "Warning"
```

### **Exemples Pratiques**

#### **Voir toutes les erreurs PostgreSQL**
```
@Message like "%PostgreSQL%" and @Level = "Error"
```

#### **Voir les logs d'un tenant spécifique**
```
TenantId = "abc-123-456"
```

#### **Voir les requêtes SQL lentes**
```
@Message like "%DEBUG SQL%" and @Elapsed > 1000
```

---

## 📊 Fonctionnalités Avancées de Seq

### **1. Dashboards**
- Créez des tableaux de bord personnalisés
- Graphiques en temps réel
- KPIs de performance

### **2. Alertes**
- Configurez des alertes par email
- Notifications Slack/Teams
- Webhooks personnalisés

### **3. Requêtes Sauvegardées**
- Sauvegardez vos filtres fréquents
- Partagez avec l'équipe

### **4. Export**
- Exportez les logs en JSON/CSV
- Archivage long terme

---

## 🎯 Cas d'Usage Typiques

### **Débugger un Login Échoué**

1. Ouvrez Seq : http://localhost:5341
2. Filtrez : `ServiceName = "Konta.Identity" and @Message like "%login%"`
3. Cliquez sur un log pour voir tous les détails
4. Suivez la trace complète de la requête

### **Analyser les Performances**

1. Filtrez : `@Message like "%DEBUG SQL%"`
2. Triez par durée d'exécution
3. Identifiez les requêtes lentes

### **Surveiller les Erreurs**

1. Filtrez : `@Level = "Error"`
2. Groupez par `ServiceName`
3. Identifiez les services problématiques

---

## 📁 Structure des Logs Fichiers

```
src/
├── Konta.Identity/
│   └── logs/
│       ├── konta.identity-20260120.txt
│       ├── konta.identity-20260119.txt
│       └── ...
├── Konta.Finance/
│   └── logs/
│       ├── konta.finance-20260120.txt
│       └── ...
└── ...
```

**Rotation** : Un fichier par jour, 30 jours de rétention.

---

## 🔧 Commandes PowerShell Utiles

### **Voir les Logs Fichiers**

```powershell
# Dernières lignes
Get-Content .\logs\konta.identity-20260120.txt -Tail 50

# Suivre en temps réel
Get-Content .\logs\konta.identity-20260120.txt -Wait -Tail 20

# Rechercher une erreur
Select-String -Path .\logs\*.txt -Pattern "ERROR"
```

### **Gérer Seq via Docker**

```powershell
# Arrêter Seq
docker stop seq

# Démarrer Seq
docker start seq

# Voir les logs de Seq
docker logs seq

# Supprimer Seq
docker rm -f seq
```

---

## ✅ Checklist de Vérification

- [ ] Docker Desktop est démarré
- [ ] Seq est accessible sur http://localhost:5341
- [ ] Konta.Identity est configuré avec Serilog
- [ ] Les logs apparaissent dans Seq
- [ ] Les fichiers de logs sont créés dans `logs/`
- [ ] Les autres microservices sont configurés

---

## 🎉 Résultat Final

Vous avez maintenant :

✅ **Logs Console** - Pour le développement local  
✅ **Logs Fichiers** - Pour l'archivage et l'audit  
✅ **Logs Seq** - Pour la visualisation et l'analyse  

**Tous vos microservices logguent de manière centralisée et structurée !**

---

## 🚀 Prochaines Étapes

1. Configurez les autres microservices (Tenant, Finance, etc.)
2. Explorez l'interface Seq
3. Créez des dashboards personnalisés
4. Configurez des alertes pour les erreurs critiques

**Bonne exploration des logs !** 🎯
