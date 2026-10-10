# 🌶️ PAPA VÓLEY: diseño refinado del minijuego musical

Centrarse en un solo minijuego es una decisión mucho mejor para un Game Jam de 24 horas. En lugar de tener tres minijuegos sencillos, se puede construir uno con personalidad, mecánicas interesantes y suficiente variedad para que la gente quiera volver a jugarlo.

La idea tiene potencial: una papa caliente que viaja entre dos jugadores al ritmo de la música, con una mecánica inspirada en Guitar Hero, rachas de aciertos y poderes para perjudicar al rival.

Pero el cambio importante es este: no diseñarlo como un Guitar Hero en el que simplemente pulsas una tecla cuando llega la papa. Convertirlo en un **duelo musical** donde cada golpe cambia el ritmo, la presión y las posibilidades de ambos jugadores.

**La fantasía del juego:** estás intentando deshacerte de una papa que está cada vez más caliente, pero tu rival puede cambiar las reglas, acelerar la música o devolvértela cuando menos te lo esperas.

---

## 1. La identidad del juego

# PAPA VÓLEY

*Un duelo musical de reflejos, rachas y sabotajes.*

Dos jugadores compiten en una cancha arcade. Una papa caliente viaja de un lado a otro siguiendo el ritmo de la música. Cuando llega a la zona de golpeo, el jugador debe pulsar el botón en el momento correcto para devolverla.

Cada acierto aumenta la intensidad. Las rachas permiten desbloquear poderes, y los errores hacen que la papa se caliente, se descontrole o termine explotando.

**Objetivo:** sobrevivir más que el rival y conseguir que sea él quien falle primero.

### Los pilares del diseño

- **Fácil de entender:** basta con observar la papa y pulsar un botón al ritmo.
- **Difícil de dominar:** la velocidad, los patrones y los poderes obligan a adaptarse.
- **Competitivo:** no juegas solo para conseguir puntos; intentas poner al rival en aprietos.
- **Impredecible, pero justo:** los poderes cambian las condiciones, pero ambos jugadores pueden entender qué está pasando y reaccionar.
- **Partidas cortas:** cada duelo debería durar aproximadamente entre 60 y 120 segundos, con posibilidad de revancha inmediata.

> Recomendación: que el juego se pueda explicar en diez segundos, pero que después de un minuto todavía puedan aparecer situaciones nuevas.

---

## 2. El ciclo principal de juego

La mecánica fundamental debe ser muy clara. Este es el ciclo que se repite durante la partida:

1. **La papa viaja por la línea musical.** Baja hacia la zona de golpeo siguiendo el tempo de la canción.
2. **El jugador pulsa en el momento correcto.** El juego mide la precisión temporal del golpe.
3. **La papa vuelve al rival.** Una animación de golpe la lanza hacia el otro lado, con un nuevo patrón de llegada.
4. **Las rachas desbloquean poderes.** El jugador puede modificar el próximo intercambio y complicarle la vida al rival.
5. **Alguien falla.** La papa explota, aparece una animación cómica y se declara al ganador.

La clave es que cada intercambio tenga una consecuencia: un acierto normal mantiene la partida; un acierto perfecto construye una racha; una racha habilita un poder; y un error puede cambiar inmediatamente el resultado.

---

## 3. La línea musical: cómo funcionaría realmente

Esta es la parte más importante de la implementación.

En lugar de mover una papa libremente por una cancha con físicas complejas, se utiliza una **línea musical vertical con una zona de impacto fija**, similar a un juego de ritmo. La papa baja por esa línea y el jugador pulsa el botón cuando cruza la zona objetivo.

### Vista conceptual del minijuego

```
Jugador 1
RACHA: 7
CALOR: 72%

PATRÓN MUSICAL
♪ · ♪ · ♫ · ♪

        🥔  PAPA

    ─── ZONA DE IMPACTO ───
  PERFECTO · BIEN · FALLASTE

PULSA ESPACIO
Golpea cuando la papa llegue a la zona
```

*Maqueta conceptual de la interfaz, no una captura del juego final.*

### Las reglas de precisión

El juego debe reconocer tres resultados:

