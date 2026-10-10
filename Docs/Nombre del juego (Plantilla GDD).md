# Beat Potato (Papa Beat) - Game Design Document (GDD)

<aside>

**Pitch de elevador:**  
Beat Potato es un duelo de ritmo arcade para dos jugadores en un mismo PC (con opcion individual contra la maquina) donde los rivales pulsan notas en cuatro carriles al compas de la musica. Mantener el ritmo y encadenar rachas permite desatar poderes de sabotaje y empujar una papa explosiva hacia el campo contrario antes de que se sobrecaliente y estalle.

</aside>

## **Objetivo del juego:**

Sobrevivir al intercambio ritmico manteniendo rachas de aciertos para traspasar la papa al oponente hasta que se sobrecaliente al 100% y explote en su campo.

## Pilares y valores de diseno:

### **Pilares de diseno:**

- **Ritmo Competitivo Inmediato:** Mecanica de cuatro carriles clara y reconocible al instante, donde la precision temporal y el oido musical determinan el exito de cada intercambio.
- **Tension Fisica de Traspaso:** La papa viaja de un lado a otro como una amenaza constante visible en pantalla; acertar aleja el peligro hacia el rival y fallar retiene la amenaza en el lado propio.
- **Interaccion y Sabotaje por Rachas:** Recompensar la consistencia con poderes especiales que alteran la pantalla del oponente (velocidad de notas, notas invisibles), generando dinamismo y remontadas.
- **Accesibilidad Compartida en un Mismo Teclado:** Jugabilidad fluida para dos personas en una sola computadora sin conflicto de teclas, acompanada de modo solitario contra una IA adaptable.

### **Valores de diseno:**

- **Justicia y Claridad Ritmica:** La musica es la fuente de verdad temporal absoluta; el acierto se juzga por el compas y no por fluctuaciones de rendimiento grafico.
- **Diversion Frenetica Arcade:** Partidas cortas, tensas y emocionantes de 60 a 90 segundos que inviten de inmediato a la revancha.

## Referentes:

### **Gameplay:**

**Friday Night Funkin' / Dance Dance Revolution**

Toma el sistema de notas direccionales en cuatro carriles (Izquierda, Abajo, Arriba, Derecha) que caen hacia una zona de impacto fija, con clasificacion de precision temporal (Perfect, Great, Good, Miss) y acumulacion de rachas. Adicionalmente, incorpora la dinamica de eliminacion y tension de los juegos de papa caliente arcade.

### **Visuales:**

**Estetica Arcade 2D y Rhythm Games Modernos**

Interfaz dividida en dos paneles simetricos con lineas de carriles nitidas, efectos visuales de chispas en las zonas de impacto, textos flotantes de feedback con tipografia llamativa y una papa central que cambia visiblemente de color amarillo a rojo incandescente al calentarse.

### **Experiencia o fantasia:**

**Duelo de Salón de Fiestas y Papa Caliente**

La sensacion de estar en una competencia frenetica donde un solo descuido puede hacer que el objeto peligroso te queme las manos, obligando a mantener la concentracion bajo la presion del rival.

## **Mecanicas del jugador:**

- **Pulsacion Direccional en Carriles:**
  - Jugador 1: Flechas del teclado (Izquierda, Abajo, Arriba, Derecha).
  - Jugador 2: Teclas WASD (A: Izquierda, S: Abajo, W: Arriba, D: Derecha).
- **Activacion de Poder Especial:** Presionar la tecla asignada (Enter para Jugador 1, Barra Espaciadora para Jugador 2) al alcanzar el umbral de racha requerida.
- **Seleccion de Modo y Dificultad:** Elegir entre Duelo Local (dos jugadores) o Partida Solitaria contra la maquina (Facil, Normal, Dificil).

## **Mecanicas del juego:**

- **Sincronizacion Temporal por Audio (dspTime):** Calculo de tiempos exactos y beats mediante el reloj interno de audio para garantizar precision constante independientemente de los cuadros por segundo.
- **Generacion Continua de Notas (NoteSpawner):** Instanciacion anticipada de notas que caen hacia las zonas de impacto a la velocidad programada.
- **Sistema de Calificacion de Impacto (Hit Detection):**
  - Perfect: Margen de +-50 ms (suma racha, carga poder, enfria la papa y la impulsa con fuerza al rival).
  - Great: Margen de +-100 ms (suma racha e impulsa la papa).
  - Good: Margen de +-150 ms (mantiene racha y devuelve la papa sin impulso extra).
  - Miss: Fuera de margen o nota rebasada (reinicia racha a cero, frena la papa en el lado del jugador y aumenta el calor en +20%).
- **Traspaso y Estado Termico de la Papa:** La papa oscila rítmicamente en el centro; los aciertos la empujan hacia el rival y los fallos acumulan calor (de 0% a 100%) hasta provocar la detonacion.
- **Inteligencia Artificial para Modo Solo:** Simulacion automatizada de respuestas de la CPU ajustando la tasa de acierto y dispersion de timing segun la dificultad escogida.
- **Condicion de Victoria:** Detonacion de la papa en el campo del jugador que alcance el 100% de calor, otorgando la ronda al adversario.

## **Imagenes del proceso:**

![Proceso 1](images.jpg)

![Proceso 2](images%201.jpg)

![Proceso 3](images%202.jpg)

---

## Datos tecnicos:

**Desarrollado por:**  
Equipo Simeones (Jose David Rodriguez Pinilla, Felipe Calderon, Faiber Piedrahita, Maria Juliana Marin Shek, Ivan Andres Quintero)

**Duracion estimada del desarrollo total:**  
24 horas (Game Jam ICESI)

**Motor y Tecnologias:**  
Unity 2D, C#, Universal Render Pipeline (URP), sincronizacion de audio dspTime

---

## Preguntas de Playtest:

### Clasificacion (Understand - Measure - Behaviours - Opinions):

1. **Understand (Comprension):** ¿Los jugadores entienden en los primeros 10 segundos que deben presionar las teclas cuando las notas llegan a la zona de impacto inferior?
2. **Measure (Medicion):** ¿Cual es el promedio de rachas alcanzadas por los jugadores y con que frecuencia se completan las cargas de poder?
3. **Behaviours (Comportamientos):** ¿Los jugadores reaccionan visualmente a la cercania y calor de la papa, o se concentran exclusivamente en sus propios carriles?
4. **Opinions (Opiniones):** ¿El ritmo de la musica se siente sincronizado con la pulsacion de las teclas? ¿La partida resulta divertida y genera deseo inmediato de revancha?

### Mecanismo(s) elegidos:

- Observacion directa de sesiones 1v1 sin explicacion previa de reglas para medir comprension natural.
- Cuestionario breve post-partida enfocado en sensacion de justicia de los controles y grado de diversion.
- Registro cuantitativo de porcentaje de aciertos (Perfect/Great/Good/Miss) por sesion.