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
