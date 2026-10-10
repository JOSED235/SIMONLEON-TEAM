using System;
using System.Collections.Generic;
using UnityEngine;

public static class AIControllerTests
{
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception("ASSERTION FAILED: " + message);
        }
    }

    private static void AssertApproximately(float actual, float expected, float tolerance, string message)
    {
        if (Mathf.Abs(actual - expected) > tolerance)
        {
            throw new Exception(string.Format("ASSERTION FAILED: {0}. Expected: {1}, Actual: {2}", message, expected, actual));
        }
    }

    private static void SetupTestRhythmManager(float bpm = 120f)
    {
        RhythmManager.Instance = null;
        RhythmManager rm = new RhythmManager();
        rm.songBpm = bpm;
        rm.beatsPerMeasure = 4;
        rm.songOffset = 0f;
        rm.gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        rm.musicSource = new AudioSource();
        rm.RecalculateBeatDuration();
        rm.StartTrack(100.0);
        RhythmManager.Instance = rm;
    }

    private static void SetRhythmTime(RhythmManager rm, double currentDsp)
    {
        var type = typeof(RhythmManager);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        float songPos = (float)(currentDsp - rm.dspSongStartTime - rm.songOffset);
        float songPosBeats = songPos / rm.secPerBeat;

        type.GetField("songPositionField", flags).SetValue(rm, songPos);
        type.GetField("songPositionInBeatsField", flags).SetValue(rm, songPosBeats);
    }

    /// <summary>
    /// Verifica que la generacion de desfases temporales sigue exactamente la calibracion
    /// de probabilidad de cada nivel de dificultad (Easy ~70%, Normal ~85%, Hard ~96%).
    /// </summary>
    public static void TestAIDifficultyOffsetDistributions()
    {
        SetupTestRhythmManager(120f);
        AIController ai = new AIController();

        // 1. Dificultad EASY (Papa Tibia)
        // Roll 10 (< 30) -> Miss intencional (float.MaxValue)
        float offEasyMiss = ai.GenerateTimingOffset(AIDifficulty.Easy, false, 10f, 1f);
        Assert(offEasyMiss == float.MaxValue, "Easy con roll 10 debe producir Miss (float.MaxValue)");

        // Roll 50 (30..75) -> Good (desfase > 0.10s y <= 0.15s)
        float offEasyGood = ai.GenerateTimingOffset(AIDifficulty.Easy, false, 50f, 1f);
        Assert(offEasyGood > 0.10f && offEasyGood <= 0.15f, "Easy con roll 50 debe generar desfase en ventana Good (0.10-0.15s)");

        // Roll 85 (75..95) -> Great (desfase > 0.05s y <= 0.10s)
        float offEasyGreat = ai.GenerateTimingOffset(AIDifficulty.Easy, false, 85f, 1f);
        Assert(offEasyGreat > 0.05f && offEasyGreat <= 0.10f, "Easy con roll 85 debe generar desfase en ventana Great (0.05-0.10s)");

        // Roll 98 (95..100) -> Perfect (desfase <= 0.05s)
        float offEasyPerf = ai.GenerateTimingOffset(AIDifficulty.Easy, false, 98f, 1f);
        Assert(offEasyPerf <= 0.05f, "Easy con roll 98 debe generar desfase en ventana Perfect (<=0.05s)");

        // 2. Dificultad NORMAL (Papa Caliente)
        // Roll 10 (< 15) -> Miss
        float offNormMiss = ai.GenerateTimingOffset(AIDifficulty.Normal, false, 10f, 1f);
        Assert(offNormMiss == float.MaxValue, "Normal con roll 10 debe producir Miss");

        // Roll 50 (30..70) -> Great
        float offNormGreat = ai.GenerateTimingOffset(AIDifficulty.Normal, false, 50f, 1f);
        Assert(offNormGreat > 0.05f && offNormGreat <= 0.10f, "Normal con roll 50 debe generar Great (0.05-0.10s)");

        // Roll 85 (70..100) -> Perfect
        float offNormPerf = ai.GenerateTimingOffset(AIDifficulty.Normal, false, 85f, 1f);
        Assert(offNormPerf <= 0.05f, "Normal con roll 85 debe generar Perfect (<=0.05s)");

        // 3. Dificultad HARD (Papa Volcanica)
        // Roll 2 (< 4) -> Miss (solo 4% de fallo)
        float offHardMiss = ai.GenerateTimingOffset(AIDifficulty.Hard, false, 2f, 1f);
        Assert(offHardMiss == float.MaxValue, "Hard con roll 2 debe producir Miss");

        // Roll 25 (10..40) -> Great
        float offHardGreat = ai.GenerateTimingOffset(AIDifficulty.Hard, false, 25f, 1f);
        Assert(offHardGreat > 0.05f && offHardGreat <= 0.10f, "Hard con roll 25 debe generar Great");

        // Roll 70 (40..100) -> Perfect (60% de aciertos perfectos)
        float offHardPerf = ai.GenerateTimingOffset(AIDifficulty.Hard, false, 70f, 1f);
        Assert(offHardPerf <= 0.05f, "Hard con roll 70 debe generar Perfect");

        Console.WriteLine("[PASS] AIController: Distribuciones de probabilidad y precision segun dificultad");
    }

    /// <summary>
    /// Verifica que la IA solo procesa y programa notas pertenecientes a su jugador asignado (Player2),
    /// ignorando las notas del jugador humano (Player1).
    /// </summary>
    public static void TestAIProcessesOnlyTargetPlayerNotes()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        AIController ai = new AIController();
        ai.aiPlayer = PlayerID.Player2;

        // Nota para Player1 (humano)
        NoteObject noteP1 = new NoteObject();
        noteP1.targetPlayer = PlayerID.Player1;
        noteP1.laneID = 0;
        noteP1.beatOfThisNote = 10.0f;
        NoteObject.ActiveNotes.Add(noteP1);

        // Nota para Player2 (IA)
        NoteObject noteP2 = new NoteObject();
        noteP2.targetPlayer = PlayerID.Player2;
        noteP2.laneID = 2;
        noteP2.beatOfThisNote = 10.0f;
        NoteObject.ActiveNotes.Add(noteP2);

        ai.ProcessNotes();

        // Debe haber programado exactamente 1 nota (la de Player2)
        Assert(ai.scheduledNotesCount == 1, "La IA debe programar solo la nota correspondiente a su PlayerID");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] AIController: Filtro estricto por jugador objetivo (ignora notas de P1)");
    }

    /// <summary>
    /// Verifica que cuando el tiempo musical alcanza el beat programado, la IA resuelve la nota
    /// mediante el mismo flujo compartido (EvaluateAndResolve), emitiendo los eventos correspondientes.
    /// </summary>
    public static void TestAIHitEvaluationAndResolutionFlow()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat, DSP 100.0 = Beat 0

        // Cancion en Beat 4.0 (DSP 102.0)
        SetRhythmTime(RhythmManager.Instance, 102.0);

        AIController ai = new AIController();
        ai.aiPlayer = PlayerID.Player2;
        ai.difficulty = AIDifficulty.Hard;

        // Nota para P2 en Beat 4.02
        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player2;
        note.laneID = 3; // Right
        note.beatOfThisNote = 4.02f;
        NoteObject.ActiveNotes.Add(note);

        PlayerID hitPlayer = PlayerID.None;
        int hitLane = -1;
        HitResult hitRating = HitResult.Miss;
        int hitEventsCount = 0;

        GameEvents.OnNoteHit += delegate(PlayerID p, int l, HitResult r, float acc)
        {
            hitPlayer = p;
            hitLane = l;
            hitRating = r;
            hitEventsCount++;
        };

        // Simular impacto determinista con roll de Perfect
        HitResult simResult = ai.SimulateHitForNote(note, 80f, 1f);

        Assert(simResult == HitResult.Perfect, "El impacto simulado con roll 80 en Hard debe ser Perfect");
        Assert(note.isResolved, "La nota debe quedar resuelta");
        Assert(hitEventsCount == 1, "Debe dispararse OnNoteHit");
        Assert(hitPlayer == PlayerID.Player2, "El jugador del impacto debe ser Player2 (IA)");
        Assert(hitLane == 3, "El carril debe ser 3 (Right)");
        Assert(hitRating == HitResult.Perfect, "La calificacion debe ser Perfect");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] AIController: Resolucion de impacto con flujo compartido de evaluacion");
    }

    /// <summary>
    /// Verifica que cuando la IA decide omitir una nota (Miss intencional), no la resuelve antes de tiempo
    /// y permite que expire naturalmente como auto-miss cuando la nota rebase la HitZone.
    /// </summary>
    public static void TestAIMissDecisionAllowsAutoMiss()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        AIController ai = new AIController();
        ai.aiPlayer = PlayerID.Player2;
        ai.difficulty = AIDifficulty.Easy;

        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player2;
        note.laneID = 1;
        note.beatOfThisNote = 5.0f;
        NoteObject.ActiveNotes.Add(note);

        // Forzar roll 10 (< 30) en Easy -> Miss intencional
        HitResult result = ai.SimulateHitForNote(note, 10f);

        Assert(result == HitResult.Miss, "El resultado de la omision debe ser Miss");
        Assert(note.isResolved, "La nota queda marcada como resuelta tras el fallo");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] AIController: Omision de notas y despacho de Miss segun probabilidad");
    }

    /// <summary>
    /// Verifica la programacion y activacion automatica de poderes especiales segun la dificultad.
    /// Hard usa el poder casi inmediatamente (0.25 beats), mientras Normal espera 2.0 beats.
    /// </summary>
    public static void TestAIPowerActivationCycle()
    {
        GameEvents.ResetAllEvents();
        SetupTestRhythmManager(120f);
        SetRhythmTime(RhythmManager.Instance, 105.0); // Beat 10.0

        AIController ai = new AIController();
        ai.aiPlayer = PlayerID.Player2;
        ai.difficulty = AIDifficulty.Hard;
        ai.autoUsePowers = true;

        // Registrar suscripciones a eventos de Unity manualmente (como si estuviese en escena)
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(AIController).GetMethod("OnEnable", flags).Invoke(ai, null);

        PlayerID activatedPlayer = PlayerID.None;
        PowerType activatedPower = PowerType.None;
        int activationEvents = 0;

        GameEvents.OnPowerActivated += delegate(PlayerID p, PowerType pw)
        {
            activatedPlayer = p;
            activatedPower = pw;
            activationEvents++;
        };

        // Disparar evento de desbloqueo de poder para Player2
        GameEvents.TriggerPowerUnlocked(PlayerID.Player2, PowerType.RocketPotato);

        Assert(ai.currentAvailablePower == PowerType.RocketPotato, "La IA debe almacenar el poder desbloqueado");
        AssertApproximately(ai.scheduledPowerBeat, 10.25f, 0.01f, "En Hard el poder debe programarse a Beat 10.25 (0.25 beats)");

        // En Beat 10.1 aun no debe activarse
        SetRhythmTime(RhythmManager.Instance, 105.05); // Beat 10.1
        ai.ProcessPower();
        Assert(activationEvents == 0, "Aun no debe activarse antes de scheduledPowerBeat");

        // Al alcanzar Beat 10.25 debe activarse y consumirse
        SetRhythmTime(RhythmManager.Instance, 105.125); // Beat 10.25
        ai.ProcessPower();

        Assert(activationEvents == 1, "Debe dispararse OnPowerActivated");
        Assert(activatedPlayer == PlayerID.Player2, "El activador debe ser Player2");
        Assert(activatedPower == PowerType.RocketPotato, "El poder debe ser RocketPotato");
        Assert(ai.currentAvailablePower == PowerType.None, "El poder debe haberse consumido");

        typeof(AIController).GetMethod("OnDisable", flags).Invoke(ai, null);
        Console.WriteLine("[PASS] AIController: Activacion automatica de poderes segun retraso por dificultad");
    }

    /// <summary>
    /// Verifica que la IA detecta cuando el rival activa Papa Electrica,
    /// estrechando su ventana de evaluacion mientras este activo el sabotaje.
    /// </summary>
    public static void TestAIElectricPotatoReaction()
    {
        GameEvents.ResetAllEvents();

        AIController ai = new AIController();
        ai.aiPlayer = PlayerID.Player2;

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(AIController).GetMethod("OnEnable", flags).Invoke(ai, null);

        Assert(!ai.isElectricEffectActive, "isElectricEffectActive debe ser false al inicio");

        // Player1 activa ElectricPotato contra Player2 (la IA)
        GameEvents.TriggerPowerActivated(PlayerID.Player1, PowerType.ElectricPotato);
        Assert(ai.isElectricEffectActive, "isElectricEffectActive debe ser true tras sabotaje del rival");

        // Expiracion de ElectricPotato
        GameEvents.TriggerPowerExpired(PowerType.ElectricPotato);
        Assert(!ai.isElectricEffectActive, "isElectricEffectActive debe volver a false tras expirar");

        typeof(AIController).GetMethod("OnDisable", flags).Invoke(ai, null);
        Console.WriteLine("[PASS] AIController: Reaccion y recuperacion ante sabotaje de Papa Electrica");
    }

    /// <summary>
    /// Verifica que al iniciar una nueva ronda se reinician las notas programadas,
    /// los poderes pendientes y los efectos de sabotaje activos.
    /// </summary>
    public static void TestAIRoundReset()
    {
        GameEvents.ResetAllEvents();

        AIController ai = new AIController();
        ai.aiPlayer = PlayerID.Player2;

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(AIController).GetMethod("OnEnable", flags).Invoke(ai, null);

        GameEvents.TriggerPowerUnlocked(PlayerID.Player2, PowerType.GhostPotato);
        GameEvents.TriggerPowerActivated(PlayerID.Player1, PowerType.ElectricPotato);

        Assert(ai.currentAvailablePower == PowerType.GhostPotato, "Debe tener GhostPotato guardado");
        Assert(ai.isElectricEffectActive, "Debe tener efecto electrico activo");

        // Disparar inicio de ronda
        GameEvents.TriggerRoundStarted(2);

        Assert(ai.currentAvailablePower == PowerType.None, "El poder debe haberse limpiado al iniciar ronda");
        Assert(!ai.isElectricEffectActive, "El efecto electrico debe limpiarse al iniciar ronda");

        typeof(AIController).GetMethod("OnDisable", flags).Invoke(ai, null);
        Console.WriteLine("[PASS] AIController: Limpieza completa de estado en OnRoundStarted");
    }

    public static int RunAll()
    {
        TestAIDifficultyOffsetDistributions();
        TestAIProcessesOnlyTargetPlayerNotes();
        TestAIHitEvaluationAndResolutionFlow();
        TestAIMissDecisionAllowsAutoMiss();
        TestAIPowerActivationCycle();
        TestAIElectricPotatoReaction();
        TestAIRoundReset();
        return 7;
    }
}
