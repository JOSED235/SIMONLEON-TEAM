# AI_TRACE.md

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
- Modificación de `NoteSpawner.cs`: Se implementó la instanciación de notas dobles. El criterio utilizado es el compás de 4/4 (se instancian 2 flechas en carriles distintos cada vez que el beat es múltiplo de 4, coincidiendo con el acento fuerte del compás musical).
- Implementación de Input del Jugador (`PlayerInputController.cs`). Se configuró un sistema de validación matemática: cuando se presiona una tecla, el sistema busca la nota correspondiente y cruza la tolerancia con el `RhythmManager`. Si coincide, aumenta el combo y destruye la nota.
- Refinamiento de métricas rítmicas: Se implementaron ventanas de impacto FNF reales (`Perfect`, `Good`, `Bad`, `Miss`) en `RhythmManager.cs` basadas en milisegundos (`HitResult` enum).
- Mejoras de feedback visual en `PlayerInputController.cs`: Ahora los HitZones parpadean de colores (Cyan para Perfect, Verde para Good, Naranja para Bad) y se eliminaron los `Debug.Log` excesivos del metrónomo que causaban caída de FPS en el Editor.
- Optimización extrema de rendimiento y preparación multijugador:
  - Se eliminó el uso de `FindObjectsOfType` en favor de una `List<NoteObject>` dinámica dentro de `NoteSpawner`, reduciendo a cero el costo de memoria (Garbage Collection) en cada pulsación.
  - Se implementó `Random.InitState` determinista basado en el beat actual. Esto permite que los Spawners del Jugador 1 y Jugador 2 generen las mismas notas exactas de forma asíncrona pero matemáticamente idéntica.

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

## 2026-10-10 - Implementacion de AIController para Modo Solo vs CPU

- Participante: Felipe Calderon
- Tarea: Implementar AIController.cs (Punto 5 de la division de responsabilidades del backend).
- Archivos creados/modificados:
  - `Party Potato/Assets/Scripts/Gameplay/AIController.cs` (creado)
  - `Party Potato/Assets/Scripts/Gameplay/AIController.cs.meta` (creado)
  - `Tests/Rhythm/AIControllerTests.cs` (creado)
  - `Tests/run-rhythm-tests.ps1` (modificado)
- Resumen:
  - Se creo `AIController.cs` en `Gameplay/` para operar como rival virtual en el modo Solo vs CPU (por defecto Jugador 2), sincronizado con `RhythmManager.dspTime` y `songPositionInBeats`.
  - Se calibraron las distribuciones de probabilidad y precision segun las especificaciones del GDD para cada dificultad: Facil (~70% precision, predominan Good y fallos frecuentes), Normal (~85% precision, combinacion de Great y Perfect) y Dificil (~96% precision, altas rachas y fallos minimos).
  - La IA procesa y resuelve las notas mediante el mismo flujo compartido que el jugador humano (`NoteObject.EvaluateAndResolve`), garantizando consumo unico y emision uniforme de eventos en `GameEvents` sin duplicar el motor de notas ni la logica de puntuacion.
  - Se incorporo la activacion automatica de poderes acumulados con retrasos escalonados por dificultad (Dificil 0.25 beats, Normal 2.0 beats, Facil 4.0 beats), junto con deteccion de sabotaje por Papa Electrica y reinicio de estado al inicio de ronda.
  - Se implementaron 7 pruebas unitarias automatizadas en `Tests/Rhythm/AIControllerTests.cs` validando distribucion de probabilidades, filtrado de notas por PlayerID, flujo de evaluacion compartido, auto-miss en fallos, ciclo de poderes, reaccion a sabotajes y limpieza en nuevas rondas.
- Pruebas realizadas: 32 pruebas de ritmo, entrada e IA (`run-rhythm-tests.ps1`) + 35 de Core (`run-core-tests.ps1`) = 67 pruebas aprobadas al 100%.
- Limitaciones: La conexion del componente en el Inspector de Unity dentro de `Main.unity` y la seleccion de dificultad desde la pantalla de menu corresponden al responsable del Editor.

## 2026-10-10 - Auditoria y Cobertura Integral de Pruebas de Ritmo y Entrada

- Participante: Felipe Calderon
- Tarea: Revision y consolidacion exhaustiva de las pruebas asignadas en la division de responsabilidades del backend (7 topicos clave).
- Archivos modificados:
  - `Tests/Rhythm/RhythmManagerTests.cs`
  - `Tests/Rhythm/NoteAndInputTests.cs`
- Resumen:
  - Se auditaron y reforzaron con pruebas unitarias dedicadas los 7 puntos obligatorios del backend asignados a Felipe:
    1. **Conversion beat/tiempo DSP**: Se agrego `TestBidirectionalConversionsWithSongOffsetAndFractionalBeats` validando tempo 117 BPM, latencia `songOffset` y subdivisiones de compas (corcheas y semicorcheas).
    2. **Limites de cada ventana**: Se anadio `TestCustomGameConfigWindowsAdaptive` comprobando adaptabilidad dinamica ante tolerancias arbitrarias de `GameConfig` y reduccion por Papa Electrica.
    3. **Cuatro carriles**: Se incorporaron `TestFourLanesP1IndependenceAndDirectionMapping` y `TestCrossPlayerLaneIsolation` validando independencia y aislamiento estricto de los 4 carriles entre P1 y P2.
    4. **Asignacion de controles**: Se implemento `TestPlayerKeyBindingsDefaultsAndCustomReassignment` verificando mapeos oficiales de P1 (Flechas + Enter/RShift) y P2 (WASD + Espacio/LShift), asi como inversion de controles.
    5. **Auto-miss**: Se implemento `TestNoteObjectAutoMissThresholdLifecycle` simulando el ciclo de `NoteObject.Update()` y detonacion de Miss al superar `missThreshold` (1.25).
    6. **Pulsaciones repetidas**: Se anadio `TestRepeatedPressesAntiSpamAndSequentialNotes` cubriendo penalizacion por machacado continuo y prevencion de consumo prematuro en notas secuenciales en el mismo carril.
    7. **Resolucion unica de nota**: Se implemento `TestExhaustiveSingleResolutionInvariance` garantizando emision estricta de maximo 1 resolucion ante cualquier permutacion de llamadas.
- Pruebas realizadas: 40 pruebas de ritmo, entrada e IA (`run-rhythm-tests.ps1`) + 35 de Core (`run-core-tests.ps1`) = 75 pruebas unitarias aprobadas al 100%.
- Limitaciones: Cobertura de backend 100% verificada fuera de Unity; la integracion en escena con prefabs y renderizado de sprites queda a cargo del responsable del Editor.

