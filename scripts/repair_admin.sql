-- SCRIPT DE RÉPARATION DE L'ADMINISTRATEUR
-- Ce script assure que l'administrateur de test possède les bons identifiants et rôles

CREATE SCHEMA IF NOT EXISTS identity;

CREATE TABLE IF NOT EXISTS identity.UserRoles (
    UserId UUID NOT NULL,
    RoleId UUID NOT NULL,
    PRIMARY KEY (UserId, RoleId)
);

UPDATE identity.Users 
SET Email = 'admin@kontacorp.com',
    PasswordHash = '$2a$11$OvHGeoGRS694M.8YPOrKUuFaHQHh9ixo7VjZeYCSk4IGmqF9Vif/6' -- Hash valide pour "Password123!"
WHERE Email = 'admin@kontacorp.com' 
   OR Id = '7c9e6679-7425-40de-944b-e07fc1f90ae7';

-- Si l'utilisateur n'existe pas, on le recrée proprement
INSERT INTO identity.Users (Id, TenantId, Email, PasswordHash, FirstName, LastName, Role)
SELECT '7c9e6679-7425-40de-944b-e07fc1f90ae7', 'd290f1ee-6c54-4b01-90e6-d701748f0851', 'admin@kontacorp.com', '$2a$11$OvHGeoGRS694M.8YPOrKUuFaHQHh9ixo7VjZeYCSk4IGmqF9Vif/6', 'Jean', 'Admin', 'Admin'
WHERE NOT EXISTS (SELECT 1 FROM identity.Users WHERE Email = 'admin@kontacorp.com');

-- S'assurer que les rôles existent
INSERT INTO identity.Roles (Id, TenantId, Name, Description, IsDefault) 
SELECT 'a1e9c8a2-1234-4a5b-bcde-f0123456789a', 'd290f1ee-6c54-4b01-90e6-d701748f0851', 'Admin', 'Administrateur complet', FALSE
WHERE NOT EXISTS (SELECT 1 FROM identity.Roles WHERE Name = 'Admin');

-- Assigner le rôle Admin à l'utilisateur admin
INSERT INTO identity.UserRoles (UserId, RoleId)
SELECT '7c9e6679-7425-40de-944b-e07fc1f90ae7', 'a1e9c8a2-1234-4a5b-bcde-f0123456789a'
WHERE NOT EXISTS (SELECT 1 FROM identity.UserRoles WHERE UserId = '7c9e6679-7425-40de-944b-e07fc1f90ae7' AND RoleId = 'a1e9c8a2-1234-4a5b-bcde-f0123456789a');
