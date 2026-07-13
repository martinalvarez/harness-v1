# Verification

Criterios de aceptación chequeables. Cada `AC-xx` y `ARCH-xx` tiene un
test 1:1 en `/gates` con el mismo id en el nombre del método, para que
un test que falla se pueda trazar directo a la línea de este documento
que lo originó.

## Funcionales — `Gates.Api.Tests`

| ID     | Criterio                                                                                   |
|--------|----------------------------------------------------------------------------------------------|
| AC-01  | `GET /api/products` responde `200` con un array (JSON).                                     |
| AC-02  | `POST /api/products` con datos válidos responde `201`, header `Location` y body con `Id`.    |
| AC-03  | `GET /api/products/{id}` de un producto existente responde `200` con los datos correctos.    |
| AC-04  | `GET /api/products/{id}` con id inexistente responde `404`.                                  |
| AC-05  | `POST /api/products` con `Price <= 0` responde `400` con detalle mencionando `Price`.        |
| AC-06  | `POST /api/products` con `Name` vacío responde `400` con detalle mencionando `Name`.          |
| AC-07  | `POST /api/products` con `Name` duplicado (case-insensitive) responde `400`.                 |
| AC-08  | `POST /api/products` con `Stock < 0` responde `400` con detalle mencionando `Stock`.          |
| AC-09  | `PUT /api/products/{id}` existente con datos válidos responde `200` con los datos actualizados. |
| AC-10  | `PUT /api/products/{id}` con id inexistente responde `404`.                                   |
| AC-11  | `PUT /api/products/{id}` con datos inválidos responde `400`.                                  |
| AC-12  | `DELETE /api/products/{id}` existente responde `204`; un `GET` posterior al mismo id responde `404`. |
| AC-13  | `DELETE /api/products/{id}` con id inexistente responde `404`.                                |

## Arquitectura / Constitution — `Gates.Architecture.Tests`

| ID       | Criterio                                                                                  |
|----------|---------------------------------------------------------------------------------------------|
| ARCH-01  | Ningún tipo del assembly `Api` depende de `Microsoft.EntityFrameworkCore`.                   |
| ARCH-02  | Ningún tipo del assembly `Api` depende del assembly `Infrastructure`.                        |
| ARCH-03  | Ningún tipo del assembly `Domain` depende del assembly `Infrastructure`.                     |
| ARCH-04  | Ningún tipo del assembly `Domain` depende de `Microsoft.EntityFrameworkCore` ni de ningún otro framework. |
| ARCH-05  | Todo tipo que hereda de `ControllerBase` en el assembly `Api` tiene un nombre que termina en `Controller`. |

## Estilo / naming — `.editorconfig` (warning-as-error)

| ID       | Criterio                                                                                  |
|----------|---------------------------------------------------------------------------------------------|
| STYLE-01 | Interfaces empiezan con `I` (`IDE1006`, severity error).                                     |
| STYLE-02 | Campos privados en `_camelCase` (`IDE1006`, severity error).                                 |
| STYLE-03 | Tipos y miembros públicos en `PascalCase` (`IDE1006`, severity error).                        |
| STYLE-04 | Nullable reference warnings tratados como error (`CS8600`-`CS8625`).                          |

## Cómo correr todo

```powershell
./scripts/verify.ps1
```

o (bash):

```bash
./scripts/verify.sh
```

Exit code `0` = todos los gates en verde. Cualquier otro valor = al
menos un gate rojo (build, arquitectura, estilo o funcional).

> Nota: mientras `/src` esté vacío o incompleto, los `AC-xx` van a
> fallar por conexión rechazada (no hay API corriendo) y los `ARCH-xx`
> van a fallar porque no encuentran los assemblies compilados. Ese
> rojo es la demo: el harness detecta "no implementado" igual que
> detectaría una violación de arquitectura.
