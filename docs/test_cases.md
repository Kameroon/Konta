# Cas de Test - Plateforme Konta ERP

Ce document décrit les scénarios de test essentiels pour valider l'intégration et la logique métier de la solution Konta.

## 1. Onboarding et Authentification
### CT-01 : Inscription d'un nouveau Tenant
- **Description** : Vérifier qu'un nouveau client peut s'enregistrer.
- **Étapes** :
    1. Envoyer un `POST /gateway/identity/api/auth/register` avec les données de l'entreprise et de l'admin.
- **Résultat attendu** : 
    - Création du Tenant en base.
    - Création de l'utilisateur avec le rôle Admin.
    - Réponse 200 OK avec le jeton JWT.

### CT-02 : Connexion Utilisateur
- **Description** : Valider l'authentification.
- **Étapes** :
    1. Envoyer un `POST /gateway/identity/api/auth/login` avec des identifiants valides.
- **Résultat attendu** : Retour d'un `AccessToken` et d'un `RefreshToken`.

## 2. Gestion Financière & Comptable
### CT-03 : Création d'une Écriture Comptable Équilibrée
- **Description** : Vérifier que le système accepte les écritures où Débit = Crédit.
- **Étapes** :
    1. Soumettre un `JournalEntry` avec 2 lignes (un débit de 100€ et un crédit de 100€).
- **Résultat attendu** : Persistance réussie dans les tables `JournalEntries` et `EntryLines`.

### CT-04 : Rejet d'une Écriture Déséquilibrée
- **Description** : Vérifier la validation de l'équilibre comptable.
- **Étapes** :
    1. Soumettre une écriture avec Débit = 100€ et Crédit = 80€.
- **Résultat attendu** : Erreur 400 Bad Request avec message d'erreur explicite.

## 3. OCR et Intelligence Artificielle
### CT-05 : Cycle complet d'extraction OCR
- **Description** : Transformer un PDF en données structurées.
- **Étapes** :
    1. Uploader un PDF de facture via `POST /gateway/ocr/api/extraction/upload`.
    2. Suivre le statut du job jusu'à `Completed`.
- **Résultat attendu** : Données extraites (Montant TTC, Fournisseur) visibles dans la table `ExtractedInvoices`.

## 4. Reporting et Dashboard
### CT-06 : Génération de Snapshot Analytique
- **Description** : Valider le calcul et la mise en cache des indicateurs.
- **Étapes** :
    1. Appeler l'endpoint `GET /gateway/reporting/api/dashboard/summary`.
- **Résultat attendu** : 
    - Création d'un enregistrement dans `ReportingSnapshots`.
    - Retour rapide des agrégats (Chiffre d'Affaires, Trésorerie).

## 5. Billing et SaaS
### CT-07 : Traitement des Webhooks Stripe
- **Description** : Simuler le paiement d'un abonnement.
- **Étapes** :
    1. Envoyer un événement `invoice.paid` fictif à la gateway de billing.
- **Résultat attendu** : 
    - Enregistrement dans `WebhookEvents`.
    - Mise à jour du plan du Tenant vers "Premium".

## 6. Tests par Profils (RBAC)
### CT-08 : Validation de Facture par un Comptable (Accountant)
- **Description** : Vérifier qu'un utilisateur avec le rôle Comptable peut valider une facture et générer une écriture.
- **Étapes** :
    1. Se connecter avec le profil `Accountant`.
    2. Sélectionner une facture au statut `Draft`.
    3. Cliquer sur "Valider pour comptabilisation".
- **Résultat attendu** : 
    - Le statut de la facture passe à `Validated`.
    - Une écriture comptable est automatiquement générée dans le journal des achats/ventes.

### CT-09 : Consultation Read-Only par un Auditeur (Auditor)
- **Description** : Vérifier que le profil Auditeur ne peut pas modifier les données.
- **Étapes** :
    1. Se connecter avec le profil `Auditor`.
    2. Tenter de modifier le libellé d'un compte comptable via `PUT /gateway/finance/api/accounts/{id}`.
- **Résultat attendu** : 
    - Accès aux tableaux de bord et exports autorisé.
    - Échec de la modification avec une erreur `403 Forbidden`.
