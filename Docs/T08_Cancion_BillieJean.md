# T08 - Preparacion de Billie Jean

## Resultado actualizado

La estimacion anterior de `librosa.beat_track` (117,454 BPM) era cuantizada y no basta para sincronizar todo el clip. La regresion de largo alcance da **116,899 BPM**, valor recomendado para SongData.

| Dato | Resultado |
| --- | --- |
| BPM medido del audio | **116,899 BPM**; regresion lineal sobre onsets de todo el OGG. Ajuste base: 116,898771 BPM; verificacion en el OGG final: 116,899093 BPM. |
| BPM del MIDI | **120 BPM** (`set_tempo` = 500000 us/negra). |
| ¿Tempo constante? | MIDI: si, un solo evento de tempo. El audio se ajusta a una cuadrícula global estable, aunque algunos transientes superan 30 ms de residuo. |
| Compas / `beatsPerBar` | **4/4 / 4**. |
| Offset del primer pulso del OGG final | **0,029786 s** (~30 ms), medido en el OGG final mediante la interseccion de la regresion con pulso 0. |
| Residuo maximo del ajuste final | **33,88 ms**, superior al umbral solicitado de 30 ms. Cuatro candidatos superan 30 ms: pulsos 121, 131, 149 y 162. Ver advertencia abajo. |
| Inicio del recorte en el MP3 | **00:00.171** (sample 8193 a 48 kHz). |
| Fin del recorte en el MP3 | **01:30.505**. |
| Numero de compases | **44** (176 pulsos). |
| Duracion exacta por formula | **90,334568 s** = 44 x 4 x 60 / 116,898771 BPM. Duracion decodificada: **90,334563 s**, diferencia por redondeo al sample mas cercano a 48 kHz. |
| Fade out | Lineal de **2 s** al final. |
| Nombre final | `MUS_BillieJean.ogg` |
| Herramientas usadas | Python 3.14.3; `librosa` 1.0.0 (onset strength, hop 128 a 22050 Hz = 5,805 ms, deteccion de picos y regresion); `mido` 1.3.3; `soundfile` 0.14.0/libsndfile 1.2.2 para leer MP3 y codificar OGG/Vorbis. FFmpeg y Audacity no estaban disponibles en PATH. |

## Lectura del MIDI

`Michael Jackson - Billie Jean.mid` es tipo 1, con 192 ticks por negra. Tiene un unico evento `set_tempo` al inicio (500000 microsegundos por negra = 120 BPM), una firma de compas 4/4 y la primera nota MIDI a 1,875 s. No hay cambios posteriores de tempo. El MIDI difiere del audio, por lo que se recomienda el BPM medido en la grabacion.

## Metodo de medicion y advertencia

Se analizo la duracion completa del OGG anterior y del OGG regenerado; no se uso `beat_track` para decidir el tempo. A 22050 Hz se calculo `librosa.onset.onset_strength` con `hop_length=128` (5,805 ms por trama). Se buscaron picos locales con prominencia minima 0,35 y separacion de 16 tramas. Para cada pulso entero se asigno el candidato mas fuerte dentro de +/-120 ms y se ajusto `tiempo = offset + pulso x periodo` por minimos cuadrados.

En el OGG previo se asignaron 174 transientes distribuidos a lo largo del clip: 116,898771 BPM, periodo 0,513264591 s y offset 0,021095 s. El primer pulso estimado en el original estaba a 00:00.200678; el nuevo corte comienza aproximadamente 30 ms antes, en 00:00.170688.

En el OGG final se asignaron 175 transientes a los pulsos 1-175: 116,899093 BPM, offset 0,029786 s y residuo absoluto maximo 33,883 ms. **El residuo supera el limite de 30 ms**. Los picos atribuidos a los pulsos 121, 131, 149 y 162 presentan los cuatro mayores desajustes. El flujo espectral no separa instrumentos: algun candidato puede corresponder a otro transiente musical y no al bombo/caja. No ocultar ni ignorar esta advertencia; confirmar esos golpes a mano antes de fijar SongData.

Como contraste, se volvio a calcular el flujo espectral por bandas: 35-160 Hz (bombo) y 160-3500 Hz (caja/ataques medios-altos). La banda grave solo produjo 13 candidatos en todo el clip, insuficientes para representar cada pulso. La banda media-alta produjo 175 candidatos; su ajuste dio 116,897069 BPM, offset 0,030264 s y residuo maximo 34,42 ms (pulsos 121 y 149). El contraste tambien supera 30 ms; por tanto, el aviso no depende unicamente de la deteccion de banda completa.

El MP3 tiene un marcador `Info`, pero no se encontro una etiqueta `LAME` con encoder delay/padding. Por eso no se infirio una correccion del padding. El offset informado se midio sobre el OGG final decodificado. Falta escuchar el archivo y confirmar la convencion de signo que usa `RhythmManager`.

## Recorte y reproduccion en Audacity

44 compases a 116,898771 BPM duran 90,334568 s, el numero entero de compases cuya duracion queda mas cerca de 90 s. El OGG de prueba actualizado se guardo en `_scratch/MUS_BillieJean.ogg`, fuera de `Assets`, con fade lineal de 2 s.

Para repetirlo en Audacity:

1. Importar `Billie Jean.mp3`.
2. Seleccionar desde **00:00.171** hasta **01:30.505**.
3. Aplicar **Editar > Recortar audio > Recortar** (o `Ctrl+T`) para conservar la seleccion.
4. Aplicar **Efecto > Fundidos > Fundido de salida** a los ultimos 2 s: **01:28.505-01:30.505**.
5. Exportar como OGG Vorbis con el nombre `MUS_BillieJean.ogg`. Unity creara el `.meta` al importar el asset.
6. Escuchar el OGG y verificar el ataque inicial, el fade y los cuatro pulsos con residuo >30 ms antes de entregar los datos a Felipe.

El archivo de prueba y el entorno Python estan bajo `_scratch/`, excluido por `.gitignore`; no deben incluirse en el commit. No se pudo escuchar el audio durante este analisis.

## Mensaje para Felipe

> Felipe: actualice la medicion de `MUS_BillieJean.ogg`: **116,899 BPM** por regresion de largo alcance de onsets, `beatsPerBar = 4` (4/4), offset inicial **0,029786 s** (~30 ms). El MIDI marca 120 BPM, distinto al audio. El recorte es de **44 compases / 90,334568 s**, con fade de 2 s. Advertencia: el residuo maximo es **33,88 ms**, mayor que 30 ms (candidatos en pulsos 121, 131, 149 y 162); confirmar auditivamente antes de cargar en SongData.
