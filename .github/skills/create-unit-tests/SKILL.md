---
name: create-unit-tests
description: 'Create or extend unit tests in Triton. Use when: writing new test files, adding test methods to existing tests, implementing round-trip tests, testing transports, adding Moq-based integration tests. Produces NUnit 4.x test classes matching project conventions.'
---

# Create Unit Tests

## Workflow

### 1. Locate the Source Under Test (SUT)

Identify the class, method, or module to test. Determine:
- **Which file** contains the SUT (check `src/Core/`, `src/Transport/`, or `src/Bundles/`)
- **What namespace** the SUT belongs to (usually `TheXDS.Triton.<Module>`)
- **What interfaces** it depends on (check if Moq mocks are needed)

### 2. Determine Test Location

Test files **mirror** the source structure under `src/Tests/`, with one test project per source project:

| Source | Test File |
|--------|-----------|
| `src/Transport/Triton.EFCore/EfContextBuilder.cs` | `src/Tests/Transport/Triton.Tests.EFCore/EfContextBuilderTests.cs` |
| `src/Bundles/Triton.Dynamic/QueryBuilder.cs` | `src/Tests/Bundles/Triton.Tests.Dynamic/QueryBuilderTests.cs` |
| `src/Core/Triton/Services/DbContextService.cs` | `src/Tests/Core/Triton.Tests/DbContextServiceTests.cs` |

Shared test code lives in `src/Tests/Triton.Tests.Shared/` and is imported via `.projitems` into test projects.

If a test file already exists in the correct location, **add to it** rather than creating a new file.

### 3. Create the Test Class

```csharp
namespace TheXDS.Triton.<Module>;

internal class <SUTName>Tests
{
    // Test methods go here
}
```

**Rules:**
- Namespace must match the source namespace
- Class is `internal` (exposed via `InternalsVisibleTo`)
- Class name: `<SUTName>Tests` (e.g., `EfContextBuilderTests`, `QueryBuilderTests`)

### 4. Write Test Methods

Follow the pattern: `<MethodName>_<scenario>_<expected>`

```csharp
[Test]
public void Build_returns_null_if_configuration_is_invalid()
{
    Assert.That(EfContextBuilder.Build(invalidConfig), Is.Null);
}
```

### 5. Choose the Right Test Pattern

**A. Pure function / unit test** — no dependencies:
```csharp
[Test]
public void MethodName_returns_expected_result_for_valid_input()
{
    var result = Sut.Method(input);
    Assert.That(result, Is.EqualTo(expected));
}
```

**B. Round-trip / integration test** — read + write:
```csharp
[TestCaseSource(nameof(GetTestCases))]
public void Roundtrip_context_builder(IConfiguration config)
{
    var obj = DbContextBuilder.Create(config);
    var output = DbContextBuilder.Validate(obj);
    Assert.That(output.IsValid, Is.True);
}

private static IEnumerable<IConfiguration> GetTestCases()
{
    yield return GetValidContextConfig();     // Shared fixture
    yield return GetDeterministicRndArray(65536);
    yield return [.. Enumerable.Range(0, 256).Select(i => (byte)i)];
}
```

**C. Mock-based test** — interface dependency:
```csharp
[Test]
public void Tool_calls_serializer_deserialize()
{
    var mock = new Mock<IDependency>();
    mock.Setup(d => d.Operation(It.IsAny<byte[]>()))
        .Returns(new Result());

    var sut = new ClassUnderTest(mock.Object);
    // Act & assert
}
```

### 6. Add Test Fixtures (if needed)

For shared test fixtures:
- Place shared test utilities in `src/Tests/Triton.Tests.Shared/`
- Import via `.projitems` into test projects that need them
- Organize by feature or module as needed

### 7. Validate

- **No compiler warnings** in the test file
- **Tests compile** — run `dotnet build`
- **Tests pass** — run `dotnet test`
- **Deterministic** — no reliance on time, randomness, or system state

## Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Test class | `<SUT>Tests` | `EfContextBuilderTests` |
| Test method | `<Method>_<scenario>_<expected>` | `Build_returns_null_if_config_invalid` |
| Helper method | `Get<Descriptor>()` | `GetValidContextConfig()`, `GetRoundTripTestCase()` |

## Common Pitfalls

1. **Wrong namespace** — must match the source namespace exactly
2. **Public test class** — must be `internal`
3. **Non-deterministic tests** — no `DateTime.Now`, `Random`, or file I/O without embedded resources
4. **Missing `TestCaseSource`** — use for data-driven tests, not loops inside `[Test]`
5. **Ignoring shared test project** — use `Triton.Tests.Shared` for common fixtures via `.projitems`
6. **Warnings** — zero compiler warnings, including in test files

## Running Tests

```bash
dotnet test Triton.slnx                          # All tests
dotnet test Triton.slnx --filter "FullyQualifiedName~EfContext"  # Subset
dotnet test Triton.slnx -c Release               # Release mode
```
