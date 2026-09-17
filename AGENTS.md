# Tritón — AI Agent Guide

## Project Overview

**Tritón** is a .NET utility library that facilitates access to Database Management System APIs, particularly Entity Framework Core. It provides services, base classes, dynamic generators, and other miscellaneous tools for rapid software development with data connections.

- **Language**: C# 10+ (nullable references enabled, implicit usings enabled)
- **Target Framework**: `.net8.0` (open to targeting newer .NET versions in the future)
- **Test Framework**: NUnit 4 + Moq + Microsoft.NET.Test.Sdk
- **Package Manager**: NuGet
- **Build System**: MSBuild via `dotnet` CLI (no custom scripts)
- **Solution File**: `Triton.slnx` (new SLNX format, single solution)

---

## Repository Structure

```
Triton/
├── Triton.slnx                          # Single solution (SLNX format)
├── src/
│   ├── Directory.Build.props            # Global MSBuild props (imports BuildPaths, PackageVersion, PackageInfo)
│   ├── Directory.Build.targets           # Global MSBuild targets (imports CompileOptions, GlobalDirectives)
│   │
│   ├── Core/                             # Core libraries — NO external dependencies
│   │   ├── Triton/                       # Main Triton library
│   │   └── Triton.Models/               # Shared data models
│   │
│   ├── Transport/                        # Database transport technologies (one project per engine)
│   │   ├── Triton.Dapper/               # Dapper integration
│   │   ├── Triton.EFCore/               # Entity Framework Core integration
│   │   ├── Triton.InMemory/             # In-memory data store
│   │   └── Triton.JsonLocalStore/       # JSON-based local storage
│   │
│   ├── Bundles/                          # Extensions / auxiliary libs (may depend on 3rd-party packages)
│   │   ├── ServicePool.Triton/           # Service pool integration
│   │   ├── ServicePool.Triton.Ef/        # Service pool + EF
│   │   ├── ServicePool.Triton.EfContextBuilder/
│   │   ├── Triton.CrudNotify/            # CRUD notification support
│   │   ├── Triton.Diagnostics/           # Diagnostics / MVVM helpers
│   │   ├── Triton.Dynamic/               # Dynamic code generators
│   │   ├── Triton.EfContextBuilder/      # EF context builder utilities
│   │   ├── Triton.Faker/                 # Data generation / faker utilities
│   │   ├── Triton.SecurityEssentials/    # Security utilities
│   │   └── Triton.SecurityEssentials.Ef/ # Security + EF
│   │
│   └── Tests/
│       ├── Triton.Tests.Shared/          # Shared test project (.shproj) — imported via .projitems
│       ├── Core/
│       │   ├── Triton.Tests/             # Tests for main Triton library
│       │   └── Triton.Tests.Models/      # Tests for data models
│       ├── Bundles/
│       │   └── (one test project per bundle)
│       └── Transport/
│           ├── Triton.Tests.EFCore/
│           └── Triton.Tests.InMemory/
```

### Folder Conventions

| Folder | Purpose | Rules |
|--------|---------|-------|
| `Core/` | Base library with **no external dependencies** (beyond internal project refs and TheXDS.MCART) | Pure Triton functionality |
| `Transport/` | One project per database connection technology. If a new DB engine/connection tech is supported, its project lives here. | Implements transport-layer abstractions |
| `Bundles/` | Extensions or auxiliary libraries. May depend on 3rd-party NuGet packages. | Depends on Core or Transport projects |
| `Tests/` | Mirror structure of `src/`. One test project per source project. Shared test code via `.projitems` import from `Triton.Tests.Shared`. | NUnit test projects, non-packable |

---

## Build & Run Commands

### Build the Entire Solution

```bash
dotnet build Triton.slnx
```

### Build with Release Configuration

```bash
dotnet build Triton.slnx --configuration Release
```

### Run All Tests

```bash
dotnet test Triton.slnx
```

### Run Tests with Code Coverage (OpenCover format for Codecov)

```bash
dotnet test Triton.slnx --no-build --collect:"XPlat Code Coverage" \
  --results-directory ./Build/tests \
  -p:DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover \
  -- -maxcpucount:1
```