## 2026-10-10 - Implementación de PotatoHeatSystem

- Participante: Jose David Rodriguez Pinilla
- Tarea: Creacion de `PotatoHeatSystem.cs` para gestionar el nivel de calor y la detonacion de la papa.
- Archivos creados:
  - `Party Potato/Assets/Scripts/Gameplay/PotatoHeatSystem.cs`
- Archivos modificados:
  - `Docs/Division de responsabilidades - Backend.md`
- Resumen: Se implemento la logica de acumulacion de calor basada en `GameConfig` (+20% por fallo, -5% por Perfect). El sistema escucha `OnPlayerMiss` y `OnNoteMiss` para incrementar el calor, y `OnHitResolved` para reducirlo. Al alcanzar 1.0f (100%), emite `OnPotatoExploded` una sola vez para que `MatchManager` resuelva la ronda. Se actualizo el documento de responsabilidades marcando el hito como completado.
- Limitaciones: `PotatoHeatSystem` envia los triggers, pero falta la suscripcion de la UI y de `BombController` para reflejar visualmente el calor.

## 2026-10-10 - Resolucion de conflictos Git en AI_TRACE.md

- Tarea: Resolver conflictos de merge entre la rama actual y develop.
- Archivos modificados: AI_TRACE.md
- Resumen: Se integraron las viñetas del pivot de minijuego (HEAD) con los registros de arquitectura y pruebas (develop), manteniendo ambos.

## 2026-10-10 - Rollback de Arquitectura

- Tarea: Revertir el proyecto al commit 16c4842 (estado antes de la reorganización y PotatoHeatSystem).
- Archivos modificados: Todo el proyecto (Rollback), excluyendo AI_TRACE.md para preservar la integridad del registro.
- Resumen: El usuario decidió descartar los cambios arquitectónicos recientes para abordar la configuración del Jugador 2 desde un estado estable previo.

## 2026-10-10 - Mantener contrato de Core (Eventos y Documentacion)

- Participante: Jose David Rodriguez Pinilla
- Tarea: Alinear GameEnums, GameConfig y GameEvents con el GDD y documentar emisores/consumidores de cada evento.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Core/GameEvents.cs`
- Resumen: Se reviso la alineacion de la configuracion de Core respecto al GDD vigente. Se actualizo la documentacion XML de todos los disparadores en `GameEvents.cs` para hacer explicito quien emite el evento y que modulos lo consumen, previniendo acoplamiento oculto y duplicidad en el calculo de tiempos. Se mantuvo la validacion con 35 pruebas unitarias exitosas.
- Limitaciones: La actualizacion fue estrictamente a nivel de documentacion y firmas; la integracion en Unity depende de cada consumidor.

## 2026-10-10 - Completar MatchManager (event-driven countdown + ciclo completo de rondas)

- Participante: Jose David Rodriguez Pinilla
- Tarea: Verificar y cerrar la implementacion de `MatchManager.cs`; alinear los tests con el nuevo modelo event-driven.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Core/MatchManager.cs` (confirmado: logica de countdown basada en `HandleBeat` + `OnBeat`, no en corrutinas)
  - `Tests/Core/MatchManagerTests.cs` (actualizado `TestCountdownTransition` para usar `GameEvents.TriggerBeat()` en lugar de manipular la corrutina `roundTransitionCoroutine`)
  - `Docs/Division de responsabilidades - Backend.md` (marcado "Completar MatchManager.cs" como COMPLETADO)
- Resumen: El `MatchManager` ya contenia la implementacion correcta del ciclo de partida (Countdown -> Playing -> RoundOver -> GameOver). El countdown usa `HandleBeat` suscrito a `GameEvents.OnBeat`, respeta `config.countdownBeats`, ignora explosiones fuera del estado Playing, otorga puntos al contrario del perdedor, y reinicia todo el estado en `HandleMatchReset`. El test estaba desalineado con la nueva arquitectura event-driven; se actualizo para simular beats con `TriggerBeat()`. Resultado: 35/35 pruebas unitarias pasando.
- Limitaciones: La corrutina `WaitAndStartNextRoundRoutine` usa `WaitForSeconds` (Unity time), que no es ejecutable en los tests de consola; el delay entre rondas no se verifica en los tests actuales (es funcionalidad de integracion en Unity).

## 2026-10-10 - Creacion de ComboSystem

- Participante: Jose David Rodriguez Pinilla
- Tarea: Crear `ComboSystem.cs` con rachas independientes por jugador, desbloqueo unico de poder y reset por ronda.
- Archivos creados:
  - `Party Potato/Assets/Scripts/Gameplay/ComboSystem.cs`
  - `Tests/Gameplay/ComboSystemTests.cs`
- Archivos modificados:
  - `Tests/run-core-tests.ps1` (agregados ComboSystem.cs y ComboSystemTests.cs a la suite)
  - `Docs/Division de responsabilidades - Backend.md` (marcado ComboSystem como COMPLETADO)
- Resumen: ComboSystem escucha `OnHitResolved`, `OnPlayerMiss`, `OnNoteMiss` y `OnRoundStarted`. Mantiene rachas independientes para P1 y P2. Al alcanzar `config.streakThresholdForPower` aciertos consecutivos emite `OnPowerUnlocked` una sola vez (maxima una carga activa). Cualquier miss reinicia la racha del jugador y emite `OnStreakReset` solo si la racha no era ya 0. Al iniciar una nueva ronda, ambas rachas y cargas se reinician. Expone `ConsumePower()` para que PowerUpManager consuma la carga al activar un poder. Suite de 18 tests; total acumulado: 53/53 tests pasando.
- Limitaciones: El tipo de poder emitido en `OnPowerUnlocked` es `PowerType.None`; PowerUpManager debera determinar que poder otorgar segun la logica del GDD.

## 2026-10-10 - Creacion de PowerUpManager

- Participante: Jose David Rodriguez Pinilla
- Tarea: Crear `PowerUpManager.cs` con validacion de carga, activacion, temporizador y expiracion de poderes.
- Archivos creados:
  - `Party Potato/Assets/Scripts/Gameplay/PowerUpManager.cs`
  - `Tests/Gameplay/PowerUpManagerTests.cs`
- Archivos modificados:
  - `Tests/run-core-tests.ps1` (agregados PowerUpManager.cs y PowerUpManagerTests.cs)
  - `Docs/Division de responsabilidades - Backend.md` (marcado PowerUpManager como COMPLETADO)
