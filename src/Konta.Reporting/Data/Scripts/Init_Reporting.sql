-- Database: Konta_Reporting
-- Description: Schéma pour le microservice de Reporting et Analytics

-- Table des Snapshots (Historisation des indicateurs pour performance)
CREATE TABLE IF NOT EXISTS ReportingSnapshots (
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
CREATE INDEX IF NOT EXISTS idx_reporting_tenant_date ON ReportingSnapshots(TenantId, ReferenceDate);
CREATE INDEX IF NOT EXISTS idx_reporting_type ON ReportingSnapshots(SnapshotType);

-- Note: Ce microservice effectue également des requêtes cross-database (via DB Links ou agrégation applicative)
-- pour récupérer les données fraîches depuis Konta_Finance_Core et Konta_Finance.
