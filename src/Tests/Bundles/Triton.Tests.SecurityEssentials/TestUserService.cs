using TheXDS.Triton.InMemory.Services;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.SecurityEssentials;

internal class TestUserService(ITransactionFactory factory) : TritonService(factory), IUserService
{
    public TestUserService() : this(new InMemoryTransFactory())
    {
    }
}