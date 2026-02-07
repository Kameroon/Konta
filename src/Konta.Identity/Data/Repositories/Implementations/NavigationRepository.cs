using Dapper;
using Konta.Identity.Data.Repositories.Interfaces;
using Konta.Identity.Models;
using Konta.Shared.Data;
using Konta.Shared.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace Konta.Identity.Data.Repositories.Implementations;

public class NavigationRepository : BaseRepository<NavigationRepository>, INavigationRepository
{
    public NavigationRepository(IDbConnectionFactory connectionFactory, ILogger<NavigationRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    public async Task<IEnumerable<NavigationItem>> GetAllAsync()
    {
        const string sql = "SELECT * FROM identity.NavigationItems ORDER BY DisplayOrder";
        using var connection = CreateConnection(sql);
        return await connection.QueryAsync<NavigationItem>(sql);
    }

    public async Task<NavigationItem?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM identity.NavigationItems WHERE Id = @Id";
        using var connection = CreateConnection(sql, new { Id = id });
        return await connection.QueryFirstOrDefaultAsync<NavigationItem>(sql, new { Id = id });
    }

    public async Task<bool> UpdateAsync(NavigationItem item)
    {
        const string sql = @"
            UPDATE identity.NavigationItems 
            SET Label = @Label, 
                Path = @Path, 
                Icon = @Icon, 
                RequiredPermission = @RequiredPermission, 
                RequiredRole = @RequiredRole, 
                DisplayOrder = @DisplayOrder, 
                IsVisible = @IsVisible,
                UpdatedAt = NOW()
            WHERE Id = @Id";
        
        using var connection = CreateConnection(sql, item);
        var affected = await connection.ExecuteAsync(sql, item);
        return affected > 0;
    }
}
