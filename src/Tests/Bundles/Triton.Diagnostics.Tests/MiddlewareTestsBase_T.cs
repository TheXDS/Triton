using TheXDS.Triton.Middleware;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.Diagnostics;

internal abstract class MiddlewareTestsBase<T> where T : ITransactionMiddleware, new()
{
    protected static (IMiddlewareRunner runner, T perfMon) Build()
    {
        return (((IMiddlewareConfigurator)new TransactionConfiguration()).Attach<T>(out var perfMon).GetRunner(), perfMon);
    }

    protected static void RunCrudAction(IMiddlewareRunner runner, CrudAction action = CrudAction.Commit)
    {
        runner.RunPrologue(action, null);
        runner.RunEpilogue(action, null);
    }
}