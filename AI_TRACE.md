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

## 2026-10-10 01:15 -05:00 - T08 Billie Jean
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
- **Herramienta:** GitHub Copilot en Visual Studio.
- **Tarea:** Verificar los residuos mayores de 30 ms con deteccion de transientes en bandas de frecuencia.
- **Archivos modificados:** `AI_TRACE.md` y `Docs/T08_Cancion_BillieJean.md`.
- **Metodo/resultados:** contraste sobre el OGG final, hop 128 a 22050 Hz. Banda 35-160 Hz: 13 candidatos, insuficientes para cubrir el clip. Banda 160-3500 Hz: 175 candidatos, ajuste 116,897069 BPM, offset 0,030264 s y residuo maximo 34,42 ms (pulsos 121 y 149). Se mantiene la advertencia >30 ms.
- **Verificaciones humanas pendientes:** escuchar y confirmar si esos transientes son golpes metricos o elementos musicales distintos. No se modifico `Assets` ni se instalaron paquetes.
- **Commit:** pendiente, lo hara Ivan.
