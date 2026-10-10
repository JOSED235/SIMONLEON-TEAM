# Division de responsabilidades del backend

## Proposito y alcance

Este documento distribuye el trabajo de backend de Beat Potato entre Felipe Calderon y Jose David Rodriguez Pinilla, tomando como referencia el [GDD](./papa-voley-diseno.md), la estructura tecnica y los scripts que existen en el proyecto.

El objetivo inmediato es completar un nucleo jugable para dos personas en el mismo teclado. El modo Solo vs CPU y los poderes adicionales vienen despues de estabilizar ritmo, notas, golpes, calor y rondas. No incluye trabajo de escenas, HUD, Animator, VFX ni build, que corresponden a la integracion en el Editor de Unity.

## Criterios compartidos

- `RhythmManager` es la unica fuente de tiempo musical. Las notas, los golpes y los efectos temporales deben referirse al reloj de audio (`AudioSettings.dspTime`), no usar `Time.time` como reloj alternativo.
- La precision se calcula contra el tiempo programado de la nota, no contra su posicion visual.
- Cada nota se resuelve una sola vez: como golpe o como fallo automatico.
- `GameEvents` es el contrato entre ritmo, gameplay, partida y presentacion. Antes de cambiar la firma o el significado de un evento, ambos responsables acuerdan el cambio y actualizan o coordinan a sus consumidores.
- Los parametros de balance se mantienen en `GameConfig`, no duplicados como constantes dentro de cada sistema.
- El codigo de escenas y sus referencias visuales lo integra el responsable del Editor de Unity; el backend entrega componentes y comunica los eventos que necesita.
- Los archivos `.meta` deben acompanarse al mover o renombrar scripts.

## Estado de la base existente

Estado observado en el arbol de trabajo al preparar esta distribucion:

| Area | Archivos | Estado comprobado |
| --- | --- | --- |
| Contratos y estado | `Core/GameEnums.cs`, `Core/GameEvents.cs` | Existen. Los enums incluyen `Miss`, `Good`, `Great` y `Perfect`, los cuatro carriles, modos, dificultad, poderes y estados. El bus declara eventos para beats, golpes y fallos por carril, calor, poderes y rondas. |
| Balance | `Core/GameConfig.cs` | Existe con ventanas de calificacion, calor, umbrales, poderes, cuenta regresiva y rondas. |
| Flujo de partida | `Core/MatchManager.cs` | Existe; se suscribe a la explosion, puntua la ronda, comprueba la victoria y atiende el reinicio. |
| Reloj musical | `Rhythm/RhythmManager.cs` | Prototipo existente: calcula BPM y posicion musical con `dspTime`. Su comprobacion actual busca el beat entero mas cercano; falta evaluar la ventana respecto al tiempo de cada nota/carril y completar el inicio de audio sincronizado que requiera el juego. |
| Generacion y movimiento de notas | `Rhythm/NoteSpawner.cs`, `Rhythm/NoteObject.cs` | Prototipo en el arbol de trabajo: configura carriles, genera notas y las interpola segun el beat. El spawner actual elige carriles al azar; la nota perdida solo escribe un mensaje y se destruye, sin notificar el resultado al bus de eventos. |
| Papa | `Gameplay/BombController.cs` | Prototipo en el arbol de trabajo: oscila visualmente con el beat, cambia parcialmente el material al calentarse y permite marcarla como explotada. Aun no coordina el calor, los golpes ni la resolucion de ronda mediante eventos. |
| Entrada y evaluacion | `Rhythm/PlayerInput.cs` | Pendiente: no se encontro el script. |
| Calor | `Gameplay/PotatoHeatSystem.cs` | Pendiente: no se encontro el script. |
| Rachas y poderes | `Gameplay/ComboSystem.cs`, `Gameplay/PowerUpManager.cs` | Pendientes: no se encontraron los scripts. |
| Rival CPU | `Gameplay/AIController.cs` | Pendiente: no se encontro el script. |
| Pruebas | `Tests/Core/`, `Tests/run-core-tests.ps1` | Existen pruebas unitarias ejecutables desde PowerShell. La ejecucion de la suite actual reporto 35 pruebas aprobadas; no sustituye una validacion dentro del Editor de Unity. |

### Movimiento de scripts aun por integrar

