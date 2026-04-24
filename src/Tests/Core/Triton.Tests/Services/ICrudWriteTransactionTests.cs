using Moq;
using NUnit.Framework;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;
using TheXDS.Triton.Tests.Models;

namespace TheXDS.Triton.Tests.Services;

internal class ICrudWriteTransactionTests
{
    [Test]
    public void Create_T_calls_non_generic_version()
    {
        var newEntity = new User() { Id = "NewId" };
        var writeMock = new Mock<ICrudWriteTransaction>() { CallBase = true };
        writeMock.Setup(x => x.Create((Model)newEntity)).Returns(ServiceResult.Ok).Verifiable(Times.Once);
        var result = writeMock.Object.Create(newEntity);
        Assert.That(result.IsSuccessful, Is.True);
        writeMock.Verify();
    }
}
