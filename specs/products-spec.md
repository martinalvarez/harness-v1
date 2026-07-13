# Feature Spec: Products API

Define **qué** debe hacer el sistema. No prescribe clases, carpetas ni
librerías: eso es responsabilidad de quien implemente en `/src` respetando
`constitution.md`.

## Entidad: Product

| Campo | Tipo    | Regla                                              |
|-------|---------|-----------------------------------------------------|
| Id    | GUID    | Asignado por el sistema al crear. Inmutable.         |
| Name  | string  | Requerido, no vacío, **único** (case-insensitive).   |
| Price | decimal | Requerido, **> 0**.                                  |
| Stock | int     | Requerido, **>= 0**.                                 |

## Endpoints (REST, JSON)

### `GET /api/products`
Devuelve la lista completa de productos. `200 OK` con un array (vacío si
no hay productos).

### `GET /api/products/{id}`
Devuelve un producto por `Id`.
- Existe -> `200 OK` con el producto.
- No existe -> `404 Not Found`.

### `POST /api/products`
Crea un producto nuevo. Body: `Name`, `Price`, `Stock` (sin `Id`).
- Válido -> `201 Created`, header `Location` apuntando a
  `GET /api/products/{id}`, body con el producto creado (incluye `Id`
  generado).
- Inválido (ver reglas de negocio) -> `400 Bad Request` con detalle por
  campo de qué regla se violó.

### `PUT /api/products/{id}`
Actualiza un producto existente. Body: `Name`, `Price`, `Stock`.
- Id existe y body válido -> `200 OK` con el producto actualizado.
- Id no existe -> `404 Not Found`.
- Body inválido -> `400 Bad Request` con detalle por campo.

### `DELETE /api/products/{id}`
- Id existe -> `204 No Content`.
- Id no existe -> `404 Not Found`.

## Reglas de negocio

1. `Price` debe ser estrictamente mayor a 0.
2. `Name` es requerido (no vacío/whitespace) y único entre todos los
   productos, comparación case-insensitive. Intentar crear o actualizar
   con un `Name` que ya existe en otro producto es una violación de
   validación.
3. `Stock` debe ser mayor o igual a 0.

## Edge cases

- `GET`, `PUT` o `DELETE` con un `Id` que no existe -> `404 Not Found`
  (nunca `500` ni `200` con body vacío).
- Cualquier violación de las reglas de negocio en `POST`/`PUT` -> `400
  Bad Request` con el detalle de qué campo(s) fallaron y por qué (no un
  400 genérico sin contexto).
- Crear/actualizar con `Name` duplicado (mismo nombre que otro producto
  existente, ignorando mayúsculas/minúsculas) -> `400 Bad Request`.
- Todas las respuestas de error siguen el formato `ProblemDetails`
  definido en `constitution.md`.

## Fuera de alcance (explícitamente no pedido)

- Autenticación / autorización.
- Paginación, filtros o búsqueda en `GET /api/products`.
- Soft delete / auditoría / versionado.