En el arbol de trabajo, `BombController.cs` esta bajo `Gameplay/` y `NoteSpawner.cs` y `NoteObject.cs` bajo `Rhythm/`. Git aun registra las rutas anteriores `Scripts/Minigame/` como eliminadas y las nuevas rutas como no rastreadas. Antes de cerrar esta reorganizacion, revisar y registrar juntos cada script y su `.meta`, comprobar que los GUID de los `.meta` se conservaron y validar que no queden referencias rotas. No se da por integrado o publicado este movimiento.

## Asignacion principal

### Felipe Calderon — ritmo, notas y entrada

Responsable principal del recorrido de una nota desde su planificacion hasta la evaluacion del golpe.

1. **Completar `RhythmManager.cs`.**
   - Mantener un unico reloj musical basado en `dspTime`.
   - Exponer la conversion entre beat y tiempo DSP para que cada nota tenga un tiempo objetivo consultable.
   - Iniciar la pista de forma coordinada con el reloj del juego y publicar beats/compases mediante `GameEvents`.
   - Evitar que la precision dependa de los FPS o de la posicion visual de la nota.
2. **Completar `NoteSpawner.cs` y `NoteObject.cs`.**
   - Representar las notas de ambos jugadores y conservar su carril y tiempo objetivo.
   - Generar notas desde una secuencia/patron configurable en vez de depender solo de una seleccion aleatoria.
   - Disparar el fallo automatico una sola vez cuando la nota vence.
   - Mantener el movimiento visual separado de la evaluacion temporal.
3. **Crear `PlayerInput.cs`.**
   - Leer Flechas para P1 y WASD para P2, usando el mapeo de carriles definido en el GDD.
   - Detectar pulsaciones de poder con controles separados por jugador.
   - Encontrar una nota valida del jugador y carril correspondiente, solicitar su evaluacion y evitar doble consumo.
4. **Implementar la evaluacion de notas.**
   - Clasificar el error temporal en `Perfect` (±50 ms), `Great` (±100 ms), `Good` (±150 ms) o `Miss`, respetando los umbrales y valores de `GameConfig`.
   - Publicar el resultado con jugador, carril y precision usando el evento acordado (`OnNoteHit`, `OnNoteMiss` y/o `OnHitResolved`).
   - No otorgar aciertos cuando se pulsa fuera de una ventana valida.
5. **Implementar `AIController.cs` despues del modo local.**
   - Responder a las notas del lado CPU usando los mismos tiempos objetivo y el mismo flujo de evaluacion que una entrada humana.
   - Ajustar probabilidad/precision por `AIDifficulty`; no duplicar el motor de notas ni la logica de puntuacion.

**Pruebas a cargo de Felipe:** conversion beat/tiempo DSP, limites de cada ventana, cuatro carriles, asignacion de controles, auto-miss, pulsaciones repetidas, y que cada nota genere como maximo una resolucion.

### Jose David Rodriguez Pinilla — estado de partida, papa y progresion

Responsable principal de convertir los resultados ritmicos en el ciclo de riesgo, rondas y poderes.

1. **Mantener el contrato de Core.**
   - Alinear `GameEnums.cs`, `GameEvents.cs` y `GameConfig.cs` con el GDD vigente.
   - Documentar el significado y los consumidores de cada evento antes de anadir o cambiar una firma.
   - Evitar duplicar en Core la evaluacion temporal que pertenece al flujo de ritmo/entrada.
2. **Completar `MatchManager.cs`.**
   - Iniciar y finalizar rondas usando los eventos del bus; otorgar el punto al contrario del jugador que recibe la explosion.
   - Respetar la cuenta regresiva, el objetivo de rondas y los retrasos configurados.
   - Ignorar resoluciones duplicadas o eventos recibidos fuera del estado valido.
   - Reiniciar puntajes y estado al pedir revancha.
3. **Crear `PotatoHeatSystem.cs` e integrar la papa.**
   - Acumular el calor por jugador segun los fallos y valores de `GameConfig`; informar cambios normalizados para UI/audio.
   - Restablecer o reducir calor de acuerdo con las reglas acordadas para los golpes y poderes.
   - Emitir `OnPotatoExploded` una sola vez al alcanzar el umbral; `MatchManager` usa ese evento para resolver la ronda.
   - Coordinar con Felipe el uso de `OnPotatoPositionUpdated`/`OnPotatoLaunched` para que la papa visual represente el resultado del intercambio, sin usar su posicion para decidir si un golpe acerto.
