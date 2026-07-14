# Guía — Qué slide del HTML corresponde a qué parte del guion

Mapeo entre `presentation/index.html` (el deck) y `GUION.md` /
`guion-eng.md` (el texto para decir en voz alta).

| # HTML | Slide | Sección del guion |
|---|---|---|
| 0 | Cover | (no está en el guion — portada nueva) |
| 1 | "The automated control system..." | **Sección 1** — Opening: qué es Harness para mí |
| 2 | "One small Greenfield case..." | **Sección 2** — Why minimalist |
| 3 | "specs/, gates/, scripts/, src/" (mapa de carpetas) | **Sección 3** — parte 1: el árbol de carpetas |
| 4 | "The spec is the primary artifact..." (constitution/products-spec/verification) | **Sección 3** — parte 2: el recorrido por `specs/` |
| 5 | "A tollgate. Doesn't matter who wrote the code." (13 funcionales / 5 arquitectura) | **Sección 3** — parte 3: qué es `gates/` |
| 6 | "Now, watch the loop run." (RED / GREEN / CAUGHT) | **Sección 4** — Live demo (es solo el *anuncio* de los 3 pasos; los pasos en sí se corren en VS Code, no están en el HTML) |
| 7 | "The spec was the feedforward..." | **Sección 5** — Closing |
| 8 | "Anticipated questions" | El bloque final del guion, **"In case they ask"** |

## Ojo con el slide 6

Es solo la transición visual ("ahora miren esto en vivo"), **no
reemplaza** los pasos A/B/C que hay que ejecutar de verdad en la
terminal y en Copilot — para eso hay que saltar del navegador a VS
Code:

- Paso A — correr `.\scripts\verify.ps1` con `/src` vacío (rojo).
- Paso B — pegar el prompt en Copilot Chat, modo Agent (verde).
- Paso C — inyectar la violación a mano y correr `verify.ps1` de nuevo
  (`ARCH01` la caza).

Después de esos 3 pasos en vivo, volver al navegador y saltar al slide
7 (Closing) para cerrar.
