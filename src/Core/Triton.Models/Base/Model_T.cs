﻿namespace TheXDS.Triton.Models.Base;

/// <summary>
/// Clase base para todos los modelos de Triton que expone un campo llave a
/// utilizar como Id de la entidad.
/// </summary>
/// <typeparam name="T">Tipo de campo llave de la entidad.</typeparam>
public abstract class Model<T> : Model where T : IComparable<T>, IEquatable<T>
{
    /// <inheritdoc/>
    public sealed override string IdAsString => Id?.ToString() ?? string.Empty;

    /// <summary>
    /// Obtiene o establece el campo llave de esta entidad.
    /// </summary>
    public T Id { get; set; }

    /// <summary>
    /// Inicializa una nueva instancia del modelo, sin establecer el valor
    /// del campo llave.
    /// </summary>
    /// <remarks>
    /// Utilice este constructor predeterminado únicamente al crear nuevas
    /// entidades, o cuando intencionalmente no se debe especificar el
    /// valor del campo llave de la entidad.
    /// </remarks>
    protected Model()
    {
        Id = default!;
    }

    /// <summary>
    /// Inicializa una nueva instancia del modelo, estableciendo el valor
    /// del campo llave.
    /// </summary>
    /// <param name="id">Valor del campo llave a establecer.</param>
    /// <exception cref="ArgumentNullException">
    /// Se produce si <paramref name="id"/> es <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Si desea dejar a propósito el valor del campo llave en su valor
    /// predeterminado (por ejemplo, para campos llave de tipos numéricos
    /// primitivos, <see cref="string"/> o <see cref="Guid"/>), utilice el
    /// constructor predetermiando de <see cref="Model{T}"/>.
    /// </remarks>
    protected Model(T id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
}