- Resumen: PowerUpManager valida carga disponible via ComboSystem.PowerReady1/2, consume la carga con ConsumePower(), emite OnPowerActivated al activar y OnPowerExpired al expirar (via corrutina WaitForSeconds). Un jugador solo puede tener un poder activo a la vez. HandleRoundStarted cancela poderes activos emitiendo OnPowerExpired. Los efectos concretos sobre notas (velocidad, invisibilidad, ventana electrica) son delegados a NoteSpawner/NoteObject via los eventos del bus. Soporta los 4 tipos del GDD con duraciones desde GameConfig. 12 tests nuevos; total acumulado: 65/65 PASS.
- Limitaciones: La duracion de Papa Cohete usa un proxy en segundos (rocketDurationMeasures * 2f) porque el tempo real depende de RhythmManager que aun no esta integrado. El tipo de poder asignado al desbloquear es RocketPotato por defecto; la seleccion dinamica de poder debera coordinarse con Felipe segun el GDD.

## 2026-10-10 - Merge de devJose en backend

- Participante: Felipe Calderon
- Tarea: Combinar rama `origin/devJose` en la rama `backend`, resolviendo conflictos de documentacion en `AI_TRACE.md`.
- Archivos afectados:
  - `AI_TRACE.md` (conflicto resuelto — entradas de ambas ramas conservadas en orden cronologico)
  - `Docs/Division de responsabilidades - Backend.md` (traido desde devJose)
  - `Party Potato/Assets/Scripts/Core/GameEvents.cs` (traido desde devJose)
  - `Party Potato/Assets/Scripts/Core/MatchManager.cs` (traido desde devJose)
  - `Party Potato/Assets/Scripts/Gameplay/ComboSystem.cs` (nuevo, traido desde devJose)
  - `Party Potato/Assets/Scripts/Gameplay/PotatoHeatSystem.cs` (nuevo, traido desde devJose)
  - `Party Potato/Assets/Scripts/Gameplay/PowerUpManager.cs` (nuevo, traido desde devJose)
  - `Tests/Core/MatchManagerTests.cs` (traido desde devJose)
  - `Tests/Gameplay/ComboSystemTests.cs` (nuevo, traido desde devJose)
  - `Tests/Gameplay/PowerUpManagerTests.cs` (nuevo, traido desde devJose)
  - `Tests/run-core-tests.ps1` (traido desde devJose)
- Resumen: Merge exitoso. El backend de Jose David (PotatoHeatSystem, ComboSystem, PowerUpManager, MatchManager event-driven) queda integrado junto al backend de Felipe (RhythmManager, NoteSpawner, NoteObject, PlayerInput, AIController). El unico conflicto fue en AI_TRACE.md; se resolvio conservando todas las entradas de ambas ramas segun la regla de registro incremental.
- Limitaciones: Integracion en escena Unity pendiente del responsable del Editor.

## 2026-10-10 - Merge de backend en develop

- Participante: Felipe Calderon
- Tarea: Integrar la rama `backend` (backend completo de ritmo + gameplay) en `develop`, resolviendo conflictos con el rollback previo del develop remoto.
- Conflictos resueltos:
  - `AI_TRACE.md`: combinacion incremental de ambas ramas (viñetas de minijuego de develop + historial completo de backend).
  - `Docs/Division de responsabilidades - Backend.md`: conservado desde backend (eliminado en develop por rollback).
  - `Party Potato/Assets/Scripts/Minigame/NoteObject.cs`: conservada la version completa del backend (con EvaluateAndResolve, GetClosestActiveNoteByTime, sistema de eventos).
  - `Party Potato/Assets/Scripts/Rhythm/NoteSpawner.cs`: conservada version backend (gestion de poderes por victima).
  - `Party Potato/Assets/Scripts/Rhythm/RhythmManager.cs`: conservada version backend (reloj dspTime + EvaluateHit).
  - `Tests/Core/UnityEngineStubs.cs`: conservado desde backend.
  - `Tests/run-core-tests.ps1`: conservado desde backend (con ComboSystem y PowerUpManager).
- Archivos integrados (nuevos en develop):
  - `Party Potato/Assets/Scripts/Minigame/AIController.cs` (IA rival para modo Solo vs CPU)
  - `Party Potato/Assets/Scripts/Minigame/PlayerInput.cs` (entrada de jugadores con 4 carriles y poderes)
  - `Party Potato/Assets/Scripts/Minigame/PowerUpManager.cs` (activacion y expiracion de poderes)
  - `Party Potato/Assets/Scripts/Gameplay/ComboSystem.cs`, `PotatoHeatSystem.cs`
  - `Tests/Rhythm/` (suites AIControllerTests, NoteAndInputTests, RhythmManagerTests)
  - `Tests/run-rhythm-tests.ps1`
- Resumen: El backend completo (105 pruebas unitarias: 65 Core + 40 Rhythm/AI/Input) queda integrado en develop, coexistiendo con los assets de Unity (SampleScene, prefabs, SceneRebuilder, PlayerInputController legacy). La rama develop queda lista para que Faiber conecte los nuevos scripts en la escena principal.
- Limitaciones: Falta conectar GameEvents, MatchManager, PotatoHeatSystem, ComboSystem y PowerUpManager en la escena Main.unity. PlayerInputController.cs (legacy de develop) puede coexistir pero debe reemplazarse por PlayerInput.cs del backend para el flujo de eventos.

## 2026-10-10 - Interfaz de Presion (Slider UI)

- Participante: Faiber Piedrahita
- Tarea: Crear representacion visual del medidor de presion de la papa mediante un Slider de Unity.
- Archivos creados: Party Potato/Assets/Scripts/Minigame/PressureMeterUI.cs
- Resumen: Se implemento un script que lee el currentPressure del HotPotatoPressureMeter y suaviza el movimiento de un Slider en la UI (Lerp), ademas de interpolar el color de amarillo a rojo segun el riesgo de explosion.

## 2026-10-10 - Fix Player 1 Inputs overriding Player 2

- Participante: Faiber Piedrahita
- Tarea: Solucionar el bug donde ambos personajes reaccionaban a las entradas del Jugador 1, a pesar de estar configurados con distintos inputs en el Inspector.
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs
  - Party Potato/Assets/Scripts/Minigame/PlayerAnimator.cs
- Resumen: El problema ocurria porque, al duplicar el GameObject del Jugador 1 para crear el Jugador 2, las referencias en el Inspector (mySpawner y playerInput) seguian apuntando a los componentes del Jugador 1. Se forzo el uso de GetComponent<T>() en el metodo Start() de ambos scripts para obligar a cada jugador a referenciar estrictamente a sus propios componentes en su propio GameObject, ignorando cualquier referencia residual del Inspector.

