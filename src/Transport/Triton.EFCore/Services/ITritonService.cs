using TheXDS.Triton.Services;

namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Defines a set of members to be implemented by a type that exposes access to
/// an Entity Framework data context.
/// </summary>
/// <typeparam name="T">
/// Type of <see cref="DbContext"/> that this service provides access to.
/// </typeparam>
public interface ITritonService<out T> : ITritonService where T : DbContext
{
}