### Generate Local Coverage HTML Report (requires ReportGenerator)

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./Build/tests/*/coverage.opencover.xml -targetdir:./Build/Coverage/
```

> **Note**: Use `reportgenerator` (not `reportgenerator.exe`) for cross-platform compatibility. On Linux/macOS, invoking `reportgenerator.exe` directly will fail — always call it via the global tool name.

### Create NuGet Packages (from `dotnet pack`)

```bash
dotnet pack Triton.slnx --configuration Debug \
  --version-suffix $(git rev-parse --short HEAD) \
  --include-source \
  -p:RepositoryBranch=refs/heads/master \
  -p:RepositoryCommit=$(git rev-parse HEAD) \
  -p:ContinuousIntegrationBuild=true
```

Output: `.nupkg` files are placed in `Build/bin/<ProjectName>/`

---

## MSBuild & Project Configuration

All projects inherit from shared MSBuild files in `src/`:

| File | Purpose |
|------|---------|
| `Directory.Build.props` | Sets `Root`, imports `BuildPaths.props`, `PackageVersion.props`, `PackageInfo.props` |
| `Directory.Build.targets` | Imports `CompileOptions.targets`, `GlobalDirectives.targets` |
| `BuildTargets/BuildPaths.props` | Redirects output to `Build/bin/<ProjectName>` and `Build/obj/<ProjectName>` |
| `BuildTargets/PackageVersion.props` | Sets `VersionPrefix` to `2.0.0` |
| `BuildTargets/PackageInfo.props` | Package metadata: author, company, license (MIT), icon, tags, description |
| `BuildTargets/CompileOptions.targets` | Nullable, deterministic, XML doc file, implicit usings, config-specific defines |
| `BuildTargets/GlobalDirectives.targets` | Enables `ImplicitUsings` and `ExtraDefineConstants` |

### Key Defaults

- **Nullable references**: `enable`
- **Deterministic builds**: `true`
- **XML documentation**: generated to `$(OutDir)$(AssemblyName).xml`
- **Implicit usings**: `enable`
- **NuGet package ID pattern**: `TheXDS.<AssemblyName>` (e.g., `TheXDS.Triton`)
- **Source Link**: `Microsoft.SourceLink.GitHub` included in all projects
- **AOT compatibility**: Main Triton project sets `IsAotCompatible=true`

---

## NuGet Package Publishing

### Publishing Workflow (GitHub Actions)

The `publish.yml` workflow triggers on **GitHub Release created**:

1. Builds packages in **Release** configuration
2. Pushes to **GitHub Packages** (`https://nuget.pkg.github.com/TheXDS/`)
3. Pushes to **NuGet.org** (`https://api.nuget.org/v3/index.json`)
4. Uses `--skip-duplicate` to avoid errors on re-runs

### Secrets Required

| Secret | Purpose |
|--------|---------|
| `GITHUB_TOKEN` | Pushes to GitHub Packages (auto-generated by GitHub Actions) |
| `NUGET_TOKEN` | API key for pushing to NuGet.org |

### To Publish a New Version

1. Bump `VersionPrefix` in `BuildTargets/PackageVersion.props` (currently `2.0.0`)
2. Create and push a **GitHub Release** (tagged version)
3. The `publish.yml` workflow triggers automatically

### Pre-release Packages

When `VersionSuffix` is set (e.g., during CI builds), packages are tagged as pre-release and include a `PackageReleaseNotes` warning about production use.

### Local GitHub NuGet Source

For local development, add the private GitHub NuGet source:

```bash
nuget sources add -Name "TheXDS GitHub Repo" \
  -Source https://nuget.pkg.github.com/TheXDS/index.json
```

---

## Continuous Integration (GitHub Actions)

### `build.yml` — Build & Test

- **Triggers**: Push to non-doc branches (ignores `**/*.md`, `docs*` branches, tags)
- **OS**: `windows-latest`
- **Steps**:
  1. Checkout
  2. Install .NET 6.0.x and 8.0.x SDKs
  3. Install `codecov` CLI via Chocolatey
  4. Build solution
  5. Run tests with OpenCover coverage collection
  6. Upload coverage to Codecov (uses `CODECOV_TOKEN` secret)
  7. Pack NuGet packages (with git SHA version suffix, source link)
  8. Upload packages as artifact `Triton-nuget-packages`

### `publish.yml` — Release & NuGet Publishing

- **Triggers**: GitHub `release` `created` event
- **OS**: `windows-latest`
- **Steps**:
  1. Checkout
  2. Install .NET 6.0.x and 8.0.x SDKs
  3. Pack NuGet packages (version from release tag)
  4. Push to GitHub Packages
  5. Push to NuGet.org

### `docfx.yml` — Documentation Deployment

- **Triggers**: Push to `master`, or manual `workflow_dispatch`
- **OS**: `ubuntu-latest`
- **Steps**:
  1. Checkout
  2. Setup GitHub Pages
  3. Run DocFX (via `clFaster/docfx-build-action`)
  4. Deploy to GitHub Pages as static site

---

## Documentation (DocFX)

- **Config**: `docs/docfx.json`
- **TOC**: `docs/toc.yml`
- **Index**: `docs/index.md`
- **Articles**: `docs/articles/`
- **API Reference**: `docs/api/`

DocFX is run in CI (`docfx.yml`) and deployed to GitHub Pages. The `docs/` directory is excluded from build triggers to prevent unnecessary CI runs.

---

## Coding Conventions & Patterns

### Naming

- **Root namespace**: `TheXDS.<AssemblyName>` (e.g., `TheXDS.Triton`, `TheXDS.Triton.EFCore`)
- **Resource strings**: Localized `.resx` files under `Resources/Strings/` with designer classes
- **Test projects**: Prefix with `Triton.Tests.` (e.g., `Triton.Tests.EFCore`)

### Dependencies

- **Core projects**: Only reference other Core/Transport projects. No 3rd-party NuGet packages except `Microsoft.SourceLink.GitHub`.
- **Bundle projects**: May reference 3rd-party packages (e.g., `thexds.mcart.mvvm`, `TheXDS.MCART`).
- **Transport projects**: May reference 3rd-party DB packages (e.g., `Microsoft.EntityFrameworkCore`, Dapper).

### Code Structure Rules

- **Max 3 levels of indentation.** If deeper — rethink the logic (segregate into small, manageable methods).
- Keep functions small, clear, and testable.
- User-facing strings go in `.resx` files — **no magic strings** (SQL queries and well-known constants are fine).
- In-code comments should be minimal — code should be self-explanatory. Add comments only to explain *why* something looks odd or overly complex.

### Test Projects

- All test projects target `net8.0` and set `IsPackable=false`.
- Shared test code is in `Triton.Tests.Shared` (shared project `.shproj`) and imported via `.projitems`.
- Test dependencies: `NUnit`, `NUnit3TestAdapter`, `Moq`, `coverlet.collector`, `Microsoft.NET.Test.Sdk`.
- **Test pairing**: Every new public API must have a corresponding unit test in the matching `*.Tests` project.

### Embedded Resources

Resource strings are managed via `.resx` files with auto-generated designer classes:

```xml
<Compile Update="Resources\Strings.Designer.cs">
  <DesignTime>True</DesignTime>
  <AutoGen>True</AutoGen>
  <DependentUpon>Strings.resx</DependentUpon>
