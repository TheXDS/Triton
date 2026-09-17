# Unit Tests Instructions

## Framework & Structure

- **Test framework:** NUnit 4.x (`[Test]`, `[TestCaseSource]`, `[SetUp]`, `[TearDown]`)
- **Assertions:** `Assert.That(...)`, `Throws.InstanceOf<T>()`
- **Mocks:** Moq for interface-based dependencies
- **Test project:** one per source project under `src/Tests/`, mirroring the `src/` structure
- **Test classes are `internal`** — exposed to tests via `InternalsVisibleTo`
- **Global usings:** `NUnit.Framework` is imported globally in `GlobalUsings.cs` (in shared test project)

## File Organization

- Tests mirror the source structure under `src/Tests/`. Example:
  - `Triton.EFCore/EfContextBuilderTests.cs` tests `Triton.EFCore/EfContextBuilder.cs`
  - `Triton.Dynamic/QueryBuilderTests.cs` tests `Triton.Dynamic/QueryBuilder.cs`
  - `Triton.Dapper/DapperExtensionsTests.cs` tests `Triton.Dapper/DapperExtensions.cs`

## Test Patterns

### Simple unit test (pure function)

```csharp
namespace TheXDS.Triton.EFCore;

internal class EfContextBuilderTests
{
    [Test]
    public void Build_returns_context_when_configuration_is_valid()
    {
        Assert.That(EfContextBuilder.Build(options), Is.Not.Null);
    }

    [Test]
    public void Build_throws_when_connection_string_is_invalid()
    {
        Assert.That(() => EfContextBuilder.Build(invalidOptions),
            Throws.InstanceOf<ArgumentException>());
    }
}
```

### Testing with Moq

```csharp
[Test]
public void Service_calls_dependency_with_correct_parameters()
{
    var mockContext = new Mock<IDbContext>();
    mockContext.Setup(c => c.SaveChanges())
               .Returns(1);

    var service = new MyService(mockContext.Object);
    // ... act and assert
}
```

## Test Fixtures & Resources

- **Shared test code** lives in `src/Tests/Triton.Tests.Shared/` (shared project `.shproj`)
- Load shared resources via helper methods in the shared test project
- Organize test data by feature or module: `EFCore/`, `Dapper/`, `Dynamic/`
- Synthetic test data (deterministic arrays, edge cases) should be generated inline

## Naming Conventions

- **Test classes:** `<SUT>Tests` (e.g., `EfContextBuilderTests`, `QueryBuilderTests`)
- **Test methods:** `<MethodName>_<scenario>_<expected>` (e.g., `Build_returns_context_when_valid`)
- **Private helpers:** `Get<TestName>()`, `Get<TestName>TestCase()`, etc.

## Running Tests

```bash
dotnet test Triton.slnx                          # Run all tests (Debug)
dotnet test Triton.slnx -c Release               # Run tests in Release
dotnet test Triton.slnx --filter "FullyQualifiedName~EfContext"  # Run a subset
```

## Best Practices

1. **No compiler warnings** — tests must compile cleanly
2. **Tests should be deterministic** — avoid reliance on system state, time, or randomness
3. **Use shared test code** — put common fixtures in `Triton.Tests.Shared`
4. **Keep tests small and focused** — one assertion per concept, or one round-trip per test
5. **Use `TestCaseSource` for data-driven tests** rather than looping inside `[Test]` methods
6. **Cover edge cases** — empty inputs, nulls (where applicable), boundary values
7. **Integration tests should reflect real usage** — services should be tested with real dependencies where appropriate

## Shared Test Code

- Shared test utilities, fixtures, and helpers go in `src/Tests/Triton.Tests.Shared/`
- Import shared code into test projects via `.projitems` file
- This avoids duplication across test projects and keeps common utilities maintainable
