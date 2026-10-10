# 🎮 Guía de Integración de Assets en Unity — `feature/asset-integration-unity`

> Branch de prueba para conectar todos los assets visuales y de audio al proyecto Unity.
> Base: `develop` (commit `6ad0e87`)

---

## 📦 Assets disponibles en esta branch

### 🎨 Sprites — `Assets/Art/Sprites/`

| Carpeta        | Archivos                                                                                                                              |
|----------------|---------------------------------------------------------------------------------------------------------------------------------------|
| `Characters/`  | Potato (Idle x4, Happy x4, Worried x4, Defeated, Freeze, Sleep), Boy (Idle, Run, Walk, Attack, Cheer, Fight, Guitar, Peace), Girl (ídem), Fries_Idle |
| `Props/`       | Amp, Banner, Cable, Chair, Cooler, Crates, DrumKit, Guitars, Keyboard, LightTruss, MicStand, Monitor, PaperBall, Stars, TrafficCone, TrashCan, WaterCrate |
| `UI/`          | Flechas (color + blanco + catcher) ↑↓←→, botones verde/naranja, partícula glow, palabras de feedback (Perfect, Great, Good, Ok, Bad, Miss), Logotipo Beat Potato |

### 🔤 Fuentes — `Assets/Art/Fonts/`
- `PressStart2P-Regular.ttf` (pixel art, licencia OFL)

### 🎵 Audio — `Assets/Audio/`
| Carpeta   | Archivos                                                                 |
|-----------|--------------------------------------------------------------------------|
| `Menu/`   | SFX_UI_Cancel, CloseMenu, Confirm, Equip, Exit, MenuSelections, OpenMenu, Pause, Resume, Saved, Shop, Unequip (.ogg) |
| Raíz      | `Billie Jean.mp3`, `Michael Jackson - Billie Jean.mid`                   |

---

## ✅ Checklist de integración en Unity

### 1. Sprites de la Papa (personaje principal)
- [ ] Importar `Potato_Idle_01..04` → crear animación **Potato_Idle** (Animator)
- [ ] Importar `Potato_Happy_01..04` → crear animación **Potato_Happy**
- [ ] Importar `Potato_Worried_01..04` → crear animación **Potato_Worried**
- [ ] Importar `Potato_Defeated_01`, `Potato_Freeze_01`, `Potato_Sleep_01` → animaciones de estado
- [ ] Asignar **Animator Controller** al prefab de la papa
- [ ] Conectar transiciones con eventos de `GameEvents.cs` (`OnPlayerExploded`, `OnHeatChanged`)

### 2. Sprites de Personajes (Boy / Girl)
- [ ] Importar sprites de Boy y Girl
- [ ] Crear animaciones: Idle, Run, Walk, Attack, Cheer, Fight, Guitar, Peace
- [ ] Conectar animator con `PlayerInput.cs` o `PlayerInputController.cs`

### 3. Sprites de UI (flechas del ritmo)
- [ ] Asignar `arrow_colors_↑↓←→` al **NoteObject prefab** (`Assets/Prefab/Note_Prefab.prefab`)
- [ ] Asignar `arrowcatcher_↑↓←→` a las zonas de hit en la escena
- [ ] Configurar `arrow_white_↑↓←→` para el estado "inactivo" de la flecha
- [ ] Colocar palabras de feedback (Perfect/Great/Good/Ok/Bad/Miss) en el **HUD** — conectar con `RhythmManager.cs` evento de evaluación

### 4. Props del escenario
- [ ] Colocar props en la escena `SampleScene.unity` como decoración de fondo
- [ ] Recomendados para el escenario musical: DrumKit, Amp, Guitar, Keyboard, MicStand, LightTruss, Banner

### 5. Fuente Press Start 2P
- [ ] Crear **Font Asset** en Unity (Window → TextMeshPro → Font Asset Creator)
- [ ] Aplicar a todos los textos de UI (score, combo, feedback, título)

### 6. Audio — Menú
- [ ] Crear `AudioSource` en el objeto de menú
- [ ] Asignar `SFX_UI_Confirm` al botón Play
- [ ] Asignar `SFX_UI_Cancel` al botón Back
- [ ] Asignar `SFX_UI_Pause/Resume` al botón de pausa
- [ ] Conectar con sistema de eventos de `GameEvents.cs`

### 7. Audio — Música del minijuego
- [ ] Importar `Billie Jean.mp3` como `AudioClip` (Streaming recomendado)
- [ ] Asignar al `RhythmManager.cs` (campo `musicClip`)
- [ ] Verificar BPM=117.0, offset=0.245s (ver `Docs/T08_Cancion_BillieJean.md`)
- [ ] Sincronizar spawneo de notas con el BPM del audio

### 8. Sistema Visual de Hot Potato (Tira y Afloja)
- [ ] Conectar `HotPotatoPressureMeter.cs` + `PressureMeterUI.cs` con sprites de papa
- [ ] Visual de papa → cambiar sprite según nivel de heat (`PotatoHeatSystem.cs`)
  - Heat < 30% → `Potato_Idle`
  - Heat 30–70% → `Potato_Worried`
  - Heat > 70% → animación urgente (shake o parpadeo)
  - Heat 100% → `Potato_Defeated` + explosión

---

## 🗂️ Estructura de carpetas Unity recomendada

```
Assets/
├── Art/
│   ├── Animations/      ← Animator Controllers y Animation Clips aquí
│   ├── Fonts/           ✅ PressStart2P (ya está)
│   └── Sprites/
│       ├── Characters/  ✅ Boy, Girl, Potato, Fries (ya están)
│       ├── Props/        ✅ (ya están)
│       └── UI/           ✅ Flechas, feedback words, logo (ya están)
├── Audio/
│   ├── Menu/             ✅ SFX (ya están)
│   └── Billie Jean.mp3   ✅ (ya está)
├── Prefab/
│   └── Note_Prefab.prefab  ← asignar sprite de flecha aquí
├── Scenes/
│   └── SampleScene.unity   ← escena principal de prueba
└── Scripts/
    ├── Core/               ✅ GameConfig, GameEnums, GameEvents, MatchManager
    ├── Gameplay/           ✅ AIController, ComboSystem, PotatoHeatSystem, PowerUpManager
    ├── Minigame/           ✅ RhythmManager, NoteSpawner, PlayerInput, BombController, PressureMeterUI
    └── Rhythm/             ✅ NoteObject, NoteSpawner, PlayerInput, RhythmManager
```

---

## ⚠️ Notas importantes

- Los sprites de Characters están como **PNG individuales** (no sprite sheets cortadas). Hay que configurar el **Sprite Mode** en Unity.
- `Hoja de sprites pixel art de patatas encantadoras.png` y `Héroes Pixel en Acción.png` son referencias de arte — revisar si se usan o si se sustituyen por los PNGs individuales.
- `Billie Jean.mp3` y el `.mid` son la misma canción. Usar el `.mp3` para playback en Unity.
- Hay scripts duplicados entre `Scripts/Minigame/` y `Scripts/Rhythm/` — coordinar con el equipo cuál usar en la escena.

---

*Branch creada el 2026-10-10 desde `develop`. Para mergear de vuelta a develop, hacer PR con el checklist completo.*
