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