| Resultado | Regla inicial sugerida         | Efecto                                                 |
| --------- | ------------------------------ | ------------------------------------------------------ |
| Perfecto  | Error de hasta ±70 ms          | Construye racha y carga poder.                         |
| Bien      | Error de hasta ±150 ms         | Devuelve la papa, pero no suma tanto.                  |
| Fallo     | Fuera de la ventana de acierto | La papa quema al jugador y pierde una vida o la ronda. |

Estos valores son puntos de partida, no reglas definitivas. Deben ajustarse con pruebas reales para que el juego se sienta justo y responda bien al teclado.

### ¿Cómo se conecta la papa con la música?

La música debe ser la referencia del juego, no un fondo que suena mientras la papa se mueve por su cuenta.

- La canción tiene un tempo definido en BPM (pulsos por minuto).
- La papa se programa para alcanzar la zona de impacto en un pulso determinado.
- El jugador puede anticipar los golpes escuchando la música.
- Al devolverla, se programa su siguiente llegada siguiendo la misma cuadrícula musical.
- Si se activa un poder que acelera la papa, se reduce el tiempo hasta el próximo golpe, sin desincronizar toda la canción.

Para el prototipo, empezar con una canción de tempo constante y patrones sencillos. No hace falta detectar automáticamente los beats de cualquier canción: se pueden definir manualmente los tiempos de los golpes.

> **Decisión técnica importante:** la música debe ser la fuente de referencia temporal. En Unity, conviene sincronizar los golpes con `AudioSettings.dspTime` y programar el audio con `AudioSource.PlayScheduled`, en lugar de depender exclusivamente de `Time.deltaTime`. Así se reduce el riesgo de que la papa y la música se desincronicen.

---

## 4. Sistema de rachas: que jugar bien tenga recompensa

Aquí es donde el juego puede empezar a ser mucho más que un clon de Guitar Hero.

Cada jugador tiene una racha individual. Si golpea correctamente varias veces seguidas, llena un medidor de energía y obtiene acceso a poderes.

### Medidor de racha

```
Jugador 1
7 / 8 aciertos
Un acierto perfecto más desbloquea un poder.
```

- **Racha de 3:** el medidor comienza a cargarse y la papa emite un brillo especial.
- **Racha de 5:** se desbloquea una habilidad menor o una carga adicional.
- **Racha de 8:** se habilita un poder especial más fuerte.

**Recomendación para la primera versión:** no implementar cinco niveles de poder desde el principio. Usar un único umbral: **cinco aciertos perfectos consecutivos para conseguir una carga de poder**. Es más fácil de programar, probar y explicar. Después, si queda tiempo, se puede añadir un segundo nivel.

### ¿Qué pasa si alguien falla?

- Se reinicia su racha.
- Si tenía un poder disponible, puede conservarlo, salvo que el poder sea parte de una habilidad defensiva ya consumida.
- El rival recibe una recompensa pequeña por el error: por ejemplo, un poco más de calor para la papa o una carga parcial de energía.

Así se evita que un solo fallo convierta toda la partida en una derrota inevitable.

---

## 5. El sistema de poderes: la parte más divertida

Los poderes no deben ser simples mejoras de puntuación. Deben cambiar las reglas del próximo intercambio durante unos segundos y crear decisiones interesantes.

Para evitar que sean difíciles de desarrollar, **todos los poderes se activan con la misma tecla y se consumen al usarlos**. Lo que cambia es su efecto.

### 1. ¡Papa Cohete! — Prioridad alta

**Efecto:** el próximo pase llega un 20–25 % más rápido.

La papa deja una estela de fuego y el rival dispone de menos tiempo para reaccionar. La música puede incorporar un efecto breve de aceleración, pero no necesita cambiar el tempo de la canción completa.

**Contrajuego:** el rival ve la animación de carga y sabe que el próximo pase será rápido.

### 2. Papa Eléctrica — Prioridad alta

**Efecto:** el próximo golpe del rival tiene una ventana de precisión reducida temporalmente.

La zona de impacto se ilumina y aparece un efecto eléctrico. Por ejemplo, la ventana de «Perfecto» se reduce de ±70 ms a ±50 ms durante un único golpe.

**Contrajuego:** el rival sigue teniendo una oportunidad clara de devolver la papa. No se trata de bloquear sus controles.

### 3. Papa Fantasma — Prioridad media

