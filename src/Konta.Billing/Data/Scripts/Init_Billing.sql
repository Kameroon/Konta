-- Database: Konta_Billing
-- Schema: billing

CREATE SCHEMA IF NOT EXISTS billing;

-- Table des clients Stripe
CREATE TABLE IF NOT EXISTS billing.StripeCustomers (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL UNIQUE,
    StripeCustomerId TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des factures locales
CREATE TABLE IF NOT EXISTS billing.BillingInvoices (
    Id UUID PRIMARY KEY,
    InvoiceNumber TEXT NOT NULL UNIQUE,
    TenantId UUID NOT NULL,
    StripeInvoiceId TEXT UNIQUE,
    Amount DECIMAL(18,2) NOT NULL,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    Status TEXT NOT NULL, -- 'draft', 'open', 'paid', 'uncollectible', 'void'
    PdfUrl TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des plans d'abonnement
CREATE TABLE IF NOT EXISTS billing.SubscriptionPlans (
    Id UUID PRIMARY KEY,
    Code TEXT NOT NULL UNIQUE,
    Name TEXT NOT NULL,
    Description TEXT,
    Price DECIMAL(18,2) NOT NULL,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    Interval TEXT NOT NULL DEFAULT 'month',
    MaxUsers INT,
    StorageGb INT,
    HasPrioritySupport BOOLEAN NOT NULL DEFAULT FALSE,
    HasApiAccess BOOLEAN NOT NULL DEFAULT FALSE,
    Modules JSONB,
    Features JSONB,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table pour l'idempotence des Webhooks Stripe
CREATE TABLE IF NOT EXISTS billing.WebhookEvents (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    StripeEventId TEXT NOT NULL UNIQUE,
    EventType TEXT NOT NULL,
    ProcessedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ProcessedSuccessfully BOOLEAN NOT NULL DEFAULT FALSE
);

-- Index pour la performance
CREATE INDEX IF NOT EXISTS idx_stripecustomers_tenantid ON billing.StripeCustomers(TenantId);
CREATE INDEX IF NOT EXISTS idx_billinginvoices_tenantid ON billing.BillingInvoices(TenantId);
CREATE INDEX IF NOT EXISTS idx_webhookevents_stripeid ON billing.WebhookEvents(StripeEventId);
CREATE INDEX IF NOT EXISTS idx_subscriptionplans_code ON billing.SubscriptionPlans(Code);

-- Seed Data: Plans par défaut
INSERT INTO billing.SubscriptionPlans (Id, Code, Name, Description, Price, MaxUsers, StorageGb, HasPrioritySupport, HasApiAccess, Features)
VALUES 
('7b4e9f1a-6d2c-4b5a-8e3d-0f9a1b2c3d4e', 'discovery', 'Découverte', 'Pour explorer les fonctionnalités de base', 0.00, 1, 1, FALSE, FALSE, '["1 utilisateur", "1 Go stockage", "Support communautaire"]'),
('8c5f0a2b-7e3d-5c6b-9f4e-1a0b2c3d4e5f', 'basic', 'Basique', 'Essentiel pour les petites structures', 29.00, 3, 5, FALSE, FALSE, '["3 utilisateurs", "5 Go stockage", "Support email"]'),
('9d601b3c-8f4e-6d7c-0a5f-2b1c3d4e5f6a', 'advanced', 'Avancée', 'Gestion complète pour entreprises en croissance', 59.00, 10, 20, TRUE, FALSE, '["10 utilisateurs", "20 Go stockage", "Support prioritaire"]'),
('0e712c4d-9a5f-7e8d-1b60-3c2d4e5f6a7b', 'premium', 'Premium', 'Performance et support prioritaire', 129.00, 25, 100, TRUE, TRUE, '["25 utilisateurs", "100 Go stockage", "Accès API", "Account Manager"]'),
('1f823d5e-0b60-8f9e-2c71-4d3e5f6a7b8c', 'expertise', 'Expertise', 'Sur mesure pour les grands comptes', 299.00, 999, 1000, TRUE, TRUE, '["Utilisateurs illimités", "1 To stockage", "Support 24/7", "SLA garanti"]')
ON CONFLICT (Code) DO NOTHING;
