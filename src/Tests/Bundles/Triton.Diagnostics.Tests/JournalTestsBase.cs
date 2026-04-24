using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.Diagnostics;

internal abstract class JournalTestsBase
{
    protected static IEnumerable<object[]?> GetTestCases
    {
        get
        {
            yield return [CrudAction.Write, true,  false];
            yield return [CrudAction.Read,   true,  false];
            yield return [CrudAction.Commit, true,  false];
            yield return [CrudAction.Write, false, false];
            yield return [CrudAction.Read,   false, false];
            yield return [CrudAction.Commit, false, false];
            yield return [CrudAction.Write, true,  true];
            yield return [CrudAction.Read,   true,  true];
            yield return [CrudAction.Commit, true,  true];
            yield return [CrudAction.Write, false, true];
            yield return [CrudAction.Read,   false, true];
            yield return [CrudAction.Commit, false, true];
        }
    }
}
