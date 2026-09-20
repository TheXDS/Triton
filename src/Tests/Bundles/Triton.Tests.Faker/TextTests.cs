using NUnit.Framework;
using TheXDS.Triton.Faker;

namespace TheXDS.Triton.Tests.Faker;

internal class TextTests
{
    [Test]
    public void Lorem_Test()
    {
        Assert.That(Text.Lorem(1), Is.Not.Empty);
        Assert.That(Text.Lorem(200).Length, Is.GreaterThan(Text.Lorem(100).Length));
    }

    [Test]
    public void Lorem_contract_Test()
    {
        Assert.That(((Action)(() => Text.Lorem(0, 1, 1))), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(((Action)(() => Text.Lorem(1, 0, 1))), Throws.InstanceOf<ArgumentOutOfRangeException>());
        Assert.That(((Action)(() => Text.Lorem(1, 1, 0))), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }
}
