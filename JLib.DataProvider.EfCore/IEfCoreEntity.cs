using JLib.Reflection;
using Microsoft.EntityFrameworkCore;

namespace JLib.DataProvider.EfCore;

/// <summary>
/// marks the implementing <see cref="TypeValueType"/> to be added to the <see cref="AutoDbContext"/>
/// </summary>
public interface IEfCoreEntityType : ITypeValueType;

/// <summary>
/// automatically finds all <see cref="IEfCoreEntityType"/>s and adds them to the <see cref="AutoDbContext"/>
/// </summary>
public class AutoDbContext(ITypeCache typeCache, DbContextOptions options) : DbContext(options)
{
    /// <summary>
    /// <inheritdoc cref="DbContext.OnModelCreating"/>
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entities in typeCache.All<IEfCoreEntityType>())
            modelBuilder.Entity(entities.Value);
    }
}