4. **Crear `ComboSystem.cs`.**
   - Llevar una racha independiente por jugador segun la calificacion recibida.
   - Aplicar la regla de cinco aciertos consecutivos y disponibilidad maxima de una carga.
   - Publicar actualizaciones y reinicios de racha.
5. **Crear `PowerUpManager.cs`.**
   - Empezar por Papa Cohete y Papa Fantasma, como indica la prioridad inicial del GDD.
   - Validar jugador, carga disponible y consumo unico; anunciar activacion y expiracion.
   - Hacer que los efectos cambien las notas del oponente sin bloquear su control ni cambiar el reloj global de la cancion.
   - Dejar Papa Electrica y Papa Helada para despues de estabilizar los dos poderes prioritarios.

**Pruebas a cargo de Jose David:** transiciones de partida, puntuacion y condicion de victoria, explosion unica, calor acotado a 0–1, rachas por jugador, desbloqueo/consumo de una sola carga y expiracion de efectos.

## Trabajo conjunto y dependencias

| Orden | Trabajo | Responsable principal | Dependencia / salida verificable |
| --- | --- | --- | --- |
| 0 | Cerrar el traslado de `BombController`, `NoteSpawner` y `NoteObject` junto con sus `.meta`. | Felipe (Rhythm) y Jose David (Gameplay) | Rutas de destino rastreadas por Git; GUID preservados; sin duplicados ni referencias rotas. |
| 1 | Acordar el contrato de eventos, en especial calificaciones, jugador/carril, fallo de nota y explosion. | Ambos; Jose David mantiene Core | Firmas documentadas y usadas por emisor y receptor. No continuar con dos definiciones de lo que significa un fallo. |
| 2 | Completar entrada, notas y evaluacion temporal. | Felipe | Pruebas de cuatro carriles y ventanas temporales; cada nota se resuelve una vez. |
| 3 | Conectar el resultado de nota con papa/calor y flujo de rondas. | Jose David | Un fallo aumenta el calor; al alcanzar el umbral explota una vez y la ronda se puntua una vez. |
| 4 | Probar una partida local completa y corregir integracion entre sistemas. | Ambos | Dos jugadores pueden jugar; la papa y las rondas responden a golpes y fallos sin desincronizar el ritmo. |
| 5 | Agregar rachas y Papa Cohete/Papa Fantasma. | Jose David; Felipe integra sus efectos con notas | Cinco aciertos consecutivos habilitan una carga; uso y expiracion afectan solo el objetivo previsto. |
| 6 | Agregar modo Solo vs CPU. | Felipe | La CPU juega por la misma ruta de notas/resultados y respeta las tres dificultades. |
| 7 | Agregar poderes de prioridad secundaria y ajustes de balance. | Ambos | Solo despues de validar el ciclo base y las dos habilidades iniciales. |

La salida de cada etapa debe probarse antes de iniciar la siguiente. Las escenas, HUD, efectos visuales y el build se coordinan con el responsable del Editor; ninguno de estos elementos debe incorporarse al backend modificando `Main.unity` sin coordinacion.

## Protocolo de colaboracion

1. El responsable principal implementa y prueba su modulo; el otro revisa la integracion cuando hay dependencias compartidas.
2. Evitar editar simultaneamente el mismo archivo. Para cambios de `GameEvents.cs`, el responsable que lo modifica comunica las nuevas firmas antes de integrar al otro modulo.
3. Un evento de nota (`OnNoteHit`/`OnNoteMiss`) describe una nota/carril; un fallo que cambia el calor no debe confundirse con la explosion (`OnPotatoExploded`), que termina la ronda.
4. Cada movimiento de scripts Unity incluye su `.meta`. Confirmar los cambios de rutas con Git antes de dar la reorganizacion por terminada.
5. Mantener las pruebas enfocadas por sistema y ejecutar la suite `Tests/run-core-tests.ps1` al integrar cambios de Core. La revision en Unity continua siendo necesaria para escenas, prefabs, audio y referencias serializadas.
6. Registrar el trabajo de IA de forma incremental en `AI_TRACE.md` y no sobrescribir entradas previas.
