-- Database: Konta_Finance_Core
-- Schema: finance_core

CREATE SCHEMA IF NOT EXISTS finance_core;

-- Table des Tiers
CREATE TABLE IF NOT EXISTS finance_core.Tiers (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL, -- TierType Enum
    Email TEXT,
    Siret TEXT,
    Address TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des Factures Opérationnelles
CREATE TABLE IF NOT EXISTS finance_core.BusinessInvoices (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    TierId UUID NOT NULL REFERENCES finance_core.Tiers(Id),
    Reference TEXT NOT NULL,
    AmountHt DECIMAL(18,2) NOT NULL,
    AmountTtc DECIMAL(18,2) NOT NULL,
    VatAmount DECIMAL(18,2) NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0, -- InvoiceStatus Enum
    IsPurchase BOOLEAN NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table de Trésorerie
CREATE TABLE IF NOT EXISTS finance_core.TreasuryAccounts (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Name TEXT NOT NULL,
    AccountNumber TEXT,
    CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    Currency TEXT NOT NULL DEFAULT 'EUR',
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des Budgets
CREATE TABLE IF NOT EXISTS finance_core.Budgets (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Category TEXT NOT NULL,
    AllocatedAmount DECIMAL(18,2) NOT NULL,
    SpentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    AlertThresholdPercentage DECIMAL(5,2) NOT NULL DEFAULT 90,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des Alertes
CREATE TABLE IF NOT EXISTS finance_core.FinanceAlerts (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Title TEXT NOT NULL,
    Message TEXT NOT NULL,
    Severity TEXT NOT NULL DEFAULT 'Warning',
    IsRead BOOLEAN NOT NULL DEFAULT FALSE,
    RelatedEntityId UUID,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index
CREATE INDEX IF NOT EXISTS idx_tiers_tenant ON finance_core.Tiers(TenantId);
CREATE INDEX IF NOT EXISTS idx_invoices_tenant ON finance_core.BusinessInvoices(TenantId);
CREATE INDEX IF NOT EXISTS idx_budgets_tenant ON finance_core.Budgets(TenantId);
CREATE INDEX IF NOT EXISTS idx_treasury_tenant ON finance_core.TreasuryAccounts(TenantId);
CREATE INDEX IF NOT EXISTS idx_alerts_tenant ON finance_core.FinanceAlerts(TenantId, IsRead);