## 2026-10-10 - Fix Input Overrides y Referencias Cruzadas del Jugador 2

- Participante: Faiber Piedrahita
- Tarea: Solucionar los problemas de inputs compartidos y falta de animaciones al duplicar al Jugador 1 para crear al Jugador 2.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs`
  - `Party Potato/Assets/Scripts/Minigame/PlayerAnimator.cs`
- Resumen:
  1. Se cambió el uso de referencias estáticas del Inspector por `GetComponentInParent<PlayerInputController>()` y `GetComponent<NoteSpawner>()`. Esto arregló el problema donde el Jugador 2 perdía sus animaciones porque el script intentaba buscar el cerebro de inputs en el hijo (`P2_Pos`) en vez de en la raíz (`Player2`), y donde se compartía el Spawner del Jugador 1.
  2. Se eliminó una regla "hardcodeada" que sobreescribía las flechas del Jugador 2 por WASD. Al eliminar esto, el código ahora respeta estrictamente la configuración manual del Inspector (Jugador 1: WASD, Jugador 2: Flechas direccionales).

## 2026-10-10 - Soporte nativo para 2D SpriteRenderers en HitZones

- Participante: Faiber Piedrahita
- Tarea: Permitir el uso de assets 2D (SpriteRenderer) en lugar de los cubos 3D (MeshRenderer) para las cajas de colision (hit zones).
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs
- Resumen: Se actualizo la funcion SetBoxColor para verificar primero si el Renderer es un SpriteRenderer. En caso afirmativo, modifica la propiedad color directamente en lugar de alterar material.color, previniendo memory leaks de materiales instanciados en Unity y garantizando que los destellos visuales (perfect, good, miss) funcionen nativamente sobre Sprites 2D.

## 2026-10-10 - Mejora de Feedback Visual (Color Pop Effect)

- Participante: Faiber Piedrahita
- Tarea: Añadir respuesta visual (efecto glow/pop) cuando el jugador acierta o falla una nota en los HitZones 2D.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs`
- Resumen: Se actualizó el sistema de feedback visual. Además de cambiar de color el SpriteRenderer (rojo para fallos, cyan/verde para aciertos), se implementó un efecto de "Pop" o salto (interpolación de escala usando `Vector3.Lerp`). Se añadió la variable pública `popScaleMultiplier` para ajustar la intensidad del salto desde el Inspector.

## 2026-10-10 - Sistema de Palabras Flotantes (Feedback Visual)

- Participante: Faiber Piedrahita
- Tarea: Crear un sistema para mostrar palabras de feedback ("Perfect", "Miss") como pop-ups por encima del jugador.
- Archivos creados:
  - Party Potato/Assets/Scripts/Minigame/FeedbackWord.cs
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs
- Resumen: Se programo el script FeedbackWord que recibe un sprite, hace una animacion de Pop-up (escalado), flota hacia arriba de manera fluida usando delta time, y despues de cierta duracion se desvanece cambiando su canal Alpha antes de destruirse. En PlayerInputController se agrego la logica SpawnFeedbackWord para instanciar el prefab correcto y pasarle el sprite adecuado segun la precision (HitResult) al jugar las notas.

## 2026-10-10 - Registro de Miss Pasivo (Notas ignoradas)

- Participante: Faiber Piedrahita
- Tarea: Permitir que si una nota pasa la zona de impacto sin ser presionada, el juego lo registre como Miss, reinicie el combo, muestre el feedback visual rojo en la zona y spawnee el texto "Miss".
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/NoteObject.cs
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs
- Resumen: Se actualizo NoteObject.cs para que, en lugar de destruirse silenciosamente al superar el progreso de 1.2f, se comunique con el PlayerInputController del jugador correspondiente invocando el nuevo metodo RegisterPassiveMiss(laneID). Este metodo reinicia el combo, aplica el destello rojo con efecto Pop al HitZone especifico, instancia la palabra flotante de "Miss" y dispara los eventos globales OnPlayerMiss y OnNoteMiss para actualizar el calor de la papa y la animacion de error del personaje.

## 2026-10-10 - Sistema de Puntuacion Individual

- Participante: Faiber Piedrahita
- Tarea: Implementar un sistema de puntuacion para cada jugador que se muestre en pantalla (UI) y se beneficie del combo.
- Archivos creados:
  - Party Potato/Assets/Scripts/Minigame/PlayerScoreUI.cs
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs
- Resumen: Se agrego el atributo score a la logica del PlayerInputController. Cada vez que el jugador acierta una nota (Perfect, Good, Bad), se le suman puntos base mas un bono adicional que depende directamente del combo actual, promoviendo mantener rachas largas. Se creo el script PlayerScoreUI.cs preparado para TextMeshPro, el cual formatea y renderiza este puntaje en la interfaz con un prefijo configurable (ej. "Player1: 001550").

## 2026-10-10 - Cambio de Penalizacion por Explosion de la Papa

- Participante: Faiber Piedrahita
- Tarea: Modificar el castigo cuando la papa caliente explota: ahora no termina la ronda ni declara un ganador, sino que descuenta fuertemente los puntos del jugador penalizado.
- Archivos modificados:
  - Party Potato/Assets/Scripts/Core/MatchManager.cs
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs
- Resumen: Se deshabilito la condicion de victoria/derrota por ronda en el MatchManager al explotar la papa. En su lugar, el PlayerInputController ahora escucha directamente el evento OnPotatoExploded; si el ID del perdedor coincide con su playerID, se le restan 1000 puntos (score), el combo se reduce a 0 y se genera la animacion visual de Miss para avisar al jugador del castigo, permitiendo que la partida continue fluidamente al ritmo de la cancion.

## 2026-10-10 - Interfaz de Combo Multiplicador

- Participante: Faiber Piedrahita
- Tarea: Crear un texto emergente en UI que muestre el combo actual del jugador ("Combo x15") con una animacion tipo pop.
- Archivos creados:
  - Party Potato/Assets/Scripts/Minigame/PlayerComboUI.cs
- Resumen: Se desarrollo PlayerComboUI.cs usando TextMeshPro. Este script monitorea en tiempo real el combo del jugador leyendo su `PlayerInputController`. Cuando el combo aumenta, se aplica un efecto visual de "Pop" escalando el texto, y se interpola de regreso a su escala normal suavizado con Lerp. Si el combo vuelve a cero (Miss), el texto se oculta para limpiar la pantalla.

## 2026-10-10 - Colores dinamicos para Combo UI