</Compile>
<EmbeddedResource Update="Resources\Strings.resx">
  <Generator>ResXFileCodeGenerator</Generator>
  <LastGenOutput>Strings.Designer.cs</LastGenOutput>
</EmbeddedResource>
```

### Versioning

- Base version: `2.0.0` (in `BuildTargets/PackageVersion.props`)
- CI builds append git SHA as version suffix
- Release builds use the GitHub release tag as the version

---

## Architecture Notes

### SOLID Philosophy

Use SOLID **responsibly**, not religiously. Key points:

- **No excessive interface segregation** for single implementations. If there's only one correct way to implement a service or generator, a concrete class is fine — no need for separate read/write interfaces just to satisfy ISP.
- **No unnecessary DI** for simple, testable classes. A well-coupled class that can be integration-tested is often better than layers of abstractions.
- SRP/ISP are good — but when types are closely related, group them together.

### Boy-Scout Rule

Small improvements to clarity are welcome, but **do not rewrite the entire codebase**. Incremental, focused changes are the way to go.

---

## Testing Patterns

### Test Framework & Structure

- **Framework**: NUnit 4 (`[Test]`, `[TestCaseSource]`, `[SetUp]`, etc.)
- **Assertions**: `Assert.That(...)`, `Throws.InstanceOf<T>()`
- **Mocks**: Moq for interface-based dependencies
- **Test fixture data**: Shared test code in `Triton.Tests.Shared` (shared project `.shproj`) imported via `.projitems`

### Example Test Patterns

**Unit test for a pure function:**

```csharp
[Test]
public void Method_returns_expected_result_when_input_is_valid()
{
    Assert.That(ClassUnderTest.Method(input), Is.EqualTo(expected));
}
```

**Parameterized test with TestCaseSource:**

```csharp
[TestCaseSource(nameof(GetTestCases))]
public void Roundtrip_test(object input)
{
    var result = ClassUnderTest.Process(input);
    Assert.That(result, Is.EqualTo(expected));
}
```

**Testing with Moq:**

```csharp
[Test]
public void Method_calls_dependency_with_correct_parameters()
{
    var mockService = new Mock<IService>();
    mockService.Setup(s => s.DoWork(It.IsAny<string>())).Returns(expected);

    var subject = new MyClass(mockService.Object);
    // ... act and assert
}
```

---

## Important Notes for Agents

1. **Always build from the solution root** using `dotnet build Triton.slnx`. Do not build individual projects unless there's a specific reason.
2. **When adding a new database transport**, create a new project under `src/Transport/` following the pattern of existing transport projects (EFCore, Dapper, InMemory).
3. **When adding a new bundle**, create a project under `src/Bundles/` that depends on Core/Transport projects. Bundles may use 3rd-party packages.
4. **For every source project**, create a corresponding test project under `src/Tests/` mirroring the source folder structure.
5. **Shared test code** goes in `src/Tests/Triton.Tests.Shared/` and is imported via `.projitems` into test projects.
6. **All projects output to `Build/bin/` and `Build/obj/`** — never assume default output paths.
7. **When modifying CI workflows**, be aware that `build.yml` and `publish.yml` run on `windows-latest`, while `docfx.yml` runs on `ubuntu-latest`.
8. **Nullable references are enabled globally** — all new code must comply with nullable annotations.

---

## What NOT to do

- **Do not commit without running `dotnet test Triton.slnx --no-build` and confirming it passes.**
- **Keep changes minimal and focused** — one concern per commit or PR (single library, single fix, or single feature).
- **Do not add 3rd-party NuGet packages** to Core projects — they must have no external dependencies beyond internal project references and `Microsoft.SourceLink.GitHub`.
- **Do not install additional 3rd-party dependencies** on any project without an explicit reason to do so.
- **Do not modify MSBuild files in `src/`** (`Directory.Build.props`, `Directory.Build.targets`) or `BuildTargets/` unless the task explicitly targets build infrastructure.
- **Do not hardcode version numbers** in individual projects — versioning is centralized in `BuildTargets/PackageVersion.props`.

---

## Git & Contributing

- **Commit messages:** Use [Conventional Commits](https://www.conventionalcommits.org/) (e.g., `feat:`, `fix:`, `docs:`, `test:`).
- **Branch naming:** `feature/42-add-bnk-write-support`, `fix/15-refpack-decode-error`

---

## Important Details

1. **Shared test code:** `Triton.Tests.Shared` is a shared project (`.shproj`) imported via `.projitems` into test projects.
2. **Build props hierarchy:** `src/Directory.Build.props` → `BuildTargets/*.props` → per-project `*.csproj`
3. **NuGet package ID pattern:** `TheXDS.<AssemblyName>` (e.g., `TheXDS.Triton`, `TheXDS.Triton.EFCore`)
4. **Source Link:** `Microsoft.SourceLink.GitHub` is included in all projects
5. **Output paths:** All projects output to `Build/bin/<ProjectName>/` and `Build/obj/<ProjectName>/` (controlled by `BuildTargets/BuildPaths.props`)
