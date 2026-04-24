using TheXDS.Triton.Tests.EFCore.Services;

namespace TheXDS.Triton.Tests.EFCore.Tests;

internal abstract class TritonEfTestClass
{
    protected static readonly BlogService _srv = new();
}
