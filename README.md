# Beat Potato (Papa Beat) 🥔💥

> Duelo Rítmico Arcade 1v1 y Solo vs CPU estilo Friday Night Funkin' mezclado con la dinámica de la Papa Caliente.

## 📖 Resumen

**Beat Potato** es un juego de ritmo arcade para dos jugadores en un mismo PC (con opción individual contra la CPU) desarrollado durante la **Game Jam ICESI** por el equipo **Simeones**. 

Los jugadores pulsan notas al compás de la música en cuatro carriles. Mantener el ritmo empuja una papa explosiva hacia el campo contrario, mientras que fallar notas hace que la papa se caliente. Cuando la papa llega al 100% de calor, explota y el jugador pierde la ronda.

## 🎮 Controles

El juego está diseñado para jugarse localmente de manera cómoda sin conflictos de teclado:

| Acción | Jugador 1 (Lado Izquierdo) | Jugador 2 (Lado Derecho / CPU) |
| :--- | :--- | :--- |
| **Carriles (Izquierda, Abajo, Arriba, Derecha)** | Flechas Direccionales | Teclas W, A, S, D |
| **Activar Poder Especial** | Tecla Enter | Barra Espaciadora |

## 📂 Estructura del Repositorio

Si acabas de entrar al repositorio, esta es la organización clave:

- `docs/`: Contiene toda la documentación esencial del juego.
  - `GDD_Beat_Potato.md`: Game Design Document oficial con las reglas, modos, mecánicas y visión del juego.
  - `Division de responsabilidades - Backend.md`: Detalles de arquitectura de código y responsabilidades de cada miembro del backend.
  - `AGENTS.md`: Instrucciones y reglas oficiales del uso de Inteligencia Artificial en el desarrollo.
  - `AI_TRACE.md`: Registro de auditoría con la trazabilidad del uso de herramientas de IA durante la Jam.
- `Party Potato/`: Proyecto principal de Unity 2D (C#) con los assets, scripts, prefabs y escenas del juego.
- `README.md`: Este archivo.

## 🏗️ Aspectos Técnicos Destacados

- **Sincronización Rítmica:** El tiempo del juego está gobernado por `AudioSettings.dspTime` a través de `RhythmManager.cs`, asegurando precisión absoluta independiente de los FPS.
- **Comunicación de Sistemas:** La arquitectura usa fuertemente eventos globales (`GameEvents.cs`) para desacoplar el gameplay, la interfaz y el audio.
- **Evaluación de Notas:** La precisión de cada acierto (Perfect, Great, Good, Miss) se calcula contra el tiempo musical objetivo, no por su posición gráfica en pantalla.
- **Poderes Especiales:** Mecánicas de sabotaje táctico (Papa Cohete, Papa Fantasma, etc.) que se activan acumulando rachas de aciertos (ComboSystem).

## 👥 Equipo Simeones

- **José David Rodríguez Pinilla:** Backend (Gestor de partidas, estado de la papa, rachas, poderes y eventos).
- **Felipe Calderón:** Backend (Rhythm Manager, notas, inputs del jugador y control de IA).
- **Faiber Piedrahita:** Editor de Unity (Integración de escenas, HUD, Animator, VFX y builds).
- **María Juliana Marín Shek:** Arte 2D y Animaciones.
- **Iván Andrés Quintero:** Audio y Diseño (Composición, SFX, patrones de ritmo, balance).

---
*Desarrollado en 24 horas para la Game Jam ICESI.*
