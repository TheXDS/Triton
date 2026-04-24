using Microsoft.EntityFrameworkCore;
using TheXDS.Triton.EFCore.Services;
using TheXDS.Triton.SecurityEssentials.Ef.Models;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.SecurityEssentials.Ef.Services.Base;

/// <summary>
/// Defines a set of members to be implemented by a type that exposes
/// access to a data context that stores authentication and user
/// permission information.
/// </summary>
/// <typeparam name="T">
/// Type of <see cref="DbContext"/> that contains the
/// <see cref="DbSet{TEntity}"/> needed to implement user authentication
/// and permissions.
/// </typeparam>
public interface IEfUserService<T> : IUserService, ITritonService<T> where T : DbContext, IUserDbContext, new()
{
}
