# 📋 Payloads JSON pour Postman - Konta ERP

Ce dossier contient des exemples de payloads JSON prêts à l'emploi pour tester tous les endpoints de l'API Konta.

## 📁 Structure des Fichiers

### 🔐 Authentification & Identity
- `auth_register.json` - Inscription d'un nouveau tenant avec utilisateur admin
- `auth_login.json` - Connexion d'un utilisateur
- `role_create.json` - Création d'un rôle personnalisé

### 🏢 Tenant Management
- `tenant_update.json` - Mise à jour des informations d'entreprise

### 💰 Finance (Comptabilité)
- `finance_account_create.json` - Création d'un compte comptable (512000 - Banque)
- `finance_journal_create.json` - Création d'un journal (VT - Ventes)
- `finance_entry_create.json` - Écriture comptable avec TVA

### 📊 Finance Core (Gestion Avancée)
- `tier_supplier_create.json` - Création d'un fournisseur
- `tier_customer_create.json` - Création d'un client
- `budget_create.json` - Création d'un budget trimestriel
- `treasury_account_create.json` - Création d'un compte de trésorerie
- `invoice_create.json` - Création d'une facture fournisseur

### 📈 Reporting
- `report_balance_sheet.json` - Génération d'un bilan comptable (PDF)
- `report_income_statement.json` - Génération d'un compte de résultat (Excel)

## 🚀 Utilisation dans Postman

### Méthode 1 : Copier-Coller
1. Ouvrez le fichier JSON correspondant
2. Copiez le contenu
3. Dans Postman, allez dans l'onglet **Body** → **raw** → **JSON**
4. Collez le contenu
5. Modifiez les valeurs si nécessaire (IDs, dates, montants)

### Méthode 2 : Importer comme Variable
1. Créez une variable d'environnement dans Postman
2. Collez le JSON comme valeur
3. Utilisez `{{variable_name}}` dans le body de votre requête

## ⚠️ Points d'Attention

### IDs à Remplacer
Certains payloads contiennent des IDs placeholder (`00000000-0000-0000-0000-000000000000`). Vous devez les remplacer par de vrais IDs obtenus via GET :

**Exemple pour une écriture comptable** :
```json
{
  "journalId": "00000000-0000-0000-0000-000000000000",  // ← Remplacer
  "lines": [
    {
      "accountId": "00000000-0000-0000-0000-000000000001"  // ← Remplacer
    }
  ]
}
```

**Workflow** :
1. `GET /gateway/finance/api/journals` → Récupérer l'ID du journal
2. `GET /gateway/finance/api/accounts` → Récupérer les IDs des comptes
3. Remplacer les IDs dans `finance_entry_create.json`
4. `POST /gateway/finance/api/entries` avec le payload modifié

### Dates
Les dates sont au format ISO 8601 : `2026-01-20T00:00:00Z`
- Ajustez les dates selon vos besoins de test
- Pour les périodes budgétaires, respectez la cohérence startDate < endDate

### Montants
Les montants sont en décimales (ex: `1200.00`)
- Pour les écritures comptables : **Débit = Crédit** (équilibre obligatoire)
- Pour les factures : `totalAmountTtc = totalAmountHt + vatAmount`

## 📝 Exemples de Scénarios Complets

### Scénario 1 : Créer une Vente Complète
```
1. POST /api/tiers (tier_customer_create.json) → Créer le client
2. POST /api/accounts (finance_account_create.json) → Créer les comptes si nécessaire
3. POST /api/journals (finance_journal_create.json) → Créer le journal VT
4. POST /api/entries (finance_entry_create.json) → Enregistrer la vente
5. GET /api/reports (report_income_statement.json) → Voir l'impact sur le résultat
```

### Scénario 2 : Gestion Budgétaire
```
1. POST /api/budgets (budget_create.json) → Créer un budget Marketing
2. POST /api/entries → Enregistrer des dépenses marketing
3. GET /api/budgets/{id}/status → Vérifier le taux de consommation
4. GET /api/alerts → Voir les alertes si seuil dépassé
```

### Scénario 3 : Multi-Tenant
```
1. POST /api/auth/register (auth_register.json) → Tenant A
2. POST /api/auth/register (modifier email) → Tenant B
3. Login Tenant A → Créer des données
4. Login Tenant B → Vérifier isolation (données de A invisibles)
```

## 🔧 Personnalisation

N'hésitez pas à modifier ces payloads selon vos besoins :
- Changez les noms, montants, dates
- Ajoutez des champs optionnels
- Créez vos propres variations

## 💡 Conseils

1. **Commencez simple** : Testez d'abord Register → Login → Get User
2. **Vérifiez les réponses** : Notez les IDs retournés pour les réutiliser
3. **Testez l'isolation** : Créez plusieurs tenants pour valider la RLS
4. **Consultez Swagger** : Pour voir tous les champs disponibles

Bon test ! 🚀
