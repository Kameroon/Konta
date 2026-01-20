# Guide de Test - Gateway Konta

## 1. Démarrage de la Gateway
```bash
cd c:\Users\DELL\source\repos\Konta\src\Konta.Gateway
dotnet run
```

## 2. Tests à effectuer

### A. Page d'accueil
- **URL** : `https://localhost:5000/`
- **Résultat attendu** : Redirection automatique vers la page d'accueil avec la liste des services
- **Vérification** : Vous devriez voir une belle page avec tous les microservices listés

### B. Documentation Swagger
- **URL** : `https://localhost:5000/swagger`
- **Résultat attendu** : Interface Swagger de la Gateway
- **Vérification** : Documentation interactive de l'API

### C. Routage vers Identity (exemple)
Pour tester le routage, vous devez d'abord démarrer le service Identity :

```bash
# Terminal 1 - Identity
cd c:\Users\DELL\source\repos\Konta\src\Konta.Identity
dotnet run

# Terminal 2 - Gateway
cd c:\Users\DELL\source\repos\Konta\src\Konta.Gateway
dotnet run
```

Puis testez :
- **URL** : `https://localhost:5000/gateway/identity/swagger/index.html`
- **Résultat attendu** : Swagger du service Identity via la Gateway
- **Vérification** : Vous voyez la documentation du service Identity

### D. Vérification de l'Observabilité
Dans les logs de la console, vous devriez voir :
```
Activity.TraceId: [un identifiant unique]
Activity.SpanId: [un identifiant de span]
```

Cela confirme qu'OpenTelemetry trace les requêtes.

## 3. Test complet avec le script PowerShell

Pour tester tous les services ensemble :
```powershell
cd c:\Users\DELL\source\repos\Konta
powershell -ExecutionPolicy Bypass -File scripts\test_system_health.ps1
```

Ce script va :
1. Compiler tous les services
2. Les démarrer en arrière-plan
3. Vérifier leur santé
4. Tester le routage de la Gateway
5. Tout arrêter proprement
