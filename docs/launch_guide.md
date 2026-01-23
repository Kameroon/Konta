# Guide de Lancement - Konta

## Problème identifié
Les services démarrent parfois sur des ports aléatoires ou rencontrent des lenteurs de résolution DNS avec `localhost`.

## Solution : Lancer avec 127.0.0.1 (Recommandé)

### Option 1 : Via la ligne de commande (RECOMMANDÉ)

```bash
# Gateway (Port 5000)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Gateway
dotnet run --urls "http://127.0.0.1:5000"

# Identity (Port 5001)
cd c:\Users\DELL\source\repos\Konta\src\Konta.Identity
dotnet run --urls "http://127.0.0.1:5001"

# ... (Même chose pour tous les autres services)
```

> [!TIP]
> L'utilisation de `127.0.0.1` au lieu de `localhost` est fortement recommandée sur Windows pour éviter les délais de résolution IPv6/DNS.

## Configuration de la Base de Données

Si vous avez modifié des modèles ou le script de données :
1.  **Ré-initialisation** : Exécutez le script [database_init.sql](file:///c:/Users/DELL/source/repos/Konta/scripts/database_init.sql).
2.  **Mots de passe** : 
    - `admin@konta.fr` -> `Admin123!`
    - Comptes Globex (`admin@globex.com`, etc.) -> `Password123!`

## Test rapide

Pour tester immédiatement, lancez la Gateway :
```bash
cd c:\Users\DELL\source\repos\Konta\src\Konta.Gateway
dotnet run --urls "http://127.0.0.1:5000"
```

Puis accédez à : `http://127.0.0.1:5000/` (ou `localhost:5000` si votre navigateur redirige correctement).
