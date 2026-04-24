namespace TheXDS.Triton.Dapper.Services;

/// <summary>
/// Contains information about metadata overrides to apply to a specific data model.
/// </summary>
/// <param name="TableName">Table name to use when building the required SQL statements.</param>
/// <param name="Properties">
/// Dictionary of overrides for the data model properties.
/// </param>
public record struct DapperModelDescriptor(string TableName, IDictionary<string, string>? Properties);
