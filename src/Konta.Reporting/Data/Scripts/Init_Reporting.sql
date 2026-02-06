-- Database: Konta_Reporting
-- Schema: reporting

CREATE SCHEMA IF NOT EXISTS reporting;

-- Table des Snapshots (Historisation des indicateurs pour performance)
CREATE TABLE IF NOT EXISTS reporting.ReportingSnapshots (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    SnapshotType TEXT NOT NULL, -- 'MonthlySummary', 'YearlySummary'
    Data JSONB NOT NULL, -- Données agrégées au format JSON pour flexibilité
    ReferenceDate DATE NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index pour les recherches analytiques rapides
CREATE INDEX IF NOT EXISTS idx_reporting_tenant_date ON reporting.ReportingSnapshots(TenantId, ReferenceDate);
CREATE INDEX IF NOT EXISTS idx_reporting_type ON reporting.ReportingSnapshots(SnapshotType);
