-- Database: Konta_Ocr
-- Schema: ocr

CREATE SCHEMA IF NOT EXISTS ocr;

-- Table des Jobs
CREATE TABLE IF NOT EXISTS ocr.extractionjobs (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0, -- JobStatus Enum
    DetectedType INTEGER NOT NULL DEFAULT 0, -- DocumentType Enum
    ErrorMessage TEXT,
    ProcessedAt TIMESTAMP WITH TIME ZONE,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des Factures Extraites (complète avec tous les champs du modèle)
DROP TABLE IF EXISTS ocr.extractedinvoices CASCADE;
CREATE TABLE ocr.extractedinvoices (
    Id UUID PRIMARY KEY,
    JobId UUID NOT NULL REFERENCES ocr.extractionjobs(Id),
    VendorName TEXT,
    VendorSiret TEXT,
    CustomerSiret TEXT,
    VendorVatNumber TEXT,
    InvoiceNumber TEXT,
    InvoiceDate DATE,
    DueDate DATE,
    TotalAmountHt DECIMAL(18,2),
    TotalAmountTtc DECIMAL(18,2),
    VatAmount DECIMAL(18,2),
    Currency TEXT DEFAULT 'EUR',
    ConfidenceScore INTEGER DEFAULT 0,
    RawJson JSONB,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des RIB Extraits
CREATE TABLE IF NOT EXISTS ocr.extractedribs (
    Id UUID PRIMARY KEY,
    JobId UUID NOT NULL REFERENCES ocr.extractionjobs(Id),
    BankName TEXT,
    Iban TEXT,
    Bic TEXT,
    AccountHolder TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index
CREATE INDEX IF NOT EXISTS idx_ocr_jobs_tenant ON ocr.extractionjobs(TenantId);
CREATE INDEX IF NOT EXISTS idx_ocr_jobs_status ON ocr.extractionjobs(Status);
CREATE INDEX IF NOT EXISTS idx_extracted_invoices_job ON ocr.extractedinvoices(JobId);

-- Reset les jobs en échec/bloqués pour les retraiter
UPDATE ocr.extractionjobs SET Status = 0, ErrorMessage = NULL, ProcessedAt = NULL, UpdatedAt = CURRENT_TIMESTAMP
WHERE Status IN (1, 3); -- 1 = Processing (bloqué), 3 = Failed
