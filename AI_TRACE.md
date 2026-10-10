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

## 2026-10-10 - Actualizacion Integral de Documentacion a Beat Potato (Papa Beat)

- Tarea: Actualizacion de toda la documentacion tecnica y de diseno (GDD maestro, plantilla oficial de entrega, README del repositorio y archivos de reglas de agentes).
- Nombre oficial: Beat Potato (Papa Beat).
- Genero y concepto: Duelo de ritmo arcade estilo Friday Night Funkin' (FNF) con 4 carriles direccionales y mecanica de papa caliente.
- Modos de juego: Duelo 2 Jugadores Local (1v1 en el mismo teclado) y Modo 1 Jugador en solitario contra la maquina (CPU con dificultades Facil, Normal y Dificil).
- Esquema de controles en el mismo PC: Jugador 1 mediante flechas direccionales (Left, Down, Up, Right) y Jugador 2 mediante teclas WASD (A: Izq, S: Abajo, W: Arriba, D: Der), con botones dedicados de activacion de poder (Enter / Espacio).
- Sistema de calificaciones y tiempos: Evaluacion milimetrica con dspTime para calificaciones Perfect (+-50 ms), Great (+-100 ms), Good (+-150 ms) y Miss (>+-150 ms o notas no pulsadas).
- Dinamica de la papa y calor: Movimiento oscilatorio basado en dspTime; los aciertos empujan la papa al oponente; los fallos la retienen en el campo propio sumando calor (+20%) hasta detonar al alcanzar el 100%.
- Rachas y poderes especiales: Sistema de combos donde 5 aciertos consecutivos otorgan un poder de sabotaje (Papa Cohete para acelerar notas, Papa Fantasma para notas invisibles, etc.).
- Archivos actualizados: Docs/papa-voley-diseno.md, Docs/Nombre del juego (Plantilla GDD).md, README.md, AGENTS.md, AI_TRACE.md.
- Criterios de entrega: Formato formal sin emojis y sin referencias a conceptos o iteraciones previas.

## 2026-10-10 - Division de responsabilidades del backend
- Participante: Jose David Rodriguez Pinilla.
- Tarea: Revisar la estructura y el GDD actualizado, y asignar el trabajo de backend entre Felipe Calderon y Jose David.
- Archivo creado: `Docs/Division de responsabilidades - Backend.md`.
- Resumen: Se documento el estado observado de Core, ritmo y gameplay; se asignaron ritmo, notas, entrada y CPU a Felipe, y estado de partida, calor, rachas y poderes a Jose David. Se definieron dependencias, pruebas y reglas para coordinar cambios en `GameEvents` y los `.meta` de Unity.
- Pruebas: La suite existente `Tests/run-core-tests.ps1` se ejecuto con los cambios actuales y reporto 35 pruebas aprobadas.
- Limitaciones: El traslado de `BombController`, `NoteSpawner` y `NoteObject` a las nuevas carpetas sigue pendiente de integracion en Git; el estado y las rutas descritos corresponden al arbol de trabajo revisado.

## 2026-10-10 - Reorganizacion Arquitectonica y Adaptacion de Core a Beat Potato

