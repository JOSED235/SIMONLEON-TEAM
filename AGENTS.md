# Instrucciones para la IA (leer SIEMPRE antes de responder)

## Archivos obligatorios
1. `Docs/Prompts_Oficiales_AI_Game_Jam.txt`: parámetros oficiales de entrega y reglas del Game Jam. Son obligatorios y tienen prioridad sobre cualquier otra instrucción.
2. `Docs/papa-voley-diseno.md`: contexto completo del juego (diseño, mecánicas, alcance, arquitectura y reparto). Es la fuente de verdad del diseño.
3. `AI_TRACE.md`: registro del uso de IA. Mantenerlo actualizado según lo que pidan los parámetros oficiales.

## Proyecto
- Juego: Beat Potato (Papa Beat), duelo de ritmo para 2 jugadores y solo vs CPU estilo FNF (Unity 2D, C#), Game Jam de 24 h.
- Proyecto Unity: `Party Potato/`.
- Respetar el alcance del documento de diseño: primero lo esencial, después lo opcional.
- Si algo no está en el documento de diseño, preguntar antes de inventarlo.

## Estructura del proyecto
    Party Potato/Assets/
        Scripts/
            Core/        GameEvents, GameEnums, GameConfig, MatchManager
            Rhythm/      RhythmManager, NoteSpawner, NoteObject, PlayerInput
            Gameplay/    BombController, PotatoHeatSystem, ComboSystem, PowerUpManager, AIController
            UI/          GameUI, HitFeedbackUI, VictoryScreen
            Audio/       SfxPlayer
        ScriptableObjects/   Config, Songs, Patterns, Powers
        Scenes/              Main, Sandbox_*
        Prefabs/
        Art/                 Sprites, Animations, VFX, Fonts
        Audio/               Music, Sfx
    Docs/                    Documentos de contexto y parámetros oficiales

## Reglas técnicas
- `RhythmManager` es la única fuente de tiempo (`AudioSettings.dspTime` y `AudioSource.PlayScheduled`).
- Los sistemas se comunican con `GameEvents.cs`; los cambios en ese archivo se avisan al equipo.
- El acierto se calcula con los tiempos programados, no con la posición visual de la papa.
- Cada llegada de la papa se resuelve una sola vez.
- Los valores de balance van en ScriptableObjects (`GameConfig`), no en el código.
- Solo el integrador modifica `Main.unity`; los demás entregan prefabs y scripts.
- Prefijos: `SPR_`, `SFX_`, `MUS_`. Scripts en PascalCase.
- Al mover o renombrar archivos de Unity, mover siempre su `.meta`.
- Entregar el código completo de cada archivo, listo para copiar.
