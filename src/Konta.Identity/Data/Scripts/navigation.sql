-- ============================================================================
-- Konta Identity - Navigation Menu Management
-- ============================================================================

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

-- Seed initial menu items (based on current hardcoded MainLayout.vue)
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
