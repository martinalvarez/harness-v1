# Playbook — Demo de Harness Engineering (mañana, mediodía)

Este archivo es tu material de apoyo para la presentación. Tiene tres partes:
(1) el concepto explicado para transmitir en tus palabras, (2) qué se
validó anoche (ensayo completo, no solo escrito) y (3) el runbook exacto
para correr la demo en vivo, con plan B si algo falla.

---

## 1. El concepto, para explicar con tus palabras

### La idea central en una frase

> El harness es el sistema de control alrededor de un agente de coding:
> **specs** que guían antes de que el agente escriba código (feedforward) +
> **gates automáticos** que verifican después (feedback). No revisás cada
> línea que escribe el agente — diseñás las condiciones para que lo que
> produzca sea correcto por construcción.

### Los tres conceptos, en cadena

1. **SDD (Spec-Driven Development)** — la especificación pasa a ser el
   artefacto de ingeniería principal; el código es su expresión. Antes
   escribías código que implicaba un diseño; ahora escribís el diseño
   (la spec) y el agente produce el código. Distinción clave: *planning
   session* (humano + agente escriben la spec, no hay código) vs.
   *execution session* (el agente ejecuta la spec aprobada).

2. **Harness engineering** — la práctica de diseñar esas guías
   (feedforward) y esos sensores (feedback) para que el código quede
   regulado hacia lo que la spec define. Tres capas: feedforward (specs,
   guardrails) → runtime (orquestación del agente) → feedback (tests,
   linters, chequeos de arquitectura). **SDD es lo que hace practicable
   al harness**: sin specs, el harness no tiene nada concreto que hacer
   cumplir.

3. **8090** — la versión productizada de esto: plataforma "spec to
   deploy" para industrias reguladas, con el pitch de que el líder de
   negocio define en lenguaje natural qué se construye, para que no sea
   ni el agente ni un dev junior quien tome decisiones de arquitectura.

### Cómo se reparte en tu demo concreta

| Carpeta | Qué es | Quién lo escribió |
|---|---|---|
| `specs/` | El QUÉ — constitution + feature spec + criterios chequeables | Vos (con el agente, en planning session) |
| `gates/` | El control automático — tests + reglas de arquitectura (NetArchTest) + estilo | El "corazón del harness" |
| `scripts/` | El loop de feedback (`verify.ps1`) | Orquestación mínima |
| `src/` | El código de la app | **El agente, en vivo, mañana** |

Punto que más impacta en la audiencia: **`/gates` compila y corre aunque
`/src` esté vacío.** El harness no depende de que la app exista para
poder evaluarla — "no implementado" es un estado tan detectable como
"arquitectura violada".

---

## 2. Qué se validó anoche (ensayo real, no solo el guion escrito)

Hasta anoche, el guion de `DEMO.md` estaba escrito pero **nunca se había
corrido**. Hice un ensayo completo end-to-end para no descubrir sorpresas
en vivo. Resultado:

- ✅ `/src` vacío → `verify.ps1` compila limpio y los 18 gates fallan en
  rojo como se espera (13 `AC-xx` por conexión rechazada, 5 `ARCH-xx` por
  assembly faltante).
- ✅ Implementé `/src` completo (Domain/Application/Infrastructure/Api)
  siguiendo `specs/` al pie de la letra → `verify.ps1` da **18/18 verde,
  exit code 0**.
- ✅ Inyecté la violación de arquitectura sugerida en `DEMO.md`
  (`DbContext` referenciado directo en `ProductsController`) →
  **`ARCH01` falla y señala exactamente `Api.Controllers.ProductsController`**
  como tipo ofensor. El resto (17/18) sigue verde.
- ✅ Revertí la violación → 18/18 verde de nuevo.

En el camino aparecieron **2 bugs reales** (igual que ya había pasado el
7/jul con el `using` faltante y el em-dash de PowerShell — este harness
tiene un patrón de "funciona en la teoría, falla en la práctica" que vale
la pena mencionar en la demo como ejemplo de por qué el feedback loop
importa):

1. **.NET 8 recorta el sufijo `Async` de los nombres de acción por
   default** (`SuppressAsyncSuffixInActionNames`). Esto rompía
   `CreatedAtAction(nameof(GetByIdAsync), ...)` con `500 — No route
   matches the supplied values`, porque la ruta registrada quedaba como
   `GetById`, no `GetByIdAsync`. Fix en `Program.cs`.
2. **Bug real en `gates/Gates.Architecture.Tests/AssemblyLoader.cs`**:
   buscaba el `.dll` del proyecto en *todo* `src/{Proyecto}` (incluyendo
   `obj/**/ref/`, que tiene assemblies de referencia metadata-only que no
   se pueden cargar para ejecución). Si el ref assembly tenía timestamp
   más reciente que el de `bin/`, `Assembly.LoadFrom` explotaba con
   `BadImageFormatException` — un falso rojo que no tenía nada que ver
   con arquitectura real. Ya corregido (ahora busca solo en `bin/`).

Todo esto está commiteado. La implementación completa que pasó el ensayo
quedó guardada en la rama `reference-solution` como red de seguridad
(ver sección 4).

---

## 3. Runbook para mañana (comandos exactos)

### Paso 0 — Setup (30s)

Mostrar el árbol: `specs/` (el qué) → `gates/` (el control) →
`scripts/` (el loop) → `src/` (vacío, solo `README.md`).

