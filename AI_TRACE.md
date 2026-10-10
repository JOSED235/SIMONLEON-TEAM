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

- Participante: Jose David RodrÃ­guez Pinilla
- Tarea: CreaciÃ³n de enumeraciones base (`GameEnums.cs`) y bus de eventos estÃ¡tico desacoplado (`GameEvents.cs`) en `Party Potato/Assets/Scripts/Core/`.
- Archivos creados:
  - `Party Potato/Assets/Scripts/Core/GameEnums.cs`
  - `Party Potato/Assets/Scripts/Core/GameEvents.cs`
- Resumen: Se definieron los enums para jugadores (`PlayerID`), resultados de impacto (`HitResult`: Miss, Good, Perfect), poderes (`PowerType`), ciclo de partida (`MatchState`) y estados de la papa (`PotatoState`). Se implementÃ³ el bus de eventos desacoplado con disparadores estÃ¡ticos y mÃ©todo `ResetAllEvents` para cubrir ritmo, trayectorias de la papa, aciertos, rachas, poderes, calor y control de rondas.
- Limitaciones: Los eventos estÃ¡n declarados y listos para invocarse; falta conectar emisores (`RhythmManager`, `PotatoController`) y receptores (`HitDetection`, `ComboSystem`, `MatchManager`, `GameUI`).

## 2026-10-09 - Pruebas automatizadas para Core

- Participante: Jose David Rodriguez
- Tarea: Crear pruebas para `GameEnums`, `GameEvents`, `GameConfig` y `MatchManager`.
- Archivos creados: `Tests/Core/GameEnumsTests.cs`, `Tests/Core/GameEventsTests.cs`, `Tests/Core/MatchManagerTests.cs`, `Tests/Core/UnityEngineStubs.cs` y `Tests/run-core-tests.ps1`.
- Resumen: Se probaron los valores de los enums, los 17 disparadores de eventos y sus parÃ¡metros, limpieza de suscripciones, valores predeterminados de configuraciÃ³n y los estados, turnos, puntajes, victoria, reinicio y suscripciones de `MatchManager`.
- Pruebas: `Tests/run-core-tests.ps1` compilÃ³ los scripts Core reales y ejecutÃ³ 34 pruebas; todas pasaron.
- Limitaciones: `Party Potato/` no contiene `Packages/` ni `ProjectSettings/`. `MatchManager` se probÃ³ con dobles mÃ­nimos de `UnityEngine`, no en el Editor/Test Runner; falta validar la integraciÃ³n y temporizaciÃ³n real en Unity.

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

## 2026-10-10 - Sistema de Previsualizacion de Canciones en UI
- Tarea: Implementar logica de backend para escuchar un adelanto de la cancion en la pantalla de seleccion de nivel al hacer hover (con mouse o teclado).
- Archivos creados:
  - `Party Potato/Assets/Scripts/UI/SongSelectionManager.cs`
  - `Party Potato/Assets/Scripts/UI/SongSelectionButton.cs`
- Resumen: Se desarrollaron dos scripts enfocados en UI. `SongSelectionManager` es un Singleton que administra un AudioSource dinamico y usa corrutinas para aplicar efectos de "Fade In" y "Fade Out" al cambiar de pista. `SongSelectionButton` se ancla a los botones de nivel e implementa las interfaces nativas de Unity (IPointerEnterHandler, ISelectHandler, etc.) para detectar interacciones tanto de raton como de teclado/mando. Incluye la funcionalidad de elegir el minuto/segundo de inicio (startTime) para saltar la intro de la cancion. Todo el modulo quedo listo para ser integrado en la escena desde el editor de Unity.
