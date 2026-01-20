-- Database: Konta_Billing
-- Description: Schéma initial pour le microservice de facturation

-- Table des clients Stripe
CREATE TABLE IF NOT EXISTS StripeCustomers (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL UNIQUE,
    StripeCustomerId TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des factures locales
CREATE TABLE IF NOT EXISTS BillingInvoices (
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

-- Table pour l'idempotence des Webhooks Stripe
CREATE TABLE IF NOT EXISTS WebhookEvents (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    StripeEventId TEXT NOT NULL UNIQUE,
    EventType TEXT NOT NULL,
    ProcessedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ProcessedSuccessfully BOOLEAN NOT NULL DEFAULT FALSE
);

-- Index pour la performance
CREATE INDEX IF NOT EXISTS idx_stripecustomers_tenantid ON StripeCustomers(TenantId);
CREATE INDEX IF NOT EXISTS idx_billinginvoices_tenantid ON BillingInvoices(TenantId);
CREATE INDEX IF NOT EXISTS idx_webhookevents_stripeid ON WebhookEvents(StripeEventId);
