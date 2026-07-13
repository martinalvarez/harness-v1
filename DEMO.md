# DEMO — Harness Engineering con Spec-Driven Development

Guion para 15 minutos. La idea a transmitir: el harness (specs + gates) es
el control de calidad automático alrededor del agente. El agente escribe
código; el harness decide si ese código entra.

## 0. Setup (30s)

Mostrar el árbol de carpetas:

```
specs/     -> el QUÉ (constitution + feature spec + criterios chequeables)
gates/     -> el control automático (tests + reglas de arquitectura + estilo)
scripts/   -> el loop de feedback (verify.ps1 / verify.sh)
src/       -> vacío. Todavía no existe la app.
```

Punto clave: **todo en `/gates` ya compila y corre aunque `/src` esté
vacío.** El harness no depende de que la app exista para poder evaluarla.

## 1. Mostrar las specs (3 min)

- `specs/constitution.md`: reglas globales no negociables (capas, naming,
  errores estándar). Esto no es "documentación", es lo que los gates van
  a hacer cumplir.
- `specs/products-spec.md`: el feature en sí (entidad `Product`,
  endpoints, reglas de negocio, edge cases). Sin mencionar clases,
  carpetas ni EF Core: eso es implementación.
- `specs/verification.md`: cada requisito de arriba bajado a un
  criterio con un ID (`AC-01`, `ARCH-01`, ...). Cada ID tiene un test
  con el mismo nombre en `/gates`. Trazabilidad 1:1.

## 2. Correr el gate en rojo (2 min)

```powershell
./scripts/verify.ps1
```

Resultado esperado: el build de `/gates` es exitoso, pero los 18 tests
(13 funcionales + 5 de arquitectura) fallan:

- Los `AC-xx` fallan por **connection refused** (no hay API corriendo
  todavía).
- Los `ARCH-xx` fallan porque **no encuentran los assemblies** de
  `src/Api`, `src/Domain`, etc.

Mensaje para la audiencia: esto no es un error del harness, es el
harness funcionando — "no implementado" es un estado detectable, igual
que "arquitectura violada".

## 3. Pedirle al agente que implemente `/src` (5-7 min, en vivo)

Prompt al agente (por ejemplo, a Claude Code):

> "Implementá la API en `/src` siguiendo `specs/constitution.md` y
> `specs/products-spec.md`. No toques nada en `/gates`. Al terminar,
> corré `./scripts/verify.ps1` y arreglá lo que falle hasta que pase
> en verde."

Dejar que el agente:
1. Cree los proyectos `src/Api`, `src/Application`, `src/Domain`,
   `src/Infrastructure` respetando las capas.
2. Implemente el CRUD de `Product` con las reglas de negocio.
3. Corra `verify.ps1` él mismo y itere sobre los fallos — este es el
   loop de feedback en acción, sin supervisión manual de cada línea.

Cuando termine, correr de nuevo:

```powershell
./scripts/verify.ps1
```

Resultado esperado: **todo en verde** (exit code `0`).

## 4. Inyectar una violación de arquitectura y cazarla (2-3 min)

Editar a mano `src/Api/Controllers/ProductsController.cs` para violar la
constitution a propósito, por ejemplo agregando:

```csharp
private readonly AppDbContext _db; // referencia directa a EF Core
```

o agregando un `using Microsoft.EntityFrameworkCore;` en el controller.

Correr de nuevo:

```powershell
./scripts/verify.ps1
```

Resultado esperado: `dotnet build` puede pasar, pero
**`ARCH01_Api_DoesNotDependOn_EntityFrameworkCore` falla**, señalando
exactamente qué tipos violan la regla (`FailingTypeNames`). Exit code
distinto de cero.

Cierre: revertir el cambio, correr `verify.ps1` una vez más para volver
al verde, y remarcar el punto central de la demo — **el spec fue el
feedforward, el gate fue el feedback, y ninguno de los dos requirió que
alguien leyera el diff línea por línea.**
