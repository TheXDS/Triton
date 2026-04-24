using System.Data;

namespace TheXDS.Triton.Dapper.Services;

/// <summary>
/// Defines a set of members to be implemented by a type that allows creating and opening database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Opens a new connection to a database.
    /// </summary>
    /// <returns>An open database connection.</returns>
    IDbConnection OpenConnection();
}
