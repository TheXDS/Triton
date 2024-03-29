﻿namespace TheXDS.Triton.Services.Base;

/// <summary>
/// Define una serie de miembros a implementar por un tipo que permita
/// realizar operaciones de lectura y de escritura basadas en
/// transacción sobre una base de datos.
/// </summary>
public interface ICrudReadWriteTransaction : ICrudReadTransaction, ICrudWriteTransaction
{
}