![Tritón logo](https://raw.githubusercontent.com/TheXDS/Triton/master/Art/Triton%20banner.svg)
[![CodeFactor](https://www.codefactor.io/repository/github/thexds/triton/badge)](https://www.codefactor.io/repository/github/thexds/triton)
[![codecov](https://codecov.io/gh/TheXDS/Triton/branch/master/graph/badge.svg?token=ULEQC09JGW)](https://codecov.io/gh/TheXDS/Triton)
[![Build Triton](https://github.com/TheXDS/Triton/actions/workflows/build.yml/badge.svg)](https://github.com/TheXDS/Triton/actions/workflows/build.yml)
[![Publish Triton](https://github.com/TheXDS/Triton/actions/workflows/publish.yml/badge.svg)](https://github.com/TheXDS/Triton/actions/workflows/publish.yml)
[![Issues](https://img.shields.io/github/issues/TheXDS/Triton)](https://github.com/TheXDS/Triton/issues)
[![GPL-v3.0](https://img.shields.io/github/license/TheXDS/Triton)](https://www.gnu.org/licenses/gpl-3.0.en.html)

## Introducción
Tritón es una librería auxiliar que facilita el acceso a la API de gestores
de bases de datos, particularmente Entity Framework Core Provee de servicios,
clases base, generadores dinámicos y otras herramientas misceláneas.

## Releases
Tritón se encuentra disponible en NuGet y en mi repositorio privado de GitHub.

Release | Link
--- | ---
Última versión estable: | [![Versión estable](https://buildstats.info/nuget/TheXDS.Triton)](https://www.nuget.org/packages/TheXDS.Triton/)
Última versión de desarrollo: | [![Versión de desarrollo](https://buildstats.info/nuget/TheXDS.Triton?includePreReleases=true)](https://www.nuget.org/packages/TheXDS.Triton/)

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

**Referencia de paquete**  
```xml
<PackageReference Include="TheXDS.Triton" Version="1.0.0" />
```

**Ventana interactiva (CSI)**  
```
#r "nuget: TheXDS.Triton, 1.0.0"
```

#### Repositorio de GitHub
Para obtener los paquetes de Tritón directamente desde GitHub, es necesario
agregar mi repositorio privado. Paar lograr esto, solo es necesario
ejecutar en una terminal:
```sh
nuget sources add -Name "TheXDS GitHub Repo" -Source https://nuget.pkg.github.com/TheXDS/index.json
```

## Compilación
Tritón requiere de un compilador compatible con C# 10, debido a ciertas
características especiales del lenguaje que ayudan a disminuir la
complejidad del código.

Tritón también requiere que [.Net SDK 6.0](https://dotnet.microsoft.com/) o
posterior esté instalado en el sistema.

### Compilando Tritón
```sh
dotnet build ./src/Triton.sln
```
Los binarios se encontarán en la carpeta `Build` en la raíz del repositorio.

### Ejecutando pruebas
```sh
dotnet test ./src/Triton.sln
```