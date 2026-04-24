using Microsoft.EntityFrameworkCore;
using TheXDS.Triton.Models;

namespace TheXDS.Triton.SecurityEssentials.Ef.Models;

/// <summary>
/// Defines a set of members to be implemented by a
/// <see cref="DbContext"/> that includes the necessary tables to
/// store user authentication and permission information.
/// </summary>
public interface IUserDbContext
{
    /// <summary>
    /// Gets or sets a reference to the <see cref="DbSet{TEntity}"/>
    /// that stores objects of type <see cref="LoginCredential"/>.
    /// </summary>
    DbSet<LoginCredential> LoginCredentials { get; set; }

    /// <summary>
    /// Gets or sets a reference to the <see cref="DbSet{TEntity}"/>
    /// that stores objects of type <see cref="MultiFactorEntry"/>.
    /// </summary>
    DbSet<MultiFactorEntry> MfaEntries { get; set; }

    /// <summary>
    /// Gets or sets a reference to the <see cref="DbSet{TEntity}"/>
    /// that stores objects of type <see cref="SecurityDescriptor"/>.
    /// </summary>
    DbSet<SecurityDescriptor> SecurityDescriptors { get; set; }

    /// <summary>
    /// Gets or sets a reference to the <see cref="DbSet{TEntity}"/>
    /// that stores objects of type <see cref="Session"/>.
    /// </summary>
    DbSet<Session> Sessions { get; set; }

    /// <summary>
    /// Gets or sets a reference to the <see cref="DbSet{TEntity}"/>
    /// that stores objects of type <see cref="UserGroup"/>.
    /// </summary>
    DbSet<UserGroup> UserGroups { get; set; }

    /// <summary>
    /// Gets or sets a reference to the <see cref="DbSet{TEntity}"/>
    /// that stores objects of type <see cref="UserGroupMembership"/>.
    /// </summary>
    DbSet<UserGroupMembership> UserGroupMemberships { get; set; }
}
