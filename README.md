![Tritón logo](https://raw.githubusercontent.com/TheXDS/Triton/master/Art/Triton%20banner.svg)
[![CodeFactor](https://www.codefactor.io/repository/github/thexds/triton/badge)](https://www.codefactor.io/repository/github/thexds/triton)
[![codecov](https://codecov.io/gh/TheXDS/Triton/branch/master/graph/badge.svg?token=ULEQC09JGW)](https://codecov.io/gh/TheXDS/Triton)
[![Build Triton](https://github.com/TheXDS/Triton/actions/workflows/build.yml/badge.svg)](https://github.com/TheXDS/Triton/actions/workflows/build.yml)
[![Publish Triton](https://github.com/TheXDS/Triton/actions/workflows/publish.yml/badge.svg)](https://github.com/TheXDS/Triton/actions/workflows/publish.yml)
[![Issues](https://img.shields.io/github/issues/TheXDS/Triton)](https://github.com/TheXDS/Triton/issues)
[![MIT](https://img.shields.io/github/license/TheXDS/Triton)](https://mit-license.org)

## Introduction
Tritón is a utility library that facilitates access to Database Management System APIs,
particularly Entity Framework Core. It provides services,
base classes, dynamic generators, and other miscellaneous tools.

## Releases
Tritón is available on NuGet and my private GitHub repository.

Release | Link
--- | ---
Latest stable version: | [![Stable version](https://buildstats.info/nuget/TheXDS.Triton)](https://www.nuget.org/packages/TheXDS.Triton/)
Latest development version: | [![Development version](https://buildstats.info/nuget/TheXDS.Triton?includePreReleases=true)](https://www.nuget.org/packages/TheXDS.Triton/)

**Package Manager**  
```sh
Install-Package TheXDS.Triton
```

**.NET CLI**  
```sh
dotnet add package TheXDS.Triton
```

**Paket CLI**  
```sh
paket add TheXDS.Triton
```

**Package Reference**  
```xml
<PackageReference Include="TheXDS.Triton" Version="1.4.0" />
```

**Interactive Window (CSI)**  
```
#r "nuget: TheXDS.Triton, 1.4.0"
```

#### GitHub Repository
To obtain Tritón packages directly from GitHub, it is necessary
to add my private repository. To do this, simply run
in a terminal:
```sh
nuget sources add -Name "TheXDS GitHub Repo" -Source https://nuget.pkg.github.com/TheXDS/index.json
```

## Build
Tritón requires a C# 10 compatible compiler, due to certain
special language features that help reduce code
complexity.

Tritón also requires [.NET SDK 6.0](https://dotnet.microsoft.com/) to be installed on the system.

### Building Tritón
```sh
dotnet build ./src/Triton.sln
```
Binaries will be found in the `./Build` folder at the root of the repository.

### Running Tests
```sh
dotnet test ./src/Triton.sln
```
#### Coverage Report
It is possible to obtain a code coverage report locally. To do this, it is necessary to install
[`ReportGenerator`](https://github.com/danielpalme/ReportGenerator), which will read the test execution results and generate a web page with the coverage results.

To install `ReportGenerator` run:
```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```
After installing `ReportGenerator`, you can run the following command:
```sh
dotnet test .\src\Triton.sln --collect:"XPlat Code Coverage" --results-directory:.\Build\Tests ; reportgenerator.exe -reports:.\Build\Tests\*\coverage.cobertura.xml -targetdir:.\Build\Coverage\
```
Coverage results will be stored in `./Build/Coverage`

## Contribute
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W415UCHY)

If Tritón has been useful to you, or you are interested in donating to encourage the
development of the project, feel free to make a donation via
[PayPal](https://paypal.me/thexds), [Ko-fi](https://ko-fi.com/W7W415UCHY)
or get in touch with me directly.

Unfortunately, I cannot offer other donation methods at the moment
due to my country (Honduras) not being supported by most platforms.