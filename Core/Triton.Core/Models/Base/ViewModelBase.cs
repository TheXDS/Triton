﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TheXDS.Triton.Core.Annotations;
using TheXDS.Triton.Core.Component;

namespace TheXDS.Triton.Core.Models.Base
{
    public abstract class ViewModelBase<TModel, TKey> : NotifyPropertyChanged
        where TModel : ModelBase<TKey>, new()
        where TKey : struct, IComparable<TKey>
    {
        private static readonly HashSet<PropertyInfo> ModelProperties = new HashSet<PropertyInfo>();
        static ViewModelBase()
        {
            foreach (var j in typeof(TModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite))
            {
                ModelProperties.Add(j);
            }
        }

        /// <summary>
        ///     Instancia de la entidad controlada por este ViewModel.
        /// </summary>
        protected TModel Entity { get; private set; }

        public void New()
        {
            Entity = new TModel();
            Refresh();
        }

        public void Edit([NotNull]TModel entity)
        {
            Entity = entity;
            Refresh();
        }

        public ViewModelBase()
        {
            New();
        }

        public void Refresh()
        {
            lock (Entity)
            {
                foreach (var j in ModelProperties)
                {
                    OnPropertyChanged(j.Name);
                }
            }
        }

        /// <summary>
        ///     Obtiene un valor que determina si este elemento es nuevo.
        /// </summary>
        public bool IsNew => Entity.Id.CompareTo(default) == 0;

        /// <summary>
        ///     Obtiene un valor que determina si este elemento puede ser
        ///     borrado.
        /// </summary>
        public virtual bool CanBeDeleted => true;

        /// <summary>
        ///     Obtiene un valor que determina si este elemento puede ser
        ///     eliminado por completo de la base de datos.
        /// </summary>
        public virtual bool CanBePurged => (Entity as ISoftDeletable)?.IsDeleted ?? true;
    }
}