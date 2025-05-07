using TheXDS.Triton.InMemory.Services;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.SecurityEssentials;

internal class TestUserService : TritonService, IUserService
{
    public TestUserService() : base(new InMemoryTransFactory())
    {
    }
}