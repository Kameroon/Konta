-- Database: Konta_Ocr
-- Description: Schéma pour le microservice OCR et extraction intelligente

-- Table des Jobs
CREATE TABLE IF NOT EXISTS ExtractionJobs (
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

-- Table des Factures Extraites
CREATE TABLE IF NOT EXISTS ExtractedInvoices (
    Id UUID PRIMARY KEY,
    JobId UUID NOT NULL REFERENCES ExtractionJobs(Id),
    VendorName TEXT,
    InvoiceNumber TEXT,
    InvoiceDate DATE,
    TotalAmountHt DECIMAL(18,2),
    TotalAmountTtc DECIMAL(18,2),
    VatAmount DECIMAL(18,2),
    Currency TEXT,
    RawJson TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des RIB Extraits
CREATE TABLE IF NOT EXISTS ExtractedRibs (
    Id UUID PRIMARY KEY,
    JobId UUID NOT NULL REFERENCES ExtractionJobs(Id),
    BankName TEXT,
    Iban TEXT,
    Bic TEXT,
    AccountHolder TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index
CREATE INDEX IF NOT EXISTS idx_ocr_jobs_tenant ON ExtractionJobs(TenantId);
CREATE INDEX IF NOT EXISTS idx_ocr_jobs_status ON ExtractionJobs(Status);
CREATE INDEX IF NOT EXISTS idx_extracted_invoices_job ON ExtractedInvoices(JobId);