**Efecto:** la papa desaparece brevemente y reaparece cerca de la zona de impacto.

Crea un pequeño engaño visual. El rival debe mantener la atención en la zona de golpeo en vez de confiar únicamente en la posición anterior.

**Contrajuego:** una señal visual y sonora avisa del engaño. No debe convertirse en una trampa imposible de leer.

### 4. Papa Congelada — Prioridad media

**Efecto:** el próximo golpe cambia ligeramente su anticipación musical. La papa llega un poco antes o después del pulso esperado.

Se indica visualmente que la papa está congelada y se utiliza un efecto de sonido distinto.

**Contrajuego:** el cambio debe ser pequeño y coherente con el ritmo. Para el prototipo inicial, dejarlo como una variante de patrón y no como una alteración impredecible del tiempo.

### 5. Papa Dorada — Prioridad baja

**Efecto:** durante los siguientes dos golpes correctos, el jugador obtiene puntos extra y carga más rápido su siguiente poder.

No perjudica directamente al rival, pero recompensa jugar bien bajo presión.

**Contrajuego:** el rival puede intentar romper la racha antes de que se complete la bonificación.

### Cómo evitar que los poderes rompan el equilibrio

Cuatro reglas:

1. Solo se puede tener **una carga almacenada**.
2. Los poderes **no pueden cancelar el golpe del rival ni impedirle jugar**.
3. Cada poder debe tener una **señal visual clara antes de surtir efecto**.
4. **No se pueden encadenar poderes** sobre un mismo golpe.

> **Para el Game Jam:** implementar primero **Papa Cohete** y **Papa Dorada**. Son fáciles de entender, diferentes entre sí y permiten probar tanto el sabotaje como la recompensa por jugar bien. Después añadir Papa Eléctrica si el núcleo funciona.

---

## 6. Cómo evitar que la partida se vuelva repetitiva

Tener poderes no es suficiente. Si todas las secuencias musicales son iguales, después de 30 segundos el jugador ya habrá visto todo.

La variedad debería provenir de cuatro sistemas sencillos, combinados de forma controlada.

### A. Patrones musicales diferentes

No todas las llegadas de la papa deben ocurrir en cada pulso. Se pueden definir patrones que utilicen la misma línea, pero cambien la distribución de los golpes.

**Patrón 1: Ritmo constante** — ideal para aprender.

```
●  ●  ●  ●  ●  ●  ●  ●
```

**Patrón 2: Pausas inesperadas** — alterna golpes rápidos con pequeños silencios.

```
●  —  ●  ●  —  ●  —  ●
```

**Patrón 3: Doble golpe** — dos llegadas rápidas seguidas de una pausa.

```
●  ●  —  ●  ●  —  ●  ●
```

No hace falta un editor de mapas musicales. Se pueden guardar patrones como listas de tiempos relativos y reutilizarlos a distintas velocidades.

### B. Fases de intensidad

La partida puede durar unos 90 segundos, divididos en tres fases.

| Tiempo   | Fase             | Descripción                                                                                  |
| -------- | ---------------- | -------------------------------------------------------------------------------------------- |
| 0–30 s   | Calentamiento    | Velocidad moderada, patrones simples y oportunidad de conseguir la primera racha.            |
| 30–60 s  | La papa arde     | Se introducen pausas, dobles golpes y poderes más frecuentes.                                |
| 60–90 s  | Modo infierno    | Ritmo rápido, menos margen para errores y efectos visuales más intensos, sin hacer imposible la canción. |

Los tiempos son orientativos. La partida debe terminar antes si alguien pierde, y las fases deben adaptarse a la duración real de la canción.

### C. Eventos especiales

Cada 15–20 segundos puede aparecer un evento anunciado con una señal visual y sonora:

- **Silencio incómodo:** desaparecen algunos instrumentos, pero la referencia del ritmo continúa.
- **Fiebre de papa:** aumenta la velocidad durante unos pocos golpes.
- **Doble o nada:** un golpe perfecto otorga una carga adicional; fallar tiene una penalización mayor.
- **Intercambio loco:** ambos jugadores intercambian sus colores o efectos visuales durante unos segundos.

No implementar todos en la primera versión. Dos eventos bien pulidos son mejores que seis que apenas se entienden.

