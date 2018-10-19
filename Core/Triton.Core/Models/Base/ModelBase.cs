﻿using System;
using System.ComponentModel.DataAnnotations;

namespace TheXDS.Triton.Core.Models.Base
{
    public abstract class ModelBase<T> where T : struct, IComparable<T>
    {
        /// <summary>
        ///     Campo llave primario de la entidad.
        /// </summary>
        [Key]
        public T Id { get; set; }
    }
}