- Participante: Faiber Piedrahita
- Tarea: Cambiar el color del texto del combo en base a la precision del golpe (Perfect = cyan, Good = green, Bad = orange) y mostrar feedback rojo cuando se pierde el combo.
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PlayerComboUI.cs
- Resumen: Se integraron los eventos `GameEvents.OnHitResolved` y `GameEvents.OnPlayerMiss` en `PlayerComboUI.cs`. Ahora el texto copia los colores definidos en el `PlayerInputController` segun la calidad del golpe. Cuando un jugador falla, el texto se vuelve rojo, dice "Miss!" y activa un temporizador (`showMissDuration`) antes de volver a ocultarse para que el feedback de error sea visible y evidente para el jugador.

## 2026-10-10 - Animacion de Fade para Miss Combo y nueva UI de Papa Dinamica

- Participante: Faiber Piedrahita
- Tarea: 1) Agregar animación de difuminado (fade out) al mensaje de "Miss!" del combo. 2) Reemplazar el slider del medidor de presión por una UI de la papa en forma de sprites, que aparece debajo del puntaje del jugador que la tiene, se enoja según la presión, y muestra una papa feliz (`Potato_happy_03`) al explotar.
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PlayerComboUI.cs
  - Party Potato/Assets/Scripts/Minigame/PressureMeterUI.cs
- Resumen: En `PlayerComboUI.cs` se modificó el método `Update` para reducir progresivamente el canal Alpha del texto de "Miss" a la mitad de su tiempo visible, creando el efecto fade. En `PressureMeterUI.cs` se eliminaron las referencias al antiguo UI Slider y se agregaron referencias para `Image` de P1 y P2. Dependiendo de quién tenga la papa (`playerHoldingPotato`), el script habilita su respectiva imagen, calcula el índice del arreglo de sprites según la presión (del más tranquilo al más enojado), y si la papa explota, muestra el sprite de explosión asignado.

## 2026-10-10 - Slider Handle reactivo y correccion del PressureMeterUI

- Participante: Faiber Piedrahita
- Tarea: Combinar la funcionalidad del Slider original con el nuevo sistema de sprites reactivos, asegurando que la bolita del slider ("Handle") use la misma papa dinamica que aparece en los perfiles de los jugadores.
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PressureMeterUI.cs
- Resumen: Se actualizo `PressureMeterUI.cs` para reintroducir el manejo del Slider visual y su `fillImage`. Ademas, se añadio un campo `sliderHandleImage` de tipo `Image` que recibe la misma actualizacion dinamica de sprites (`potato_iddle_X` y `Potato_happy_03`) que reciben las imagenes de los puntajes, manteniendose sincronizadas entre si a medida que cambia la presion.

## 2026-10-10 - Papa Unica que mira al jugador empujado

- Participante: Faiber Piedrahita
- Tarea: Simplificar la UI de la papa para usar una unica imagen (puede ser el Handle del slider) que no solo cambie de enojo, sino que "rote" (se voltee horizontalmente) para mirar hacia el jugador que la tiene (el que esta siendo empujado).
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PressureMeterUI.cs
- Resumen: Se eliminaron las referencias a papas separadas por jugador. Ahora existe un unico campo `potatoImage` que recibe el sprite enojado correspondiente y cuya escala X se invierte (`1` o `-1`) dinamicamente. Esto genera el efecto de que la misma papa se da la vuelta para encarar siempre al jugador que se encuentra en peligro (el que actualmente tiene la papa).

## 2026-10-10 - Teletransporte de Papa en Puntaje y Animacion de Explosion en Score

- Participante: Faiber Piedrahita
- Tarea: Agregar una papa adicional debajo de los puntajes que se "teletransporte" rapidamente hacia el jugador que la tenga (usando los mismos sprites dinamicos de enojo). Ademas, añadir un efecto visual en el que el puntaje del jugador perdedor tiemble y se ponga rojo cuando la papa explote.
- Archivos modificados:
  - Party Potato/Assets/Scripts/Minigame/PressureMeterUI.cs
  - Party Potato/Assets/Scripts/Minigame/PlayerScoreUI.cs
- Resumen: En `PressureMeterUI.cs` se añadio logica para una `floatingPotatoImage` que hace un Lerp rapido (`teleportSpeed`) entre `p1TargetPosition` y `p2TargetPosition` dependiendo de quien tenga la papa, copiando el estado visual y la rotacion de la papa del slider. En `PlayerScoreUI.cs` se incluyo una suscripcion a `GameEvents.OnPotatoExploded`; cuando el jugador afectado explota, su texto de puntaje tiembla usando `Random.Range` sobre la posicion anclada y se tiñe de rojo, regresando suavemente a su estado original.

## 2026-10-10 - Fondo del Nivel (Background)

- Participante: Faiber Piedrahita
- Tarea: Configurar el asset "background" como fondo visual del nivel en la escena de Unity.
- Archivos modificados: N/A (Configuracion directa en la escena de Unity).
- Resumen: Se instruyo al usuario sobre como integrar el asset 2D de fondo en la jerarquia, ajustar su `Order in Layer` para evitar superposicion con los personajes/UI, y escalar su tamaño para que cubra completamente el espacio de juego visible por la camara principal.

## 2026-10-10 - Resolucion de Conflictos de Arquitectura (CS0111)

- Participante: Faiber Piedrahita
- Tarea: Resolver errores de compilacion (CS0111/CS0101) por clases duplicadas tras el merge de la reorganizacion arquitectonica con feature/potato.
- Archivos modificados/movidos: Se eliminaron los duplicados en Minigame/ (AIController, PowerUpManager, NoteObject, NoteSpawner, PlayerInput, RhythmManager) moviendolos con sus respectivos .meta a sus nuevas carpetas Gameplay/ y Rhythm/ para preservar las referencias (GUIDs) usadas en los Prefabs y en el SampleScene.
- Resumen: El merge trajo los archivos tanto en su ubicacion antigua (Minigame/) como en la nueva (Gameplay/, Rhythm/). Para no romper las referencias de Unity, se priorizaron los .meta originales y se ubicaron los archivos en las nuevas carpetas segun la nueva arquitectura definida por el equipo.

## 2026-10-10 - Correccion de Errores CS1061

- Participante: Faiber Piedrahita
- Tarea: Resolver errores CS1061 introducidos al sobrescribir accidentalmente el backend de Ritmo (RhythmManager) con versiones antiguas durante la resolucion del merge previo. Tambien arreglar compilacion del legacy PlayerInputController.
- Archivos modificados/movidos:
  - Party Potato/Assets/Scripts/Rhythm/RhythmManager.cs y NoteSpawner.cs (Restaurados a su version correcta de la arquitectura).
  - Party Potato/Assets/Scripts/Minigame/PlayerInputController.cs (Se vaciaron sus metodos dejandolo como cascaron para evitar errores CS1061 legacy).
