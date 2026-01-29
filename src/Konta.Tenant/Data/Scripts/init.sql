CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Function to update 'updated_at' automatically
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ language 'plpgsql';

--------------------------------------------------------------------------------
-- Subscription Plans (Catalog)
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS SubscriptionPlans (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Code TEXT NOT NULL UNIQUE, -- 'discovery', 'basic', 'advanced', 'premium', 'expertise'
    Name TEXT NOT NULL,
    Description TEXT,
    MonthlyPrice DECIMAL(18, 2) NOT NULL,
    YearlyPrice DECIMAL(18, 2) NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE TRIGGER update_subscription_plans_modtime
    BEFORE UPDATE ON SubscriptionPlans
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- Plan Limits (Configuration)
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PlanLimits (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    PlanId UUID NOT NULL REFERENCES SubscriptionPlans(Id) ON DELETE CASCADE,
    LimitKey TEXT NOT NULL, -- 'MaxUsers', 'StorageGB', 'MaxEntitiesPerMonth', etc.
    LimitValue INT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    UNIQUE(PlanId, LimitKey)
);

--------------------------------------------------------------------------------
-- Tenants
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Tenants (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name TEXT NOT NULL,
    Identifier TEXT NOT NULL UNIQUE, -- Domain or slug
    Industry TEXT,
    Address TEXT,
    Siret TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN DEFAULT TRUE
);

CREATE TRIGGER update_tenants_modtime
    BEFORE UPDATE ON Tenants
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- Subscriptions
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Subscriptions (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    TenantId UUID NOT NULL REFERENCES Tenants(Id) ON DELETE CASCADE,
    PlanId UUID NOT NULL REFERENCES SubscriptionPlans(Id),
    Status TEXT NOT NULL, -- 'Active', 'Canceled', 'PastDue', 'Trial'
    StartDate TIMESTAMP WITH TIME ZONE NOT NULL,
    EndDate TIMESTAMP WITH TIME ZONE,
    BillingCycle TEXT NOT NULL, -- 'Monthly', 'Yearly'
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    UNIQUE(TenantId) -- One active subscription per tenant
);

CREATE TRIGGER update_subscriptions_modtime
    BEFORE UPDATE ON Subscriptions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- Tenant Usage (Current state)
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS TenantUsage (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    TenantId UUID NOT NULL REFERENCES Tenants(Id) ON DELETE CASCADE,
    MetricKey TEXT NOT NULL, -- 'UserCount', 'StorageMB', etc.
    CurrentValue BIGINT NOT NULL DEFAULT 0,
    LastUpdated TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(TenantId, MetricKey)
);

--------------------------------------------------------------------------------
-- SEED DATA: Default Plans
--------------------------------------------------------------------------------

-- 1. Discovery (Découverte)
INSERT INTO SubscriptionPlans (Id, Code, Name, Description, MonthlyPrice, YearlyPrice)
VALUES ('7b4e9f1a-6d2c-4b5a-8e3d-0f9a1b2c3d4e', 'discovery', 'Découverte', 'Pour explorer les fonctionnalités de base', 0.00, 0.00)
ON CONFLICT (Code) DO NOTHING;

-- 2. Basic (Basique)
INSERT INTO SubscriptionPlans (Id, Code, Name, Description, MonthlyPrice, YearlyPrice)
VALUES ('8c5f0a2b-7e3d-5c6b-9f4e-1a0b2c3d4e5f', 'basic', 'Basique', 'Essentiel pour les petites structures', 29.00, 290.00)
ON CONFLICT (Code) DO NOTHING;

-- 3. Advanced (Avancée)
INSERT INTO SubscriptionPlans (Id, Code, Name, Description, MonthlyPrice, YearlyPrice)
VALUES ('9d601b3c-8f4e-6d7c-0a5f-2b1c3d4e5f6a', 'advanced', 'Avancée', 'Gestion complète pour entreprises en croissance', 59.00, 590.00)
ON CONFLICT (Code) DO NOTHING;

-- 4. Premium (Premium)
INSERT INTO SubscriptionPlans (Id, Code, Name, Description, MonthlyPrice, YearlyPrice)
VALUES ('0e712c4d-9a5f-7e8d-1b60-3c2d4e5f6a7b', 'premium', 'Premium', 'Performance et support prioritaire', 129.00, 1290.00)
ON CONFLICT (Code) DO NOTHING;

-- 5. Expertise (Expertise)
INSERT INTO SubscriptionPlans (Id, Code, Name, Description, MonthlyPrice, YearlyPrice)
VALUES ('1f823d5e-0b60-8f9e-2c71-4d3e5f6a7b8c', 'expertise', 'Expertise', 'Sur mesure pour les grands comptes', 299.00, 2990.00)
ON CONFLICT (Code) DO NOTHING;

-- SEED LIMITS (Exemples)
-- Discovery limits
INSERT INTO PlanLimits (PlanId, LimitKey, LimitValue) VALUES 
('7b4e9f1a-6d2c-4b5a-8e3d-0f9a1b2c3d4e', 'MaxUsers', 1),
('7b4e9f1a-6d2c-4b5a-8e3d-0f9a1b2c3d4e', 'StorageGB', 1)
ON CONFLICT DO NOTHING;

-- Basic limits
INSERT INTO PlanLimits (PlanId, LimitKey, LimitValue) VALUES 
('8c5f0a2b-7e3d-5c6b-9f4e-1a0b2c3d4e5f', 'MaxUsers', 3),
('8c5f0a2b-7e3d-5c6b-9f4e-1a0b2c3d4e5f', 'StorageGB', 5)
ON CONFLICT DO NOTHING;
