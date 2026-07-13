# Chat — Armado del harness de Spec-Driven Development (Products CRUD)

Transcript de la sesión: pedido original, qué se hizo, y resultado final.

---

## 1. Pedido del usuario

> Contexto: Estoy armando una demo de "harness engineering" sobre Spec-Driven
> Development (SDD). El harness es la capa de control alrededor de un agente de
> coding: specs (feedforward) + gates automáticos (feedback). Vos vas a construir
> el ANDAMIAJE del harness, NO el código de la aplicación.
>
> Objetivo: Un CRUD service de "Products" (REST API en ASP.NET Core, .NET 8) usado
> como caso de prueba del harness.
>
> Generó esta estructura:
>
> ```
> /specs
>   constitution.md      -> principios y guardrails globales: convenciones de
>                           naming, arquitectura en capas obligatoria
>                           (Api -> Application -> Domain, Infrastructure sólo
>                           referenciada por Application), prohibiciones
>                           (ej: los controllers NO acceden al DbContext directo),
>                           manejo de errores estándar.
>   products-spec.md     -> la feature spec: el QUÉ, no el CÓMO. Entidad Product
>                           (Id, Name, Price, Stock), endpoints CRUD (GET all,
>                           GET by id, POST, PUT, DELETE), reglas de negocio
>                           (Price > 0, Name requerido y único, Stock >= 0),
>                           y edge cases (id inexistente -> 404, validación
>                           fallida -> 400 con detalle).
>   verification.md      -> criterios de aceptación en forma chequeable, mapeados
>                           1:1 a los tests del gate.
>
> /gates  (ESTO SÍ lo escribís vos, es el corazón del harness)
>   - Proyecto de tests xUnit con casos derivados de verification.md
>     (uno por criterio de aceptación). Los tests apuntan a la API que el
>     agente va a generar después en /src.
>   - Reglas de arquitectura con NetArchTest que hagan cumplir la constitution:
>     Controllers no dependen de EF Core / DbContext; Domain no depende de
>     Infrastructure; nombres de controllers terminan en "Controller".
>   - .editorconfig con analyzers de Roslyn en modo warning-as-error para las
>     reglas clave.
>
> /scripts
>   verify.sh (o verify.ps1) -> corre `dotnet build` + `dotnet test` y devuelve
>                               exit code distinto de cero si algún gate falla.
>                               Este es el loop de feedback.
>
> /src  -> DEJALO VACÍO con un README.md que diga: "El código de la app se genera
>          en vivo a partir de /specs. No implementar todavía."
>
> Reglas:
> - NO implementes la API en /src. Eso es la demo en vivo.
> - Los gates tienen que poder compilar y correr aunque /src esté vacío (que
>   fallen esperablemente por falta de implementación está bien: mostralo).
> - Mantené todo mínimo y legible: es para explicar en 15 minutos.
> - Al final, escribí un DEMO.md con el guion: (1) mostrar specs, (2) pedirte a
>   vos implementar /src desde la spec, (3) correr verify, (4) inyectar una
>   violación de arquitectura y mostrar el gate cazándola.

---

## 2. Qué hice

### Chequeo inicial
- Directorio `c:\martin\github\praveen` estaba vacío. `.NET 8.0.404` disponible.
- Creé la estructura de carpetas: `specs/`, `gates/Gates.Api.Tests/`,
  `gates/Gates.Architecture.Tests/`, `scripts/`, `src/`.

### `/specs`
- **`constitution.md`**: capas obligatorias (`Api -> Application -> Domain`,
  `Infrastructure` solo referenciada por `Application`), prohibiciones
  (controllers sin `DbContext`/EF Core directo, sin lógica de negocio en
  controllers, `Domain` sin dependencias de framework), convenciones de
  naming (`PascalCase`, `_camelCase` para campos privados, prefijo `I` en
  interfaces, controllers terminan en `Controller`, métodos async terminan
  en `Async`), manejo de errores estándar (`ProblemDetails`, 400/404/500).
- **`products-spec.md`**: entidad `Product` (`Id` GUID, `Name` único
  requerido, `Price` > 0, `Stock` >= 0), los 5 endpoints REST con sus
  status codes, reglas de negocio, edge cases (404 en id inexistente, 400
  con detalle en validación fallida, duplicado de `Name` case-insensitive).
- **`verification.md`**: 13 criterios funcionales (`AC-01`..`AC-13`), 5
  criterios de arquitectura (`ARCH-01`..`ARCH-05`) y 4 de estilo
  (`STYLE-01`..`STYLE-04`), cada uno mapeado 1:1 a un test o regla de
  `.editorconfig` en `/gates`.

### `/gates` (el corazón del harness)
- **`Gates.Api.Tests`** (xUnit): tests HTTP *black-box* contra
  `http://localhost:5087` (configurable vía `GATE_API_BASE_URL`), **sin
  ninguna referencia de compilación a `/src`** — así el proyecto compila
  aunque `/src` esté vacío. 13 tests, uno por cada `AC-xx`, usando nombres
  únicos (`Guid`) por test para evitar colisiones con la regla de `Name`
  único.