### D. Variabilidad en las decisiones

La variedad más interesante no viene de poner cada vez más obstáculos. Viene de que el jugador tenga decisiones reales:

- ¿Uso mi poder ahora o espero a una secuencia difícil?
- ¿Intento conseguir una racha más larga para ganar una carga mejor?
- ¿Aprovecho que el rival está cerca de fallar para acelerar la papa?
- ¿Me concentro en acertar perfectamente o me conformo con devolverla?

El jugador debe sentir que está jugando contra una persona, no solamente contra un cronómetro.

---

## 7. ¿Cómo se gana? El sistema de victoria

Para evitar que el juego se vuelva demasiado largo, la condición principal es **fallar un golpe**: la papa explota en las manos del jugador que no consigue devolverla a tiempo.

Pero hay que evitar que los poderes conviertan la partida en algo injusto. Por eso:

- Un golpe fallido termina la partida.
- Los aciertos normales permiten sobrevivir.
- Los aciertos perfectos cargan poderes.
- Los poderes modifican el siguiente intercambio, pero siempre dejan una posibilidad razonable de responder.
- El jugador que usa un poder no obtiene automáticamente la victoria: debe seguir acertando sus propios golpes.

Para las primeras pruebas, partidas **al mejor de tres rondas**. Así un solo error no elimina toda la diversión de una sesión, pero cada ronda sigue teniendo tensión.

### ¿Y si queremos una partida más larga?

Se puede añadir un sistema de vidas: cada jugador empieza con tres, pierde una por fallo y la partida termina cuando alguien pierde las tres. Sin embargo, esto requiere más tiempo para equilibrar las vidas, los poderes y la duración. **No incluirlo en el primer prototipo.**

---

## 8. La interfaz que necesita el juego

La pantalla debe permitir entender el estado de la partida de un vistazo.

```
JUGADOR 1                 PAPA VÓLEY                 JUGADOR 2
Racha: 7               RONDA 2 · 01:12               Racha: 4

                 ¡LA PAPA ESTÁ ARDIENDO!
                 Siguiente golpe: RÁPIDO

                      CALOR 72%

     ESPACIO: Golpear          SHIFT: Usar poder
```

*Ejemplo conceptual de distribución de la interfaz.*

### Elementos imprescindibles

- La papa y la zona de impacto.
- Indicador del jugador activo.
- Medidor de racha y de poder.
- Medidor de calor.
- Avisos de eventos y poderes.
- Resultado del golpe: perfecto, bien o fallo.
- Pantalla de victoria con opción de revancha.

### Controles

Si van a jugar en un mismo teclado, asignar una tecla de golpe a cada jugador, por ejemplo `F` para el jugador 1 y `J` para el jugador 2. La tecla para usar poderes también debe ser diferente para cada uno.

Esto es importante porque si ambos comparten una sola tecla, la competición pierde sentido: uno podría activar accidentalmente la acción del otro.

---

## 9. Cómo organizar el código en Unity

Para que el proyecto no termine convertido en un único script inmanejable, dividirlo en componentes pequeños.

| Script                | Responsabilidad                                               |
| --------------------- | ------------------------------------------------------------- |
| `RhythmManager.cs`    | Tempo, pulsos musicales, tiempos programados y patrones.      |
| `PotatoController.cs` | Movimiento de la papa y llegada a la zona de impacto.         |
| `HitDetection.cs`     | Detectar la pulsación, calcular precisión y registrar fallos. |
| `ComboSystem.cs`      | Rachas, multiplicadores y cargas de poder.                    |
| `PowerUpManager.cs`   | Activación, duración y efectos de poderes.                    |
| `PotatoHeatSystem.cs` | Nivel de calor, apariencia y escalada de tensión.             |
| `MatchManager.cs`     | Estado de partida, turnos de golpe, vidas y victoria.         |
| `GameUI.cs`           | Marcadores, medidores, notificaciones y pantalla final.       |

No hace falta construir todos estos sistemas como una arquitectura compleja. Pueden ser componentes sencillos que se comuniquen mediante métodos y eventos de C#.

### La lógica del golpe

