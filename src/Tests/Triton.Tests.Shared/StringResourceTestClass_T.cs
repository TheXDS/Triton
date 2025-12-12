namespace TheXDS.Triton.Tests;

internal abstract class StringResourceTestClass<T> : StringResourceTestClass where T : notnull
{
    protected StringResourceTestClass() : base(typeof(T))
    {
    }
}