- **`Gates.Architecture.Tests`** (xUnit + `NetArchTest.Rules`): carga por
  reflection los `.dll` compilados desde `src/{Proyecto}/bin/**` (no usa
  `ProjectReference`). Si el assembly no existe, el test falla con
  `Assert.Fail` y un mensaje explícito ("Implement /src per specs..."),
  en vez de romper la compilación. 5 reglas: `Api` no depende de EF Core,
  `Api` no depende de `Infrastructure`, `Domain` no depende de
  `Infrastructure`, `Domain` no depende de frameworks, controllers
  terminan en `Controller`.
- **`.editorconfig`**: severidad `error` para nullable warnings
  (`CS8600`-`CS8625`) y `dotnet_naming_rule` para `PascalCase` en
  tipos/miembros, prefijo `I` en interfaces y `_camelCase` en campos
  privados.
- **`Gates.sln`** generada con `dotnet new sln` + `dotnet sln add`.

### `/scripts`
- **`verify.ps1`** y **`verify.sh`**: `dotnet build` sobre `Gates.sln` →
  si existe `src/Api/Api.csproj`, levanta la API en background
  (`dotnet run --urls http://localhost:5087`) → `dotnet test` → mata el
  proceso de la API → exit code = el de `dotnet test`.

### `/src`
- Solo `README.md`: *"El código de la app se genera en vivo a partir de
  `/specs`. No implementar todavía."* + la estructura de carpetas
  esperada (`Api`, `Application`, `Domain`, `Infrastructure`).

### `DEMO.md`
Guion de 15 minutos en 4 pasos: (0) mostrar el árbol, (1) mostrar specs,
(2) correr `verify.ps1` en rojo con `/src` vacío, (3) pedirle al agente
que implemente `/src` y vuelva a correr `verify.ps1` hasta que dé verde,
(4) inyectar a mano una referencia a `DbContext` en un controller y
mostrar cómo `ARCH01` la caza con el nombre del tipo ofensor.

---

## 3. Validación (corrida real, no solo escrita)

- `dotnet build gates/Gates.sln` → **compila limpio** con `/src` vacío
  (0 errores).
- `dotnet test gates/Gates.sln` → **18 tests, 18 fallan**, todos de forma
  prolija y esperada:
  - Los 13 `AC-xx` fallan por `HttpRequestException` (conexión rechazada
    en `localhost:5087` — no hay API corriendo).
  - Los 5 `ARCH-xx` fallan con el mensaje explícito de `AssemblyLoader`
    (`"'Api' project not found at ... Implement /src per specs before
    this gate can pass."`).
- Bug encontrado y corregido: faltaba `using Xunit;` en `ApiFixture.cs`
  (rompía el build) — arreglado.
- Bug encontrado y corregido: `verify.ps1` tenía un em-dash (`—`) en un
  string, y PowerShell 5.1 sin BOM lo interpreta mal y rompe el parser
  (`Falta la cadena en el terminador`). Reemplazado por un guion simple
  (`-`). Confirmado con una corrida completa de `verify.ps1`: exit code
  `1` (rojo esperado, tal cual documenta `DEMO.md`).

## 4. Estructura final

```
DEMO.md
gates/
  .editorconfig
  Gates.sln
  Gates.Api.Tests/
    ApiFixture.cs
    Gates.Api.Tests.csproj
    ProductsApiGateTests.cs
  Gates.Architecture.Tests/
    ArchitectureGateTests.cs
    AssemblyLoader.cs
    Gates.Architecture.Tests.csproj
scripts/
  verify.ps1
  verify.sh
specs/
  constitution.md
  products-spec.md
  verification.md
src/
  README.md
```
