-- Database: Konta_Finance
-- Schema: finance

CREATE SCHEMA IF NOT EXISTS finance;

-- Table des Comptes (Plan Comptable)
CREATE TABLE IF NOT EXISTS finance.Accounts (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL, -- AccountType Enum
    ParentId UUID REFERENCES finance.Accounts(Id),
    FullPath TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    UNIQUE(TenantId, Code)
);

-- Table des Journaux
CREATE TABLE IF NOT EXISTS finance.Journals (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    Code TEXT NOT NULL,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL, -- JournalType Enum
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    UNIQUE(TenantId, Code)
);

-- Table des Écritures (JournalEntries)
CREATE TABLE IF NOT EXISTS finance.JournalEntries (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    JournalId UUID NOT NULL REFERENCES finance.Journals(Id),
    EntryDate DATE NOT NULL,
    Reference TEXT,
    Description TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Table des Lignes d'Écritures (EntryLines)
CREATE TABLE IF NOT EXISTS finance.EntryLines (
    Id UUID PRIMARY KEY,
    EntryId UUID NOT NULL REFERENCES finance.JournalEntries(Id),
    AccountId UUID NOT NULL REFERENCES finance.Accounts(Id),
    Label TEXT,
    Debit DECIMAL(18,2) NOT NULL DEFAULT 0,
    Credit DECIMAL(18,2) NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index pour la performance
CREATE INDEX IF NOT EXISTS idx_accounts_tenant ON finance.Accounts(TenantId);
CREATE INDEX IF NOT EXISTS idx_journalentries_tenant_date ON finance.JournalEntries(TenantId, EntryDate);
CREATE INDEX IF NOT EXISTS idx_entrylines_account ON finance.EntryLines(AccountId);
CREATE INDEX IF NOT EXISTS idx_entrylines_entry ON finance.EntryLines(EntryId);
