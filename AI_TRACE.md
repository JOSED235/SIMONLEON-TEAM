## 2026-10-09 - Pivote a Minijuego Musical & RhythmManager

- El equipo pivota de juego de tablero 2.5D a un minijuego de duelo musical.
- Se añadió la pista MIDI de prueba (Billie Jean) a la carpeta de Assets/Audio.
- Se creó `RhythmManager.cs` que lee `AudioSettings.dspTime` y calcula ventanas de acierto con BPM (117) exactos.
- Se agregó modo Debug para imprimir por consola cuando entra el beat musical.
- Se generó y añadió un archivo `.gitignore` optimizado para Unity en la raíz del proyecto para evitar subir archivos temporales (`Library/`, `Temp/`, `.csproj`, etc.) al repositorio.
- Se implementó `BombController.cs` con lógica de interpolación musical (`Mathf.PingPong` usando `dspTime`) para asegurar sincronización rítmica sin físicas desfasadas.
- Cambio de mecánica: Se implementó un sistema de notas descendentes (estilo Friday Night Funkin).
- Se crearon `NoteSpawner.cs` (orquesta el spawn anticipado) y `NoteObject.cs` (interpola la nota hasta la hitzone basado matemáticamente en el progreso de beats).
- Refactorización de `NoteSpawner.cs`: Soporte para múltiples carriles (Lanes) estilo FNF. Ahora puede instanciar notas aleatoriamente en 4 direcciones (Left, Down, Up, Right) con `laneID`.

## 2026-10-09 - Arquitectura Core: GameEnums y GameEvents

- Participante: Jose David Rodríguez Pinilla
- Tarea: Creación de enumeraciones base (`GameEnums.cs`) y bus de eventos estático desacoplado (`GameEvents.cs`) en `Party Potato/Assets/Scripts/Core/`.
- Archivos creados:
  - `Party Potato/Assets/Scripts/Core/GameEnums.cs`
  - `Party Potato/Assets/Scripts/Core/GameEvents.cs`
- Resumen: Se definieron los enums para jugadores (`PlayerID`), resultados de impacto (`HitResult`: Miss, Good, Perfect), poderes (`PowerType`), ciclo de partida (`MatchState`) y estados de la papa (`PotatoState`). Se implementó el bus de eventos desacoplado con disparadores estáticos y método `ResetAllEvents` para cubrir ritmo, trayectorias de la papa, aciertos, rachas, poderes, calor y control de rondas.
- Limitaciones: Los eventos están declarados y listos para invocarse; falta conectar emisores (`RhythmManager`, `PotatoController`) y receptores (`HitDetection`, `ComboSystem`, `MatchManager`, `GameUI`).

## 2026-10-09 - Pruebas automatizadas para Core

- Participante: Jose David Rodriguez
- Tarea: Crear pruebas para `GameEnums`, `GameEvents`, `GameConfig` y `MatchManager`.
- Archivos creados: `Tests/Core/GameEnumsTests.cs`, `Tests/Core/GameEventsTests.cs`, `Tests/Core/MatchManagerTests.cs`, `Tests/Core/UnityEngineStubs.cs` y `Tests/run-core-tests.ps1`.
- Resumen: Se probaron los valores de los enums, los 17 disparadores de eventos y sus parámetros, limpieza de suscripciones, valores predeterminados de configuración y los estados, turnos, puntajes, victoria, reinicio y suscripciones de `MatchManager`.
- Pruebas: `Tests/run-core-tests.ps1` compiló los scripts Core reales y ejecutó 34 pruebas; todas pasaron.
- Limitaciones: `Party Potato/` no contiene `Packages/` ni `ProjectSettings/`. `MatchManager` se probó con dobles mínimos de `UnityEngine`, no en el Editor/Test Runner; falta validar la integración y temporización real en Unity.
