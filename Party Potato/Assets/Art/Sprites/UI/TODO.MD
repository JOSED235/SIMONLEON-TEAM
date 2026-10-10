# Instrucciones de recoloración de assets

## Objetivo
Recolorear los sprites de flechas y planetas a la paleta oficial del proyecto usando **Unity**.

## Paleta cromática (hex)
- Fucsia → #FF2D95
- Rojo salmón → #FF6F61
- Morado → #7B2CBF
- Magenta → #EB2CA0
- Verde aguamarina → #00E5C4
- Negro → #0A0A0A
- Blanco → #FFFFFF

## Pasos en Unity
1. Importar los PNG en `Assets/Art/Sprites/`.
2. Crear un **Material** por cada color de la paleta.
   - Shader recomendado: `Sprite/Default`.
   - Ajustar el campo **Tint** al color correspondiente.
3. Asignar el Material a cada sprite según dirección o estado:
   - Flecha arriba → Fucsia
   - Flecha abajo → Rojo salmón
   - Flecha izquierda → Morado
   - Flecha derecha → Verde aguamarina
   - Planeta frío → Verde aguamarina
   - Planeta caliente → Rojo salmón
   - Planeta ardiendo → Fucsia/Morado con efectos.
4. Para animaciones de onda:
   - Crear un prefab con el sprite base.
   - Añadir un Animator que escale y desvanezca el Material Tint.
   - Guardar en `Assets/Art/Animations/Effects`.

## Entregables
- Prefabs recoloreados en `Assets/Prefabs/`.
- Sprite sheets actualizados en `Assets/Art/Sprites/`.
- Animaciones de onda en `Assets/Art/Animations/Effects/`.

---
**Nota:** Mantener los contornos negros o blancos según contraste para conservar el estilo retro‑cómic.
