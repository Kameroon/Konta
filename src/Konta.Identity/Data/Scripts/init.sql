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
    Name TEXT NOT NULL,
    Description TEXT,
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
-- NavigationItems
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS identity.NavigationItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Label TEXT NOT NULL,
    Path TEXT NOT NULL,
    Icon TEXT,
    RequiredPermission TEXT, -- Nom technique de la permission (ex: 'users.read')
    RequiredRole TEXT,       -- Rôle minimum ou spécifique (ex: 'SuperAdmin')
    DisplayOrder INTEGER NOT NULL DEFAULT 0,
    IsVisible BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_NavigationItems_RequiredRole ON identity.NavigationItems(RequiredRole);

--------------------------------------------------------------------------------
-- SEED Basic Permissions
--------------------------------------------------------------------------------
INSERT INTO identity.Permissions (SystemName, Name, Description) VALUES
('users.read', 'Voir les utilisateurs', 'Utilisateurs'),
('users.write', 'Gérer les utilisateurs', 'Utilisateurs'),
('roles.read', 'Voir les rôles', 'Sécurité'),
('roles.write', 'Gérer les rôles', 'Sécurité')
ON CONFLICT (SystemName) DO NOTHING;

--------------------------------------------------------------------------------
-- SEED Initial Navigation Items
--------------------------------------------------------------------------------
INSERT INTO identity.NavigationItems (Label, Path, Icon, RequiredRole, DisplayOrder) VALUES
('Tableau de bord', '/app/dashboard', 'fas fa-th-large', NULL, 10),
('Téléchargement', '/app/download', 'fas fa-upload', 'User', 20),
('Documents', '/app/documents', 'fas fa-file-alt', 'User', 30),
('Partenaires', '/app/companies', 'fas fa-building', 'User', 40),
('Profil', '/app/profile', 'fas fa-user-circle', 'User', 50),
('Données extraites', '/app/extracted-data', 'fas fa-database', 'SuperAdmin', 60),
('Entreprises', '/app/companies', 'fas fa-building', 'SuperAdmin', 70),
('Utilisateurs', '/app/admin', 'fas fa-users', 'SuperAdmin', 80),
('Paramètres', '/app/settings', 'fas fa-cog', 'SuperAdmin', 90)
ON CONFLICT DO NOTHING;