1. `RhythmManager` programa cuándo debe llegar la papa.
2. `PotatoController` anima su recorrido hasta la zona de golpeo.
3. `HitDetection` compara el tiempo real de la pulsación con el tiempo objetivo.
4. `ComboSystem` actualiza la racha.
5. `PowerUpManager` aplica la habilidad seleccionada.
6. `MatchManager` determina quién debe jugar a continuación o si alguien ha perdido.

Para evitar errores, **cada llegada debe resolverse una sola vez**. Si el jugador pulsa dos veces, el juego no debe registrar dos golpes para una misma papa.

También conviene **separar el movimiento visual de la lógica temporal**: la posición de la papa sirve para mostrar lo que sucede, pero el acierto se calcula con los tiempos programados. Así se evita que pequeños retrasos de los fotogramas alteren injustamente el resultado.

---

## 10. El alcance realista para 24 horas

No empezar implementando cinco poderes, cuatro eventos y tres fases. Primero hay que demostrar que el golpe musical es divertido.

### Esencial — Primera versión jugable

- Dos jugadores locales.
- Una canción y una línea musical.
- Una zona de golpeo con detección de precisión.
- Rachas y condición de derrota.
- Papa que viaja de un jugador al otro.
- Una pantalla final con revancha.

### Prioridad 2 — Lo que le da personalidad

- Dos poderes: Papa Cohete y Papa Dorada.
- Tres patrones musicales.
- Animación de calentamiento y explosión.
- Sonidos de acierto y fallo.

### Si sobra — Pulido y contenido extra

- Un tercer poder.
- Dos eventos especiales.
- Animaciones adicionales y efectos de cámara.
- Segunda canción o selección de dificultad.

### Distribución sugerida del tiempo

| Tiempo  | Objetivo                                              |
| ------- | ----------------------------------------------------- |
| 0–3 h   | Proyecto Unity, controles y sincronización básica.    |
| 3–7 h   | Línea musical, movimiento y detección de golpes.      |
| 7–10 h  | Dos jugadores y condición de victoria.                |
| 10–14 h | Rachas, medidores y dos poderes.                      |
| 14–18 h | Patrones musicales y escalada de dificultad.          |
| 18–21 h | Sonido, animaciones, interfaz y feedback visual.      |
| 21–24 h | Pruebas, equilibrio, correcciones y ejecutable final. |

Este cronograma supone que los integrantes pueden trabajar en paralelo y que ya tienen una canción adecuada o pueden crear una pista sencilla. Si están aprendiendo Unity desde cero, reducir el alcance a **una canción, un poder y un modo de juego completo**.

---

## 11. Las tres pruebas antes de darlo por terminado

No basta con que el juego funcione técnicamente. Hay que comprobar que sea divertido.

1. **¿Se entiende sin explicación?** Poner a dos personas a jugar sin explicarles las reglas. Si no entienden cuándo golpear o por qué fallaron, la interfaz necesita mejorar.
2. **¿Se siente justo?** Comprobar que un jugador no pierda constantemente por retrasos de audio, patrones imposibles o poderes que no dejan responder. Un error debe sentirse como un error del jugador, no del juego.
3. **¿Piden una revancha?** Esta es la prueba más importante. Si después de una partida los dos jugadores quieren volver a intentarlo, están construyendo algo interesante. Si se aburren después de una sola ronda, necesitan más variedad en los patrones, los poderes o las decisiones.

---

## Propuesta definitiva

Construir Papa Vóley como un duelo musical de dos jugadores, con una sola línea de golpeo, una papa que viaja siguiendo el tempo, rachas que desbloquean poderes y una dificultad que escala a lo largo de la partida.

La identidad no debe depender de tener muchísimas mecánicas, sino de combinar bien estas cinco:

1. Ritmo musical preciso.
2. Una papa visualmente expresiva.
3. Rachas que recompensan la habilidad.
4. Poderes que generan interacción entre rivales.
5. Una escalada de tensión que culmina en un fallo divertido.

Y una decisión desde el principio: **no hacer que la velocidad aumente indefinidamente sin control**. Definir un límite de dificultad y variar los patrones, porque la sorpresa y la anticipación son más divertidas que simplemente pulsar cada vez más rápido.

Con eso hay una base concreta para desarrollar, probar y presentar un minijuego completo en 24 horas, sin depender de físicas complejas ni de contenido que no alcance a terminarse.
