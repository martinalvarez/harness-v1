# Cómo inyectar la violación de arquitectura (paso C de la demo)

## El mecanismo (por qué funciona)

NetArchTest no mira si el campo se *usa* — mira si el tipo del campo
obliga al compilador a generar una referencia de ensamblado. Con solo
declarar un campo de un tipo "prohibido" alcanza, aunque nunca se use
en el cuerpo de un método. Por eso la violación es tan mínima: una
línea.

## Opción A — EF Core / `DbContext`

Si la implementación en `/src` usa EF Core, `Api.csproj` puede no tener
el paquete instalado todavía. Para poder referenciar `DbContext` en el
controller hay que agregarlo primero:

```xml
<!-- en Api.csproj, dentro de un <ItemGroup> -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
```

Y en el controller:

```csharp
using Microsoft.EntityFrameworkCore;
...
private readonly DbContext _db = null!;
```

Esto dispara **`ARCH01`** (`Api no depende de Microsoft.EntityFrameworkCore`).

## Opción B — referenciar `Infrastructure` directo (sin EF Core)

Si la implementación no usa EF Core (por ejemplo, un repositorio en
memoria), conviene esta variante: en vez de traer una dependencia
nueva de afuera, el controller se salta la capa `Application` y agarra
el repositorio concreto directo. Hay que agregar la referencia de
proyecto (temporal) en `Api.csproj`:

```xml
<ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
```

Y en el controller:

```csharp
using Infrastructure; // o el namespace exacto donde vive el repositorio concreto
...
private readonly InMemoryProductRepository _repo = null!; // el nombre real puede variar
```

Esto dispara **`ARCH02`** (`Api no depende de Infrastructure`) — y de
paso es una historia clara para contar en vivo: "el controller se
salteó `Application` y agarró la implementación concreta directo", que
es exactamente lo que la constitution prohíbe.

## Cuál elegir

Si el `/src` implementado no tiene EF Core instalado (por ejemplo,
usa un repositorio en memoria), conviene la **Opción B**: cero
paquetes nuevos que instalar, menos pasos, y el nombre del test que
salta (`ARCH02`) cuenta una historia igual de clara que `ARCH01`.
Conviene anotar de antemano el nombre exacto de la clase del
repositorio concreto (mirar dentro de `src/Infrastructure/`) para no
perder tiempo buscándolo en vivo.

## Después de mostrarlo

Revertir las líneas agregadas (y la referencia agregada en el
`.csproj`, sea `PackageReference` o `ProjectReference`) y correr
`.\scripts\verify.ps1` una vez más para cerrar en verde.