### Paso 1 — Mostrar las specs (3 min)

Abrir en este orden: `specs/constitution.md` → `specs/products-spec.md`
→ `specs/verification.md`. Remarcar la trazabilidad 1:1: cada `AC-xx` /
`ARCH-xx` tiene un test con el mismo nombre en `/gates`.

### Paso 2 — Correr el gate en rojo (2 min)

```powershell
./scripts/verify.ps1
```

Esperado: build de `/gates` OK, pero exit code `1` — 18 tests fallan
(13 por connection refused, 5 porque no encuentran los assemblies).

### Paso 3 — Pedirle al agente que implemente `/src` (5-7 min, en vivo)

Prompt sugerido:

> "Implementá la API en `/src` siguiendo `specs/constitution.md` y
> `specs/products-spec.md`. No toques nada en `/gates`. Al terminar,
> corré `./scripts/verify.ps1` y arreglá lo que falle hasta que pase en
> verde."

Cuando termine, correr de nuevo `./scripts/verify.ps1` → esperado: exit
code `0`, "All gates green."

**Nota de timing:** anoche la implementación completa (4 proyectos, 18
tests) más 2 rondas de debugging tomó bastante más de 5-7 min de trabajo
real. En vivo, con el agente ya "calentado" y sin tener que descubrir el
diseño de capas desde cero, debería ir más rápido — pero si a los 8-10
min no cerró, tenés el plan B de la sección 4.

### Paso 4 — Inyectar la violación y cazarla (2-3 min)

Editar a mano `src/Api/Controllers/ProductsController.cs`, agregando:

```csharp
using Microsoft.EntityFrameworkCore;
...
private readonly DbContext _db = null!;
```

(Y agregar `<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />`
al `Api.csproj` para que compile — si no, ni siquiera llega a correr el
gate, se cae en el build.)

Correr de nuevo `./scripts/verify.ps1` → esperado: `ARCH01` falla,
mensaje `Failing types: Api.Controllers.ProductsController`, el resto
sigue verde. Exit code `1`.

Cerrar revirtiendo el cambio (o restaurando desde `reference-solution`,
ver abajo) y corriendo `verify.ps1` una vez más para terminar en verde.

**Mensaje de cierre:** *el spec fue el feedforward, el gate fue el
feedback, y ninguno de los dos requirió que alguien leyera el diff línea
por línea.*

---

## 4. Plan de contingencia (si algo sale mal en vivo)

### Si el agente no llega a verde a tiempo en el Paso 3

Restaurar la implementación de referencia validada anoche (guardada en
la rama `reference-solution`) sin tocar el resto del repo:

```powershell
git checkout reference-solution -- src
./scripts/verify.ps1
```

Esto trae exactamente el `/src` que pasó el ensayo. Podés presentarlo
como "esto es lo que el agente generó" sin que se note.

### Volver a dejar `/src` vacío para reintentar en vivo

```powershell
git checkout HEAD -- src
git clean -fd src
```

(`HEAD` en `master` tiene `/src` con solo `README.md`.)

### Gotcha conocido: proceso `Api.exe` que queda vivo entre corridas

`verify.ps1` levanta la API con `Start-Process dotnet run ...` y al
final mata el proceso `dotnet`. Pero `dotnet run` lanza un proceso hijo
(`Api.exe`) que a veces **no muere** con eso, y se queda ocupando el
puerto 5087. Si la próxima corrida de `verify.ps1` falla toda con
`address already in use` o timeouts raros:

```powershell
Get-Process -Name "Api" -ErrorAction SilentlyContinue | Stop-Process -Force
```

y volver a correr `verify.ps1`.

### Si algo se rompe y no sabés qué cambió

Todo está en git. `git status` y `git diff` para ver qué se tocó,
`git checkout -- <archivo>` para descartar un cambio puntual, o en el
peor caso `git reset --hard HEAD` para volver limpio al último commit
(perdés cambios no commiteados — usar solo si estás seguro).

---

## 5. Preguntas esperadas de la audiencia

- **"¿Y si el agente hace trampa y edita los gates para que pasen?"** —
  Buena pregunta para 8090 real: ahí entra auditabilidad/control de
  acceso (el agente no debería tener permiso de tocar `/gates`). En esta
  demo mínima, el guardrail es el prompt ("no toques nada en /gates") +
  que vos revisás el diff antes de aceptar. Es un punto real de la
  demo, no una limitación oculta.
- **"¿Esto reemplaza a los tests que ya tenían?"** — No, es la misma
  disciplina de testing de siempre, aplicada como *gate* del agente en
  vez de (o además de) gate de CI para humanos. La diferencia es que
  acá el "developer" que tiene que pasar el gate es el agente.
- **"¿Por qué NetArchTest y no solo un linter?"** — Un linter (Roslyn
  analyzers/`.editorconfig`) chequea estilo y algunas reglas locales;
  NetArchTest chequea **dependencias entre ensamblados compilados**
  (IL), que es lo que hace cumplir la arquitectura en capas — algo que
  un linter de sintaxis no puede ver.
- **"¿Esto es specific de .NET?"** — El patrón (specs + gates + loop de
  feedback) es agnóstico de stack. NetArchTest y `.editorconfig` son la
  implementación en .NET; en otros stacks el equivalente sería
  ArchUnit (Java), import-linter (Python), dependency-cruiser (JS/TS).
