using TheXDS.Triton.Services;

namespace TheXDS.Triton.Dapper.Services;

/// <summary>
/// Implements an <see cref="ITransactionFactory"/> that creates and manages
/// data operations using the Dapper library.
/// </summary>
/// <param name="factory">
/// Database connection factory to use.
/// </param>
public class DapperTransactionFactory(IDbConnectionFactory factory) : ITransactionFactory
{
    private readonly IDbConnectionFactory _factory = factory;
    private readonly IDictionary<Type, DapperModelDescriptor> _modelOverrides = new Dictionary<Type, DapperModelDescriptor>();

    /// <summary>
    /// Defines a delegate that allows configuring a metadata override dictionary
    /// for one or more models.
    /// </summary>
    /// <param name="overridesDictionary">
    /// Metadata override dictionary for each model.
    /// </param>
    public delegate void ModelOverrideConfigurator(IDictionary<Type, DapperModelDescriptor> overridesDictionary);

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DapperTransactionFactory"/> class.
    /// </summary>
    /// <param name="factory">
    /// Database connection factory to use.
    /// </param>
    /// <param name="modelOverrideConfigurator">
    /// Delegate for configuring metadata overrides for models.
    /// </param>
    public DapperTransactionFactory(IDbConnectionFactory factory, ModelOverrideConfigurator? modelOverrideConfigurator)
        : this(factory)
    {
        modelOverrideConfigurator?.Invoke(_modelOverrides);
    }

    /// <summary>
    /// Gets a dictionary of overrides to apply when processing data models.
    /// </summary>
    public IDictionary<Type, DapperModelDescriptor> ModelOverrides => _modelOverrides;

    /// <inheritdoc/>
    public ICrudReadWriteTransaction GetTransaction(IMiddlewareRunner configuration)
    {
        return new DapperTransaction(_factory.OpenConnection(), _modelOverrides, configuration);
    }
}