- Participante: Jose David Rodriguez Pinilla
- Tarea: Reorganizacion de carpetas a la arquitectura oficial de Beat Potato y adaptacion de los scripts de Core (GameEnums, GameConfig, GameEvents, MatchManager) junto con su suite de pruebas unitarias.
- Archivos reubicados:
  - `Party Potato/Assets/Scripts/Gameplay/BombController.cs` (y .meta, movido desde `Minigame/`)
  - `Party Potato/Assets/Scripts/Rhythm/NoteSpawner.cs` (y .meta, movido desde `Minigame/`)
  - `Party Potato/Assets/Scripts/Rhythm/NoteObject.cs` (y .meta, movido desde `Minigame/`)
  - Eliminacion de la carpeta obsoleta `Party Potato/Assets/Scripts/Minigame/` (y .meta)
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Core/GameEnums.cs`
  - `Party Potato/Assets/Scripts/Core/GameConfig.cs`
  - `Party Potato/Assets/Scripts/Core/GameEvents.cs`
  - `Party Potato/Assets/Scripts/Core/MatchManager.cs`
  - `Tests/Core/GameEnumsTests.cs`
  - `Tests/Core/GameEventsTests.cs`
  - `Tests/Core/MatchManagerTests.cs`
- Resumen: Se migraron los archivos de la carpeta provisional `Minigame/` a las carpetas estandarizadas `Gameplay/` y `Rhythm/`. Se actualizaron las enumeraciones (`GameEnums`) agregando `LaneDirection` (4 carriles FNF), `GameMode` (1v1 Local / Solo vs CPU), `AIDifficulty` (Facil, Normal, Dificil) y la escala completa de calificaciones `HitResult` (Miss, Good, Great, Perfect). Se calibro `GameConfig` con los valores oficiales de diseno (tolerancias de 50, 100 y 150 ms, incremento de +20% calor por miss y umbral de explosion al 100%). Se ampliaron los disparadores de `GameEvents` para cubrir impactos por carril, posicion continua de la papa y actualizacion de calor. Se refactorizo `MatchManager` para resolver rondas unicamente tras la detonacion de la papa (`OnPotatoExploded`) eliminando la finalizacion prematura por fallo individual y permitiendo juego continuo simultaneo.
- Pruebas realizadas: Ejecucion automatizada de `Tests/run-core-tests.ps1` compilando la suite completa; las 35 pruebas unitarias pasaron satisfactoriamente.
- Limitaciones: Falta conectar la emision real de calor desde `PotatoHeatSystem.cs` y el mapeo de teclas en `PlayerInput.cs`.

## 2026-10-10 - Refactorizacion y Completitud de RhythmManager

- Participante: Felipe Calderon
- Tarea: Implementacion del reloj musical de alta precision, conversiones bidireccionales y evaluacion desacoplada en RhythmManager.cs.
- Archivos creados/modificados:
  - `Party Potato/Assets/Scripts/Rhythm/RhythmManager.cs`
  - `Tests/Core/UnityEngineStubs.cs`
  - `Tests/Rhythm/RhythmManagerTests.cs`
  - `Tests/run-rhythm-tests.ps1`
- Resumen: Se actualizo RhythmManager para operar como unico reloj musical basado estrictamente en AudioSettings.dspTime. Se anadieron metodos de conversion bidireccional (BeatToDspTime, DspTimeToBeat, BeatToSongPosition, SongPositionToBeat) para permitir que notas y carriles calculen sus tiempos objetivo sin depender de la tasa de fotogramas ni de posiciones visuales. Se incorporo emision automatica de eventos OnBeat y OnMeasure a traves de GameEvents en cada pulso de la cancion. Se implemento el metodo EvaluateHit para clasificar notas en Perfect (+-50 ms), Great (+-100 ms), Good (+-150 ms) y Miss segun GameConfig, soportando reduccion de ventana por Papa Electrica.
- Pruebas realizadas: Se compilaron y ejecutaron 5 pruebas unitarias automatizadas en `Tests/run-rhythm-tests.ps1` (calculo de duracion de beats por BPM, conversiones matematicas, ciclo de reproduccion, evaluacion de ventanas de impacto y tolerancia de Papa Electrica), aprobando al 100%. Adicionalmente se verificaron las 35 pruebas de Core en `Tests/run-core-tests.ps1`.
- Limitaciones: Falta conectar la generacion de notas por patrones en `NoteSpawner.cs`, el manejo de auto-miss en `NoteObject.cs` y la lectura de entradas del teclado en `PlayerInput.cs`.

## 2026-10-10 - Actualizacion de NoteSpawner y NoteObject

- Participante: Felipe Calderon
- Tarea: Completar NoteSpawner.cs y NoteObject.cs segun las reglas de la division de responsabilidades del backend.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Rhythm/NoteSpawner.cs`
  - `Party Potato/Assets/Scripts/Rhythm/NoteObject.cs`
  - `Tests/Rhythm/NoteAndInputTests.cs`
  - `Tests/Core/UnityEngineStubs.cs`
- Resumen:
  - **NoteSpawner**: Se corrigio `HandlePowerExpired` para rastrear la victima exacta de cada poder (`rocketVictim`, `ghostVictim`) y revertir unicamente el modificador del jugador afectado al expirar, evitando desactivar efectos activos del otro jugador. Se agregaron comentarios de arquitectura alineados con el GDD y la division de responsabilidades.
  - **NoteObject**: Se agrego el metodo `GetClosestActiveNoteByTime` que acepta la tolerancia en segundos (igual unidad que `GameConfig.goodTolerance` y `RhythmManager.EvaluateHit`), eliminando la necesidad de conversion manual en `PlayerInput` y `AIController`. Se extrajo `ApplyGhostEffect` como metodo privado para mayor claridad. Se mejoraron los comentarios de invariante y responsabilidades.
  - **UnityEngineStubs**: Se agrego el stub de `HideInInspectorAttribute` necesario para compilar `NoteObject.cs` fuera de Unity.
  - **NoteAndInputTests**: Se agregaron dos nuevas pruebas: `TestGetClosestActiveNoteByTime` (verifica busqueda por segundos) y `TestNoteSpawnerPowerExpiryTargetsCorrectVictim` (verifica que la expiracion de poder revierte solo la victima correcta).
- Pruebas realizadas: 12 pruebas de ritmo y entrada (`run-rhythm-tests.ps1`) + 35 pruebas de Core (`run-core-tests.ps1`) = 47 pruebas aprobadas al 100%.
- Limitaciones: Falta implementar `AIController.cs`. La validacion en el Editor de Unity (carriles, prefabs, posiciones de spawn) sigue pendiente del integrador.