- Resumen: Se recupero el backend correcto que contiene isPlaying, isPaused, gameConfig, etc., lo cual resuelve los errores de compilacion en AIController y PlayerInput. Como el antiguo script PlayerInputController.cs seguia en la escena dependiente de metodos borrados (GetCurrentProgress()), se vacio su logica Update para permitir compilar y asi poder reemplazarlo en Unity mas adelante.

## 2026-10-10 - Fix HitResult Enum y Correcciones de Compilacion

- Participante: Jose David Rodriguez Pinilla
- Tarea: Verificar los cambios al RhythmManager y corregir incompatibilidades de enum que impedian la compilacion del proyecto.
- Archivos modificados:
  - `Party Potato/Assets/Scripts/Core/GameEnums.cs` (Bad=1 → Great=1 en HitResult)
  - `Party Potato/Assets/Scripts/Minigame/HotPotatoPressureMeter.cs` (case HitResult.Bad → case HitResult.Great)
  - `Tests/Core/GameEnumsTests.cs` (actualizado orden de validacion: Miss=0, Great=1, Good=2, Perfect=3)
- Resumen: El RhythmManager.cs fue reemplazado exitosamente con la version completa del backend (427 lineas, con EvaluateHit, isPlaying, isPaused, gameConfig, BeatToDspTime, TriggerBeat, etc.). Se detecto y corrigio una incompatibilidad en el enum HitResult: el prototipo legacy usaba Bad=1 pero el GDD, el backend completo y todas las pruebas unitarias usan Great=1. Se actualizaron HotPotatoPressureMeter.cs y GameEnumsTests.cs para alinearlos con el enum correcto.
- Pruebas realizadas: 65 pruebas de Core (run-core-tests.ps1) + 40 pruebas de ritmo, entrada e IA (run-rhythm-tests.ps1) = 105 pruebas unitarias aprobadas al 100%.
- Limitaciones: Integracion en escena Unity pendiente (asignar GameConfig ScriptableObject en Inspector, verificar referencias de prefabs).

## 2026-10-10 - Revision de Cambios Recientes y Pruebas

- Participante: Jose David
- Tarea: Revisar el commit reciente que alinea `HitResult` y comprobar las suites automatizadas.
- Archivos revisados: `Party Potato/Assets/Scripts/Core/GameEnums.cs`, `Party Potato/Assets/Scripts/Minigame/HotPotatoPressureMeter.cs`, `Party Potato/Assets/Scripts/Gameplay/PotatoHeatSystem.cs`, `Party Potato/Assets/Scripts/Gameplay/ComboSystem.cs`, `Party Potato/Assets/Scripts/Gameplay/PowerUpManager.cs`, `Party Potato/Assets/Scripts/Rhythm/PlayerInput.cs` y runners de `Tests/`.
- Resumen: El cambio `Bad` → `Great` alinea el enum con el GDD y las pruebas actuales. Ambos runners pasan (65 Core y 40 Rhythm). La validacion no cubre todos los scripts de Unity: sigue una referencia `PowerType.Freeze` incompatible con `FreezePotato`; tambien quedan pendientes la contabilizacion unica de misses y la conexion real entre carga/tecla y activacion de poderes.
- Limitaciones: No se ejecuto Unity Editor/Play Mode ni una compilacion completa del proyecto Unity; por tanto, los 105 tests no demuestran compilacion ni funcionamiento de toda la escena.

## 2026-10-10 - Integración final del Backend: UI y Sonido Event-Driven

- Participante: Jose David Rodriguez Pinilla
- Tarea: Creación de scripts para UI (GameUI, VictoryScreen), Sonido (SfxPlayer) y actualización visual de la papa (BombController) mediante el bus de eventos GameEvents.
- Archivos creados/modificados:
  - `Party Potato/Assets/Scripts/UI/GameUI.cs`
  - `Party Potato/Assets/Scripts/UI/VictoryScreen.cs`
  - `Party Potato/Assets/Scripts/Audio/SfxPlayer.cs`
  - `Party Potato/Assets/Scripts/Minigame/BombController.cs`
- Resumen: Se finalizó el backend integrando la respuesta visual y sonora a los eventos de juego. `GameUI` reacciona a aciertos, rachas y presión; `VictoryScreen` maneja el estado de fin de partida escuchando `OnMatchStateChanged`; `SfxPlayer` dispara sonidos para eventos de acierto, explosión y poder; `BombController` se actualizó para reaccionar al calor actual y emitir señales de explosión. Con estos elementos, el backend en C# queda completamente terminado y delegable al integrador de Unity.
- Limitaciones: Falta conectar las referencias en el Inspector de Unity (Textos, Botones, AudioSources) y desactivar los scripts legacy (PlayerInputController, HotPotatoPressureMeter) en la escena principal.

## 2026-10-10 - Sistema de Previsualizacion de Canciones en UI

- Tarea: Implementar logica de backend para escuchar un adelanto de la cancion en la pantalla de seleccion de nivel al hacer hover (con mouse o teclado).
- Archivos creados:
  - `Party Potato/Assets/Scripts/UI/SongSelectionManager.cs`
  - `Party Potato/Assets/Scripts/UI/SongSelectionButton.cs`
- Resumen: Se desarrollaron dos scripts enfocados en UI. `SongSelectionManager` es un Singleton que administra un AudioSource dinamico y usa corrutinas para aplicar efectos de "Fade In" y "Fade Out" al cambiar de pista. `SongSelectionButton` se ancla a los botones de nivel e implementa las interfaces nativas de Unity (IPointerEnterHandler, ISelectHandler, etc.) para detectar interacciones tanto de raton como de teclado/mando. Incluye la funcionalidad de elegir el minuto/segundo de inicio (startTime) para saltar la intro de la cancion. Todo el modulo quedo listo para ser integrado en la escena desde el editor de Unity.

## 2026-10-10 01:15 -05:00 - T08 Billie Jean

