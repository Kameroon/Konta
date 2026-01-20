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
-- Tenants
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Tenants (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    Name TEXT NOT NULL,
    Plan TEXT NOT NULL DEFAULT 'Free',
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE TRIGGER update_tenants_modtime
    BEFORE UPDATE ON Tenants
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- Users
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Users (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    TenantId UUID NOT NULL REFERENCES Tenants(Id) ON DELETE CASCADE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    FirstName TEXT NOT NULL,
    LastName TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'Admin', -- Legacy simple role, kept for backup
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_Users_Email ON Users(Email);
CREATE INDEX IF NOT EXISTS IX_Users_TenantId ON Users(TenantId);

CREATE TRIGGER update_users_modtime
    BEFORE UPDATE ON Users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- Roles
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Roles (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    TenantId UUID NOT NULL REFERENCES Tenants(Id) ON DELETE CASCADE,
    Name TEXT NOT NULL,
    Description TEXT,
    IsDefault BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_Roles_TenantId ON Roles(TenantId);

CREATE TRIGGER update_roles_modtime
    BEFORE UPDATE ON Roles
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- Permissions (Global System Permissions)
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Permissions (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    SystemName TEXT NOT NULL UNIQUE, -- e.g. "invoices.read"
    DisplayName TEXT NOT NULL,
    "Group" TEXT NOT NULL,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE TRIGGER update_permissions_modtime
    BEFORE UPDATE ON Permissions
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

--------------------------------------------------------------------------------
-- RolePermissions
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS RolePermissions (
    RoleId UUID NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PermissionId UUID NOT NULL REFERENCES Permissions(Id) ON DELETE CASCADE,
    PRIMARY KEY (RoleId, PermissionId)
);

--------------------------------------------------------------------------------
-- UserRoles
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS UserRoles (
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    RoleId UUID NOT NULL REFERENCES Roles(Id) ON DELETE CASCADE,
    PRIMARY KEY (UserId, RoleId)
);

--------------------------------------------------------------------------------
-- RefreshTokens
--------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS RefreshTokens (
    Id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    UserId UUID NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    Token TEXT NOT NULL UNIQUE,
    ExpiresAt TIMESTAMP WITH TIME ZONE NOT NULL,
    IsRevoked BOOLEAN DEFAULT FALSE,
    ReplacedByToken TEXT,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE
);

CREATE INDEX IF NOT EXISTS IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IF NOT EXISTS IX_RefreshTokens_Token ON RefreshTokens(Token);

CREATE TRIGGER update_refreshtokens_modtime
    BEFORE UPDATE ON RefreshTokens
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- SEED Basic Permissions
INSERT INTO Permissions (SystemName, DisplayName, "Group") VALUES
('users.read', 'Voir les utilisateurs', 'Utilisateurs'),
('users.write', 'Gérer les utilisateurs', 'Utilisateurs'),
('roles.read', 'Voir les rôles', 'Sécurité'),
('roles.write', 'Gérer les rôles', 'Sécurité')
ON CONFLICT (SystemName) DO NOTHING;
