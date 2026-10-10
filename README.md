# Beat Potato (Papa Beat)

Duelo de ritmo arcade 1v1 y modo individual contra la maquina (CPU), desarrollado en Unity 2D (C#) para la Game Jam ICESI.

---

## Descripcion

Beat Potato combina las mecanicas de 4 carriles de Friday Night Funkin' (FNF) con la dinamica eliminatoria de la papa caliente. Dos jugadores compiten en una misma computadora (o un jugador contra la maquina) pulsando notas al compas de la musica. 

Los aciertos (Perfect, Great, Good) mantienen la papa en juego, la empujan hacia el campo rival y construyen rachas de combo para desbloquear poderes de sabotaje. Los fallos (Miss) retienen la papa en el campo del jugador que erro, aumentando su temperatura hasta que estalla y define al ganador.

---

## Modos de Juego

1. **Duelo Local (1 vs 1 en el mismo PC):** Dos jugadores comparten teclado y pantalla compitiendo cara a cara.
2. **Modo Solo (1 Jugador vs CPU):** Partida individual contra un agente de inteligencia artificial con tres niveles de dificultad (Facil, Normal, Dificil).

---

## Controles (Mismo Teclado)

| Accion | Jugador 1 | Jugador 2 | Modo vs CPU |
| :--- | :--- | :--- | :--- |
| Carril Izquierdo (Lane 0) | Flecha Izquierda | Tecla A | Humano usa su configuracion |
| Carril Abajo (Lane 1) | Flecha Abajo | Tecla S | Humano usa su configuracion |
| Carril Arriba (Lane 2) | Flecha Arriba | Tecla W | Humano usa su configuracion |
| Carril Derecha (Lane 3) | Flecha Derecha | Tecla D | Humano usa su configuracion |
| Poder Especial | Enter / Shift Derecho | Barra Espaciadora / Shift Izquierdo | CPU lo activa por racha |

---

## Mecanicas Principales

- **Motor Ritmico de Alta Precision:** Sincronizacion absoluta con AudioSettings.dspTime y seguimiento exacto de beats por minuto (BPM).
- **Sistema de Calificaciones (Hit Ratings):**
  - Perfect: Margen de +-50 ms. Maxima velocidad de rechazo al rival, suma racha y enfria la papa.
  - Great: Margen de +-100 ms. Desplaza la papa hacia el rival y suma racha.
  - Good: Margen de +-150 ms. Mantiene la racha sin empuje adicional.
  - Miss: Fuera de margen o nota omitida. Reinicia la racha a cero, frena la papa y eleva el calor (+20%).
- **Traspaso de la Papa y Calor Critico:** La papa oscila entre ambos lados; acumular fallos eleva el medidor termico al 100%, detonando la papa y finalizando la ronda.
- **Poderes Especiales por Rachas:** Conseguir 5 aciertos seguidos otorga una habilidad de sabotaje (Papa Cohete para acelerar notas del rival, Papa Fantasma para ocultar notas a mitad de carril, etc.).

---

## Estructura del Proyecto

```
Party Potato/Assets/Scripts/
  Core/        GameEvents, GameEnums, GameConfig, MatchManager
  Rhythm/      RhythmManager, NoteSpawner, NoteObject, PlayerInput
  Gameplay/    BombController, PotatoHeatSystem, ComboSystem, PowerUpManager, AIController
  UI/          GameUI, HitFeedbackUI, VictoryScreen
  Audio/       SfxPlayer
```

---

## Equipo de Desarrollo (Simeones)

- Jose David Rodriguez Pinilla
- Felipe Calderon
- Faiber Piedrahita
- Maria Juliana Marin Shek
- Ivan Andres Quintero
