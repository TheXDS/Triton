using Microsoft.EntityFrameworkCore;
using TheXDS.Triton.Models;

namespace TheXDS.Triton.SecurityEssentials.Ef.Models;

/// <summary>
/// Base class for a data context that includes different
/// <see cref="DbSet{TEntity}"/> objects that store authentication and
/// user permission information.
/// </summary>
public abstract class UserDbContextBase : DbContext, IUserDbContext
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UserDbContextBase"/> class.
    /// </summary>
    protected UserDbContextBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UserDbContextBase"/> class, allowing specification of
    /// data context options.
    /// </summary>
    /// <param name="options">Data context options.</param>
    protected UserDbContextBase(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc/>
    public DbSet<LoginCredential> LoginCredentials { get; set; } = null!;

    /// <inheritdoc/>
    public DbSet<MultiFactorEntry> MfaEntries { get; set; } = null!;

    /// <inheritdoc/>
    public DbSet<SecurityDescriptor> SecurityDescriptors { get; set; } = null!;

    /// <inheritdoc/>
    public DbSet<Session> Sessions { get; set; } = null!;

    /// <inheritdoc/>
    public DbSet<UserGroup> UserGroups { get; set; } = null!;

    /// <inheritdoc/>
    public DbSet<UserGroupMembership> UserGroupMemberships { get; set; } = null!;
}