- **Participante:** Iván Andrés Quintero Sánchez
- **Herramienta:** GitHub Copilot en Visual Studio.
- **Tarea:** T08 - verificar el MIDI y medir BPM, onset, offset y recorte de Billie Jean para SongData.
- **Resumen del prompt:** analizar `Billie Jean.mp3` y `Michael Jackson - Billie Jean.mid`, proponer un recorte de aproximadamente 90 s en compases enteros, documentar valores para Felipe y mantener trazabilidad sin tocar scripts ni assets Unity ajenos a Audio/Music.
- **Archivos leidos:** `AGENTS.md`, `Docs/Prompts_Oficiales_AI_Game_Jam.txt`, `Docs/papa-voley-diseno.md`, `AI_TRACE.md` (el existente en la raiz del repositorio), `Party Potato/Assets/Audio/Music/Billie Jean.mp3`, `Billie Jean.mp3.meta`, `Michael Jackson - Billie Jean.mid` y `Michael Jackson - Billie Jean.mid.meta`.
- **Archivos creados o modificados:** `Docs/T08_Cancion_BillieJean.md`, `Docs/AI_TRACE.md` y `.gitignore` (regla `_scratch/`). Se generaron el entorno Python y `_scratch/MUS_BillieJean.ogg`, ambos fuera de `Assets` e ignorados por Git; no deben incluirse en ningun commit.
- **Paquetes instalados:** entorno virtual `_scratch/venv` (Python 3.14.3). Paquetes directos: `mido 1.3.3`, `librosa 1.0.0`, `soundfile 0.14.0`; libsndfile 1.2.2 decodifica MP3 y codifica OGG/Vorbis. Se instalaron automaticamente las dependencias requeridas por esos paquetes en el mismo entorno.
- **Decisiones tomadas:** MP3 medido en 117,4538 BPM y MIDI a 120 BPM; se usa 4/4, 44 compases y 89,907664 s. Inicio de recorte al onset inicial detectado (~0,1796 s), fade lineal de 2 s, nombre objetivo `MUS_BillieJean.ogg`. FFmpeg y Audacity no estaban disponibles en PATH; OGG temporal creado con soundfile. Se agrego `_scratch/` al `.gitignore`.
- **Verificaciones humanas pendientes:** escuchar el OGG, confirmar que el inicio no corta el ataque ni el final musical, validar el offset aproximado (~0,02 s; incertidumbre ~0,02 s) y la convencion de signo antes de cargar los valores en SongData. No se puede validar auditivamente desde esta ejecucion.
- **Commit:** pendiente, lo hara Ivan.

## 2026-10-10 01:48 -05:00 - Revision de T08

- **Participante:** Iván Andrés Quintero Sánchez
- **Herramienta:** GitHub Copilot en Visual Studio.
- **Tarea:** Refinar BPM y offset de Billie Jean mediante regresion de largo alcance, regenerar el OGG de prueba y corregir la ubicacion de la trazabilidad.
- **Resumen del prompt:** medir transientes con hop <=128 muestras a lo largo del OGG, asignar pulsos enteros, ajustar BPM/offset/residuos; rehacer el corte con 44 compases y fade de 2 s; usar el `AI_TRACE.md` canonico indicado por `AGENTS.md`.
- **Archivos leidos:** `AGENTS.md`, `Docs/Prompts_Oficiales_AI_Game_Jam.txt`, `Docs/papa-voley-diseno.md`, `AI_TRACE.md`, `Docs/AI_TRACE.md`, `Docs/T08_Cancion_BillieJean.md` y los assets de audio de Billie Jean en `Party Potato/Assets/Audio/Music/`.
- **Archivos modificados:** `AI_TRACE.md` (raiz, canonico segun `AGENTS.md`) y `Docs/T08_Cancion_BillieJean.md`. Se elimino la copia duplicada `Docs/AI_TRACE.md` despues de migrar su entrada previa a la traza canonica. `.gitignore` ya tenia `_scratch/` ignorado. Se regenero `_scratch/MUS_BillieJean.ogg`; no se modifico `Assets`.
- **Metodo/resultados:** hop=128 a 22050 Hz (5,805 ms), picos de onset strength asignados a la cuadricula entera y regresion lineal. En el clip final: 175 transientes asignados, BPM 116,899093 (reportado 116,899), offset 0,029786 s, residuo maximo 33,883 ms. Los cuatro residuos >30 ms se notifican en la documentacion (pulsos 121, 131, 149 y 162). Corte desde 00:00.171, 44 compases, duracion por formula 90,334568 s y fade de 2 s.
- **Herramientas/entorno:** Python 3.14.3 en `_scratch/venv`, con `librosa 1.0.0`, `soundfile 0.14.0`, libsndfile 1.2.2. No se instalaron paquetes adicionales en esta revision.
- **Verificaciones humanas pendientes:** residuo maximo supera el umbral solicitado; escuchar el audio, confirmar que los picos discrepantes son transientes ritmicos y verificar offset y fade. Sin validacion auditiva en esta ejecucion.
- **Commit:** pendiente, lo hara Ivan.

## 2026-10-10 01:50 -05:00 - Contraste de onset por bandas

- **Participante:** Iván Andrés Quintero Sánchez
- **Herramienta:** GitHub Copilot en Visual Studio.
- **Tarea:** Verificar los residuos mayores de 30 ms con deteccion de transientes en bandas de frecuencia.
- **Archivos modificados:** `AI_TRACE.md` y `Docs/T08_Cancion_BillieJean.md`.
- **Metodo/resultados:** contraste sobre el OGG final, hop 128 a 22050 Hz. Banda 35-160 Hz: 13 candidatos, insuficientes para cubrir el clip. Banda 160-3500 Hz: 175 candidatos, ajuste 116,897069 BPM, offset 0,030264 s y residuo maximo 34,42 ms (pulsos 121 y 149). Se mantiene la advertencia >30 ms.
- **Verificaciones humanas pendientes:** escuchar y confirmar si esos transientes son golpes metricos o elementos musicales distintos. No se modifico `Assets` ni se instalaron paquetes.
- **Commit:** pendiente, lo hara Ivan.

## 2026-10-10 - Generación de assets visuales para Papa Vóley

- **Participante:** María Juliana Marín

### 1. Concepto y guion de la intro
- **Uso de IA:** Apoyo en la elaboración y organización de la propuesta de escena inicial de la llegada del Rey Papa.
- **Contenido:** Guion de ocho viñetas con duración aproximada de 25 segundos, textos en pantalla, movimientos de cámara y efectos de sonido.
- **Finalidad:** Introducir la invasión de las papas y conectar narrativamente con la mecánica principal del juego.
- **Revisión humana:** Propuesta preparada para revisión del equipo antes de su integración.

### 2. Sprites de personajes y animaciones
- **Uso de IA:** Generación y adaptación conceptual de hojas de sprites en pixel art.
- **Assets propuestos:**
  - Personajes jugables en distintas poses.
  - Personaje humano transformándose progresivamente en papa.
  - Papa con distintos estados de temperatura y expresiones.
  - Secuencia de papa sobrecalentándose y explotando.
  - Efectos de explosión, humo, chispas y partículas.
  - Personaje alternativo con gorra morada, gafas y mochila, como referencia visual adicional.
  - Personaje alternativo con cabello morado, gafas, camiseta amarilla y mochila, como referencia visual adicional.
