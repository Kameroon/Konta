-- ============================================================================
-- Konta Identity Database Initialization Script
-- Schema: identity
-- ============================================================================

CREATE SCHEMA IF NOT EXISTS identity;

--------------------------------------------------------------------------------
-- Tenants
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.Tenants (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name TEXT NOT NULL,
    Identifier TEXT,
    Industry TEXT,
    Address TEXT,
    Siret TEXT,
    Plan TEXT NOT NULL DEFAULT 'Free',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

--------------------------------------------------------------------------------
-- Users
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TenantId UUID NOT NULL REFERENCES identity.Tenants(Id) ON DELETE CASCADE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'Admin',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_Users_Email ON identity.Users(Email);
CREATE INDEX IF NOT EXISTS IX_Users_TenantId ON identity.Users(TenantId);

--------------------------------------------------------------------------------
-- Roles
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.Roles (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    TenantId UUID NOT NULL REFERENCES identity.Tenants(Id) ON DELETE CASCADE,
    Name TEXT NOT NULL,
    Description TEXT,
    IsDefault BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_Roles_TenantId ON identity.Roles(TenantId);

--------------------------------------------------------------------------------
-- Permissions (Global System Permissions)
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.Permissions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    SystemName TEXT NOT NULL UNIQUE,
    DisplayName TEXT NOT NULL,
    "Group" TEXT NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

--------------------------------------------------------------------------------
-- RolePermissions
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.RolePermissions (
    RoleId UUID NOT NULL REFERENCES identity.Roles(Id) ON DELETE CASCADE,
    PermissionId UUID NOT NULL REFERENCES identity.Permissions(Id) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, PermissionId)
);

--------------------------------------------------------------------------------
-- UserRoles
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.UserRoles (
    UserId UUID NOT NULL REFERENCES identity.Users(Id) ON DELETE CASCADE,
    RoleId UUID NOT NULL REFERENCES identity.Roles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UserId, RoleId)
);

--------------------------------------------------------------------------------
-- RefreshTokens
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.RefreshTokens (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL REFERENCES identity.Users(Id) ON DELETE CASCADE,
    Token TEXT NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsRevoked BOOLEAN DEFAULT FALSE,
    ReplacedByToken TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_RefreshTokens_UserId ON identity.RefreshTokens(UserId);
CREATE INDEX IF NOT EXISTS IX_RefreshTokens_Token ON identity.RefreshTokens(Token);

--------------------------------------------------------------------------------
-- SEED Basic Permissions
--------------------------------------------------------------------------------
INSERT INTO identity.Permissions (SystemName, DisplayName, "Group") VALUES
('users.read', 'Voir les utilisateurs', 'Utilisateurs'),
('users.write', 'Gérer les utilisateurs', 'Utilisateurs'),
('roles.read', 'Voir les rôles', 'Sécurité'),
('roles.write', 'Gérer les rôles', 'Sécurité')
ON CONFLICT (SystemName) DO NOTHING;
