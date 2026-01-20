# Guide d'Utilisation - Collection Postman Konta ERP

## 📥 Import de la Collection

1. Ouvrez Postman
2. Cliquez sur **Import** (en haut à gauche)
3. Sélectionnez le fichier `Konta_ERP_Collection.postman_collection.json`
4. La collection apparaîtra dans votre sidebar

## 🔧 Configuration

### Variables de Collection
La collection utilise 3 variables automatiques :
- `gateway_url` : URL de la Gateway (par défaut : `https://localhost:5000`)
- `access_token` : Token JWT (rempli automatiquement après login)
- `tenant_id` : ID du tenant (rempli automatiquement après login)

### Authentification Automatique
La collection est configurée avec **Bearer Token** au niveau racine. Toutes les requêtes (sauf Register et Login) utiliseront automatiquement le token.

## 🚀 Workflow de Test Recommandé

### 1. Créer un Compte (Register)
```
POST /gateway/identity/api/auth/register
```
- Crée un nouveau tenant et un utilisateur admin
- Retourne les informations du compte créé

### 2. Se Connecter (Login)
```
POST /gateway/identity/api/auth/login
```
- **Important** : Cette requête a un script automatique qui :
  - Extrait le `accessToken` de la réponse
  - Le sauvegarde dans la variable `access_token`
  - Extrait le `tenantId` et le sauvegarde
- Après cette requête, toutes les autres requêtes seront authentifiées automatiquement !

### 3. Tester les Autres Services
Une fois connecté, vous pouvez tester n'importe quel endpoint des autres services :
- ✅ Identity : Gérer les rôles, permissions, utilisateurs
- ✅ Tenant : Consulter/modifier les informations de l'entreprise
- ✅ Billing : Voir les factures et informations Stripe
- ✅ Finance : Créer des comptes, journaux, écritures comptables
- ✅ Finance Core : Gérer les tiers, budgets, trésorerie
- ✅ OCR : Uploader des documents pour extraction
- ✅ Reporting : Générer des rapports financiers

## 📝 Exemples de Scénarios de Test

### Scénario 1 : Création d'un Compte Comptable
1. Login
2. `GET /gateway/finance/api/accounts` → Voir les comptes existants
3. `POST /gateway/finance/api/accounts` → Créer un nouveau compte
4. `GET /gateway/finance/api/accounts` → Vérifier la création

### Scénario 2 : Gestion des Tiers
1. Login
2. `POST /gateway/finance-core/api/tiers` → Créer un fournisseur
3. `GET /gateway/finance-core/api/tiers` → Lister tous les tiers
4. `PUT /gateway/finance-core/api/tiers/{id}` → Modifier le fournisseur

### Scénario 3 : Extraction OCR
1. Login
2. `POST /gateway/ocr/api/extract` → Uploader une facture PDF
3. `GET /gateway/ocr/api/jobs/{jobId}` → Vérifier le statut de l'extraction

## 🔍 Vérification des Réponses

### Codes HTTP Attendus
- `200 OK` : Succès (GET, PUT)
- `201 Created` : Ressource créée (POST)
- `204 No Content` : Suppression réussie (DELETE)
- `400 Bad Request` : Données invalides
- `401 Unauthorized` : Token manquant ou invalide
- `403 Forbidden` : Accès refusé (mauvais tenant)
- `404 Not Found` : Ressource introuvable

### Vérifier l'Isolation Multi-Tenant
Pour tester la RLS (Row-Level Security) :
1. Créez 2 comptes différents (Register x2)
2. Connectez-vous avec le compte 1
3. Créez des données (compte, tier, etc.)
4. Connectez-vous avec le compte 2
5. Essayez de récupérer les données du compte 1 → Devrait retourner une liste vide !

## 🛠️ Personnalisation

### Changer l'URL de la Gateway
Si votre Gateway tourne sur un autre port :
1. Cliquez sur la collection "Konta ERP"
2. Onglet **Variables**
3. Modifiez `gateway_url`

### Ajouter des Requêtes
Vous pouvez facilement ajouter vos propres requêtes :
- Dupliquez une requête existante
- Modifiez l'URL et le body
- Le token sera automatiquement inclus

## 📊 Tests Automatisés (Optionnel)

Vous pouvez ajouter des tests automatiques dans l'onglet **Tests** de chaque requête :

```javascript
// Vérifier le code de statut
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

// Vérifier la structure de la réponse
pm.test("Response has required fields", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('name');
});
```

## 🎯 Conseils

1. **Utilisez les dossiers** : Les requêtes sont organisées par microservice
2. **Sauvegardez vos variables** : Après un login réussi, le token est automatiquement sauvegardé
3. **Testez l'isolation** : Créez plusieurs tenants pour vérifier la RLS
4. **Consultez Swagger** : Pour voir la documentation complète des endpoints : `https://localhost:5000/swagger`

Bon test ! 🚀
