﻿#pragma warning disable CS1591

using System.Reflection;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Diagnostics.Middleware;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.Diagnostics;

public abstract class JournalTestsBase
{
    protected class TestActorProvider : IActorProvider
    {
        public string? GetCurrentActor() => "Test executor";
    }

    protected class TestOldValueProvider : IOldValueProvider
    {
        public IEnumerable<KeyValuePair<PropertyInfo, object?>>? GetOldValues(Model? entity)
        {
            if (entity is null) yield break;
            foreach (var j in entity.GetType().GetProperties().Where(p => p.CanRead))
            {
                yield return new KeyValuePair<PropertyInfo, object?>(j, j.PropertyType.Default());
            }
        }
    }

    protected static IEnumerable<object[]?> GetTestCases 
    { 
        get
        {
            yield return [CrudAction.Create, true,  false];
            yield return [CrudAction.Read,   true,  false];
            yield return [CrudAction.Update, true,  false];
            yield return [CrudAction.Delete, true,  false];
            yield return [CrudAction.Commit, true,  false];
            yield return [CrudAction.Create, false, false];
            yield return [CrudAction.Read,   false, false];
            yield return [CrudAction.Update, false, false];
            yield return [CrudAction.Delete, false, false];
            yield return [CrudAction.Commit, false, false];
            yield return [CrudAction.Create, true,  true];
            yield return [CrudAction.Read,   true,  true];
            yield return [CrudAction.Update, true,  true];
            yield return [CrudAction.Delete, true,  true];
            yield return [CrudAction.Commit, true,  true];
            yield return [CrudAction.Create, false, true];
            yield return [CrudAction.Read,   false, true];
            yield return [CrudAction.Update, false, true];
            yield return [CrudAction.Delete, false, true];
            yield return [CrudAction.Commit, false, true];
        }
    }
}