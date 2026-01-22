-- SCRIPT DE RÉPARATION POUR LES PLANS D'ABONNEMENT
-- À exécuter dans la base de données Konta

-- 1. Création de la table si elle n'existe pas
CREATE TABLE IF NOT EXISTS billing.SubscriptionPlans (
    Id UUID PRIMARY KEY,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    Interval TEXT NOT NULL DEFAULT 'month',
    MaxUsers INTEGER,
    StorageGb INTEGER,
    HasPrioritySupport BOOLEAN NOT NULL DEFAULT FALSE,
    HasApiAccess BOOLEAN NOT NULL DEFAULT FALSE,
    Modules JSONB,
    Features JSONB,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- 2. Activation de la RLS pour la lecture publique
ALTER TABLE billing.SubscriptionPlans ENABLE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS public_read_policy ON billing.SubscriptionPlans;
CREATE POLICY public_read_policy ON billing.SubscriptionPlans FOR SELECT USING (true);

-- 3. Insertion des données (Suppression préalable pour éviter les doublons si partiel)
DELETE FROM billing.SubscriptionPlans WHERE Code IN ('discovery', 'basic', 'advanced', 'premium', 'expertise');

INSERT INTO billing.SubscriptionPlans (Id, Code, Name, Description, Price, Currency, MaxUsers, StorageGb, HasPrioritySupport, HasApiAccess, Features) VALUES
(gen_random_uuid(), 'discovery', 'Découverte', 'Idéal pour tester la plateforme', 0.00, 'EUR', 1, 1, FALSE, FALSE, '["1 utilisateur inclus", "Modules limités", "1 Go de stockage"]'),
(gen_random_uuid(), 'basic', 'Basique', 'Pour les petites entreprises', 19.00, 'EUR', 3, 10, FALSE, FALSE, '["3 utilisateurs inclus", "Modules standards", "10 Go de stockage", "Support Email"]'),
(gen_random_uuid(), 'advanced', 'Avancée', 'Pour une gestion poussée', 49.00, 'EUR', 6, 50, TRUE, TRUE, '["6 utilisateurs inclus", "Tous les modules", "50 Go de stockage", "Support Email + Chat", "Accès API"]'),
(gen_random_uuid(), 'premium', 'Premium', 'Tout illimité pour les PME', 99.00, 'EUR', 10, 500, TRUE, TRUE, '["10 utilisateurs inclus", "Tous les modules", "Stockage illimité", "Support SLA", "Accès API"]'),
(gen_random_uuid(), 'expertise', 'Expertise', 'Sur mesure pour les grands comptes', 0.00, 'EUR', 0, 0, TRUE, TRUE, '["Utilisateurs sur devis", "Modules sur mesure", "Stockage sur devis", "Accès API complet"]');

-- Vérification
SELECT * FROM billing.SubscriptionPlans;