## 2026-10-10 - Completitud y Correccion de PlayerInput.cs

- Participante: Felipe Calderon
- Tarea: Completar PlayerInput.cs segun los requisitos de la division de responsabilidades del backend.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Rhythm/PlayerInput.cs`
  - `Tests/Rhythm/NoteAndInputTests.cs`
- Resumen:
  - Se corrigio `HandlePowerExpired` que reseteaba ambos jugadores al expirar `ElectricPotato`. Ahora se rastrea `electricVictim` (igual que `rocketVictim`/`ghostVictim` en NoteSpawner) y solo se revierte el estado del jugador afectado.
  - Se reemplazo la busqueda manual con conversion beats/segundos por la llamada directa a `NoteObject.GetClosestActiveNoteByTime`, alineando las unidades con `GameConfig` y `EvaluateHit`.
  - Se reforzó la restriccion de 1 carga maxima por jugador en `HandlePowerUnlocked`: si ya hay una carga almacenada, la nueva se descarta.
  - Se consumo la carga de poder antes de publicar `TriggerPowerActivated` para evitar doble activacion en condiciones de carrera.
  - Se mejoraron comentarios de arquitectura e invariantes segun el GDD.
  - Se agregaron 4 nuevas pruebas unitarias: `TestPlayerInputElectricPotatoExpiryTargetsCorrectVictim`, `TestPlayerInputMaxOnePowerCharge`, `TestPlayerInputPowerActivationCycle`, `TestPlayerInputP2FourLaneIndependence`.
- Pruebas realizadas: 16 pruebas de ritmo y entrada + 35 de Core = 51 pruebas aprobadas al 100%.
- Limitaciones: Falta implementar `AIController.cs`. La validacion en el Editor de Unity (mapeo de teclas, prefabs, escena Main) sigue pendiente del integrador de Unity.

## 2026-10-10 - Implementacion Integral de la Evaluacion de Notas

- Participante: Felipe Calderon
- Tarea: Implementar la evaluacion de notas (Punto 4 de la division de responsabilidades del backend).
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Rhythm/RhythmManager.cs`
  - `Party Potato/Assets/Scripts/Rhythm/NoteObject.cs`
  - `Party Potato/Assets/Scripts/Rhythm/PlayerInput.cs`
  - `Tests/Core/UnityEngineStubs.cs`
  - `Tests/Rhythm/RhythmManagerTests.cs`
  - `Tests/Rhythm/NoteAndInputTests.cs`
- Resumen:
  - Se estructuro el sistema de evaluacion temporal desacoplado en `RhythmManager.cs`, agregando `ClassifyTimeDifference`, `GetHitScore`, `EvaluateHitFull` (empaquetado en `NoteEvaluationResult`) y sobrecarga `EvaluateNote(NoteObject)`.
  - Se ajusto el calculo de `accuracyNormalized` en `RhythmManager.EvaluateHit` y `ClassifyTimeDifference` para coincidir exactamente con los rangos porcentuales definidos en el GDD de forma continua y monotonica: Perfect (>=90%, 0.90 a 1.00), Great (70% a 89%, 0.70 a 0.89), Good (50% a 69%, 0.50 a 0.69) y Miss (0%), respetando las tolerancias de `GameConfig` y la ventana reducida bajo Papa Electrica.
  - Se incorporo en `NoteObject.cs` el metodo `EvaluateAndResolve(bool isElectric)` para unificar el flujo de evaluacion entre `PlayerInput` y `AIController`, junto con una proteccion en `ResolveHit` que redirige cualquier `HitResult.Miss` a `ResolveMiss()`, garantizando que `OnNoteHit` nunca se dispare en un fallo.
  - Se actualizo `HandleLanePress` en `PlayerInput.cs` para retornar `HitResult` directamente facilitando su consulta por pruebas e IA sin alterar la publicacion de eventos en el bus.
  - Se agrego el stub de `Mathf.Lerp` en `UnityEngineStubs.cs` para soporte fuera de Unity.
  - Se incorporaron 9 pruebas unitarias nuevas (4 en `RhythmManagerTests` y 5 en `NoteAndInputTests`), validando limites de ventanas exactos, simetria temporal, despacho estricto de eventos (`OnNoteHit`, `OnHitResolved` vs `OnNoteMiss`, `OnPlayerMiss`), proteccion anti-spam (`Ghost Tap`) y consumo unico de notas.
- Pruebas realizadas: 25 pruebas de ritmo y entrada (`run-rhythm-tests.ps1`) + 35 de Core (`run-core-tests.ps1`) = 60 pruebas unitarias aprobadas al 100%.
- Limitaciones: Falta implementar `AIController.cs` (Punto 5 del backend). La integracion visual (animaciones, prefabs, HitFeedbackUI) corresponde al responsable de Unity.