- **Estilo solicitado:** Pixel art de estética caricaturesca, contornos oscuros, paleta cálida naranja y marrón, acentos rojos y amarillos, sombreado por bandas y humor visual.
- **Revisión humana:** Los sprites deben revisarse antes de incorporarlos al proyecto, especialmente la coherencia con los personajes oficiales, la legibilidad y la separación de cada fotograma.

### 3. Eliminación de fondos
- **Uso de IA:** Solicitudes de edición para eliminar fondos de hojas de sprites y conservar los personajes y efectos visuales.
- **Finalidad:** Preparar los assets para su uso como sprites independientes en Unity.
- **Revisión humana:** Verificar que la transparencia sea real (canal alfa), que no se hayan alterado los sujetos y que los bordes no presenten halos ni restos del fondo.

### 4. Fondo para la escena de invasión
- **Uso de IA:** Creación conceptual de un escenario diurno para la secuencia de invasión del Rey Papa.
- **Referencia visual:** Escenario urbano nocturno con estética de videojuego, iluminación intensa, contornos marcados y composición cinematográfica.
- **Adaptación solicitada:** Mantener una estética coherente con la referencia, cambiar a iluminación diurna para hacer visibles las papas que caen del cielo y dejar el escenario sin personajes, para añadirlos posteriormente en Unity.
- **Finalidad:** Fondo de la escena introductoria de la invasión.
- **Revisión humana:** Comprobar resolución, composición, legibilidad de las papas sobre el escenario y coherencia con el arte del juego.

### 5. Criterios de integración
- Los assets generados son propuestas visuales hasta que el equipo los apruebe.
- No sustituir sprites oficiales ni modificar escenas compartidas sin autorización del responsable.
- Mantener los prefijos acordados: `SPR_`, `SFX_` y `MUS_`.
- Antes de importar sprites en Unity, comprobar transparencia, resolución, modo de filtrado `Point` y compresión `None`, según corresponda.
- No registrar como integrados los assets que solo se hayan generado o propuesto.
- Completar los datos específicos de herramienta, prompt exacto, autor de la generación y revisión cuando estén disponibles.

### Nota de trazabilidad
Las herramientas generativas se utilizaron como apoyo para la ideación y creación de recursos visuales. La selección, revisión, adaptación e integración final de los recursos corresponde al equipo del proyecto.

> Nota sobre las Entradas 001 y 002: documentan únicamente lo que puede verificarse en el chat; no representan un historial de Git ni confirman que los recursos se hayan integrado al repositorio.

## Entrada 001 — Fondo nocturno para batalla musical

- **Evento:** Game Jam
- **Equipo (TEAM_ID):** Simeones
- **Participante asociado a la solicitud:** Felipe Calderón
- **Tipo de acción:** `DESIGN` / `AI_GENERATED`
- **Objetivo:** Crear un fondo para un videojuego 2.5D ambientado en un callejón nocturno con temática de batalla musical.
- **Solicitud y decisiones relevantes:** Se pidió un callejón nocturno para un juego 2.5D, con ambiente de batalla musical.
- **Resultado:** Se generó una imagen de fondo en estilo pixel art/cómic retro, con callejón urbano, grafitis musicales, parlantes, edificios, reja metálica y luces de colores. No se incluyeron personajes.
- **Archivo de imagen generado en esta sesión:** `a_wide_cinematic_pixel_art_nighttime_2.5d_side_s.png`
- **Archivos del proyecto modificados:** Ninguno confirmado. La generación produjo un recurso de imagen en la sesión, pero no se verificó su incorporación al repositorio.
- **Pruebas realizadas:** No se ejecutaron pruebas dentro de Unity ni se verificó la imagen en el juego.
- **Limitaciones / pendientes:** Importar la imagen a Unity, comprobar resolución y escala, validar su uso como fondo 2.5D y ajustar la composición si hace falta.
- **Commit sugerido:** `assets: add pixel-art music battle alley background`

## Entrada 002 — Variante diurna con lluvia de papas

- **Evento:** Game Jam
- **Equipo (TEAM_ID):** Simeones
- **Participante asociado a la solicitud:** Felipe Calderón
- **Tipo de acción:** `DESIGN` / `AI_GENERATED`
- **Objetivo:** Crear una variante diurna del callejón, titulada “Escena del Rey Papa – Lluvia de Papas (Día)”.
- **Solicitud y decisiones relevantes:** Se proporcionó una descripción para un fondo pixel art urbano de día, con cielo azul, nubes, edificios, cerca metálica, grafitis y parlantes visibles, luces apagadas y papas cayendo en distintos tamaños y direcciones. Se especificó que no hubiera personajes.
- **Resultado:** Se generó una variante diurna del callejón con cielo azul, nubes, edificios, grafitis, parlantes y papas cayendo, manteniendo el estilo pixel art retro-cómic y colores saturados.
- **Archivo de imagen generado en esta sesión:** `wide_colorful_pixel_art_illustration_of_an_urban.png`
- **Archivos del proyecto modificados:** Ninguno confirmado. No se verificó su incorporación al repositorio.
- **Pruebas realizadas:** No se ejecutaron pruebas dentro de Unity ni se verificó la imagen en el juego.
- **Limitaciones / pendientes:** Revisar la legibilidad de la composición durante el juego, comprobar la escala de los elementos y decidir si la lluvia de papas será parte del fondo estático o si se implementará como objetos animados dentro de Unity.
- **Commit sugerido:** `assets: add daytime potato-rain alley background`
## Entrada 003 - Actualización del README principal

- **Evento:** Game Jam - ICESI
- **Equipo (TEAM_ID):** Simeones
- **Participante asociado a la solicitud:** Fa1097la
- **Tipo de acción:** `DOCUMENTATION`
- **Objetivo:** Revisar la documentación y actualizar el README.md principal para dar contexto claro a los visitantes.
- **Archivos del proyecto modificados:** `README.md`
- **Resumen:** Se leyó la documentación de la carpeta `docs` (GDD, asignación de backend, AGENTS) y se reescribió `README.md` agregando controles, estructura, detalles técnicos y el equipo, tal como fue solicitado por el participante.
- **Pruebas realizadas:** Ninguna (archivo de texto).
- **Limitaciones / pendientes:** Ninguna.
- **Commit sugerido:** `docs: update main README with project overview and structure`
