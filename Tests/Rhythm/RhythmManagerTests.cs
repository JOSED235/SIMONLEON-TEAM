using System;
using UnityEngine;

public static class RhythmManagerTests
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

    private static void AssertApproximatelyDouble(double actual, double expected, double tolerance, string message)
    {
        if (System.Math.Abs(actual - expected) > tolerance)
        {
            throw new Exception(string.Format("ASSERTION FAILED: {0}. Expected: {1}, Actual: {2}", message, expected, actual));
        }
    }

    private static RhythmManager CreateTestInstance(float bpm = 120f)
    {
        RhythmManager.Instance = null;
        RhythmManager rm = new RhythmManager();
        rm.songBpm = bpm;
        rm.beatsPerMeasure = 4;
        rm.songOffset = 0f;
        rm.gameConfig = ScriptableObject.CreateInstance<GameConfig>();
        rm.musicSource = new AudioSource();
        rm.RecalculateBeatDuration();
        return rm;
    }

    public static void TestBeatDurationCalculation()
    {
        RhythmManager rm = CreateTestInstance(120f);
        AssertApproximately(rm.secPerBeat, 0.5f, 0.001f, "120 BPM debe resultar en 0.5s por beat");

        rm.songBpm = 60f;
        rm.RecalculateBeatDuration();
        AssertApproximately(rm.secPerBeat, 1.0f, 0.001f, "60 BPM debe resultar en 1.0s por beat");

        rm.songBpm = 117f;
        rm.RecalculateBeatDuration();
        AssertApproximately(rm.secPerBeat, 60f / 117f, 0.001f, "117 BPM debe coincidir con la formula 60/BPM");

        Console.WriteLine("[PASS] RhythmManager: Calculo de duracion de beat por BPM");
    }

    public static void TestBidirectionalConversions()
    {
        RhythmManager rm = CreateTestInstance(120f); // 0.5s por beat
        AudioSettings.dspTime = 200.0;
        rm.StartTrack(200.0);

        // Beat a DSP
        double dspBeat0 = rm.BeatToDspTime(0f);
        AssertApproximatelyDouble(dspBeat0, 200.0, 0.001, "Beat 0 debe ser DSP 200.0");

        double dspBeat4 = rm.BeatToDspTime(4f);
        AssertApproximatelyDouble(dspBeat4, 202.0, 0.001, "Beat 4 debe ser DSP 202.0 (4 * 0.5s)");

        // DSP a Beat
        float beatFromDsp = rm.DspTimeToBeat(202.0);
        AssertApproximately(beatFromDsp, 4.0f, 0.001f, "DSP 202.0 debe convertirse a Beat 4.0");

        float beatFromDspHalf = rm.DspTimeToBeat(201.25);
        AssertApproximately(beatFromDspHalf, 2.5f, 0.001f, "DSP 201.25 debe convertirse a Beat 2.5");

        // Beat a Posicion en Cancion
        float posBeat3 = rm.BeatToSongPosition(3f);
        AssertApproximately(posBeat3, 1.5f, 0.001f, "Beat 3 debe ser 1.5s de cancion");

        float beatFromPos = rm.SongPositionToBeat(1.5f);
        AssertApproximately(beatFromPos, 3.0f, 0.001f, "1.5s debe ser Beat 3");

        Console.WriteLine("[PASS] RhythmManager: Conversiones bidireccionales Beat <-> DSP y SongPosition");
    }

    public static void TestTrackPlaybackLifecycle()
    {
        RhythmManager rm = CreateTestInstance(120f);
        AudioSettings.dspTime = 100.0;

        rm.StartTrack(100.5);
        Assert(rm.isPlaying, "Track debe estar marcada como isPlaying tras StartTrack");
        Assert(!rm.isPaused, "Track no debe estar pausada tras StartTrack");
        AssertApproximatelyDouble(rm.dspSongStartTime, 100.5, 0.001, "dspSongStartTime debe guardar el scheduled time");
        Assert(rm.musicSource.isPlaying, "AudioSource debe haberse programado para reproducir");

        rm.PauseTrack();
        Assert(rm.isPaused, "Track debe marcarse como isPaused tras PauseTrack");

        rm.ResumeTrack();
        Assert(!rm.isPaused, "Track no debe estar pausada tras ResumeTrack");

        rm.StopTrack();
        Assert(!rm.isPlaying, "Track no debe estar activa tras StopTrack");
        AssertApproximately(rm.songPosition, 0f, 0.001f, "songPosition debe reiniciarse a 0");
        AssertApproximately(rm.songPositionInBeats, 0f, 0.001f, "songPositionInBeats debe reiniciarse a 0");

        Console.WriteLine("[PASS] RhythmManager: Ciclo de vida de reproduccion (Start, Pause, Resume, Stop)");
    }

    public static void TestEvaluateHitTimingWindows()
    {
        RhythmManager rm = CreateTestInstance(120f); // 0.5s por beat
        AudioSettings.dspTime = 100.0;
        rm.StartTrack(100.0);

        // Simulamos que la cancion avanza al Beat 10.0 exacto (DSP 105.0)
        // Probamos una nota con targetBeat = 10.0f
        float targetBeat = 10.0f;

        // 1. PERFECT (diferencia de 0.02s <= 0.05s)
        // 0.02s en beat es 0.02 / 0.5 = 0.04 beats
        AudioSettings.dspTime = 105.02; // 5.02s -> Beat 10.04
        // Forzamos actualizacion manual de campos de Update
        SetRhythmTime(rm, 105.02);

        float timeDiff;
        float accuracy;
        HitResult result = rm.EvaluateHit(targetBeat, out timeDiff, out accuracy, false);
        Assert(result == HitResult.Perfect, "0.02s de diferencia debe ser HitResult.Perfect");
        Assert(accuracy > 0.5f, "Precision de Perfect debe ser > 0.5");

        // 2. GREAT (diferencia de 0.08s: > 0.05s y <= 0.10s)
        SetRhythmTime(rm, 105.08);
        result = rm.EvaluateHit(targetBeat, out timeDiff, out accuracy, false);
        Assert(result == HitResult.Great, "0.08s de diferencia debe ser HitResult.Great");

        // 3. GOOD (diferencia de 0.13s: > 0.10s y <= 0.15s)
        SetRhythmTime(rm, 105.13);
        result = rm.EvaluateHit(targetBeat, out timeDiff, out accuracy, false);
        Assert(result == HitResult.Good, "0.13s de diferencia debe ser HitResult.Good");

        // 4. MISS (diferencia de 0.20s: > 0.15s)
        SetRhythmTime(rm, 105.20);
        result = rm.EvaluateHit(targetBeat, out timeDiff, out accuracy, false);
        Assert(result == HitResult.Miss, "0.20s de diferencia debe ser HitResult.Miss");
        AssertApproximately(accuracy, 0f, 0.001f, "Precision en Miss debe ser 0");

        Console.WriteLine("[PASS] RhythmManager: Calificaciones de ventanas temporales (Perfect, Great, Good, Miss)");
    }

    public static void TestElectricPotatoWindowReduction()
    {
        RhythmManager rm = CreateTestInstance(120f);
        AudioSettings.dspTime = 100.0;
        rm.StartTrack(100.0);
        float targetBeat = 5.0f; // DSP 102.5

        // Diferencia de 0.04s.
        // En modo normal (tol 0.05s) es Perfect.
        // En modo electrico (tol 0.03s) NO es Perfect, sino Great.
        SetRhythmTime(rm, 102.54);

        float diff;
        float acc;
        HitResult normalResult = rm.EvaluateHit(targetBeat, out diff, out acc, false);
        Assert(normalResult == HitResult.Perfect, "0.04s debe ser Perfect en condiciones normales");

        HitResult electricResult = rm.EvaluateHit(targetBeat, out diff, out acc, true);
        Assert(electricResult == HitResult.Great, "0.04s debe degradarse a Great bajo Papa Electrica (tol reducida a 0.03s)");

        Console.WriteLine("[PASS] RhythmManager: Reduccion de ventana Perfect bajo efecto de Papa Electrica");
    }

    public static void TestNoteEvaluationExactBoundariesAndAccuracies()
    {
        RhythmManager rm = CreateTestInstance(120f);
        float acc;

        // 1. Centro exacto (0.00s) -> Perfect, precision 100%
        HitResult r0 = rm.ClassifyTimeDifference(0.00f, out acc);
        Assert(r0 == HitResult.Perfect, "0.00s debe ser HitResult.Perfect");
        AssertApproximately(acc, 1.00f, 0.001f, "Precision en 0.00s debe ser 1.00 (100%)");

        // 2. Mitad de Perfect (0.025s) -> Perfect, precision 95%
        HitResult rMidPerf = rm.ClassifyTimeDifference(0.025f, out acc);
        Assert(rMidPerf == HitResult.Perfect, "0.025s debe ser HitResult.Perfect");
        AssertApproximately(acc, 0.95f, 0.001f, "Precision en 0.025s debe ser 0.95 (95%)");

        // 3. Limite exacto de Perfect (0.050s) -> Perfect, precision 90%
        HitResult rEdgePerf = rm.ClassifyTimeDifference(0.050f, out acc);
        Assert(rEdgePerf == HitResult.Perfect, "0.050s debe ser HitResult.Perfect");
        AssertApproximately(acc, 0.90f, 0.001f, "Precision en 0.050s debe ser 0.90 (90%)");

        // 4. Apenas superado Perfect (0.051s) -> Great, precision ~89%
        HitResult rJustGreat = rm.ClassifyTimeDifference(0.051f, out acc);
        Assert(rJustGreat == HitResult.Great, "0.051s debe ser HitResult.Great");
        Assert(acc >= 0.70f && acc <= 0.89f, "Precision en 0.051s debe estar en rango 70%-89%");

        // 5. Limite exacto de Great (0.100s) -> Great, precision 70%
        HitResult rEdgeGreat = rm.ClassifyTimeDifference(0.100f, out acc);
        Assert(rEdgeGreat == HitResult.Great, "0.100s debe ser HitResult.Great");
        AssertApproximately(acc, 0.70f, 0.001f, "Precision en 0.100s debe ser 0.70 (70%)");

        // 6. Apenas superado Great (0.101s) -> Good, precision ~69%
        HitResult rJustGood = rm.ClassifyTimeDifference(0.101f, out acc);
        Assert(rJustGood == HitResult.Good, "0.101s debe ser HitResult.Good");
        Assert(acc >= 0.50f && acc <= 0.69f, "Precision en 0.101s debe estar en rango 50%-69%");

        // 7. Limite exacto de Good (0.150s) -> Good, precision 50%
        HitResult rEdgeGood = rm.ClassifyTimeDifference(0.150f, out acc);
        Assert(rEdgeGood == HitResult.Good, "0.150s debe ser HitResult.Good");
        AssertApproximately(acc, 0.50f, 0.001f, "Precision en 0.150s debe ser 0.50 (50%)");

        // 8. Fuera de tolerancia (>0.150s) -> Miss, precision 0%
        HitResult rMiss1 = rm.ClassifyTimeDifference(0.151f, out acc);
        Assert(rMiss1 == HitResult.Miss, "0.151s debe ser HitResult.Miss");
        AssertApproximately(acc, 0.0f, 0.001f, "Precision en Miss debe ser 0");

        HitResult rMissFar = rm.ClassifyTimeDifference(0.350f, out acc);
        Assert(rMissFar == HitResult.Miss, "0.350s debe ser HitResult.Miss");
        AssertApproximately(acc, 0.0f, 0.001f, "Precision en Miss lejano debe ser 0");

        Console.WriteLine("[PASS] RhythmManager: Limites exactos de ventanas y precision segun GDD");
    }

    public static void TestNoteEvaluationSymmetryEarlyAndLate()
    {
        RhythmManager rm = CreateTestInstance(120f); // 0.5s por beat
        rm.StartTrack(100.0);
        float targetBeat = 10.0f; // Instante DSP 105.0

        // 1. Desfase de +-0.03s (Perfect temprano vs tarde)
        SetRhythmTime(rm, 104.97); // 30ms antes (temprano)
        float diffEarly, accEarly;
        HitResult resEarly = rm.EvaluateHit(targetBeat, out diffEarly, out accEarly);

        SetRhythmTime(rm, 105.03); // 30ms despues (tarde)
        float diffLate, accLate;
        HitResult resLate = rm.EvaluateHit(targetBeat, out diffLate, out accLate);

        Assert(resEarly == HitResult.Perfect && resLate == HitResult.Perfect, "Ambos impactos a +-30ms deben ser Perfect");
        AssertApproximately(diffEarly, diffLate, 0.001f, "Diferencias absolutas deben ser identicas");
        AssertApproximately(accEarly, accLate, 0.001f, "Precisiones deben ser identicas");

        // 2. Desfase de +-0.08s (Great temprano vs tarde)
        SetRhythmTime(rm, 104.92);
        HitResult resGreatEarly = rm.EvaluateHit(targetBeat, out diffEarly, out accEarly);
        SetRhythmTime(rm, 105.08);
        HitResult resGreatLate = rm.EvaluateHit(targetBeat, out diffLate, out accLate);

        Assert(resGreatEarly == HitResult.Great && resGreatLate == HitResult.Great, "Ambos impactos a +-80ms deben ser Great");
        AssertApproximately(accEarly, accLate, 0.001f, "Precisiones en Great deben ser simetricas");

        // 3. Desfase de +-0.13s (Good temprano vs tarde)
        SetRhythmTime(rm, 104.87);
        HitResult resGoodEarly = rm.EvaluateHit(targetBeat, out diffEarly, out accEarly);
        SetRhythmTime(rm, 105.13);
        HitResult resGoodLate = rm.EvaluateHit(targetBeat, out diffLate, out accLate);

        Assert(resGoodEarly == HitResult.Good && resGoodLate == HitResult.Good, "Ambos impactos a +-130ms deben ser Good");
        AssertApproximately(accEarly, accLate, 0.001f, "Precisiones en Good deben ser simetricas");

        Console.WriteLine("[PASS] RhythmManager: Simetria temporal entre pulsaciones tempranas y tardias");
    }

    public static void TestNoteEvaluationFullResultAndScore()
    {
        RhythmManager rm = CreateTestInstance(120f);
        rm.StartTrack(100.0);
        SetRhythmTime(rm, 105.0); // Exactamente en el beat 10

        // Validar puntajes oficiales del GDD
        Assert(RhythmManager.GetHitScore(HitResult.Perfect) == 100, "Perfect debe otorgar +100 pts");
        Assert(RhythmManager.GetHitScore(HitResult.Great)   == 50,  "Great debe otorgar +50 pts");
        Assert(RhythmManager.GetHitScore(HitResult.Good)    == 25,  "Good debe otorgar +25 pts");
        Assert(RhythmManager.GetHitScore(HitResult.Miss)    == 0,   "Miss debe otorgar 0 pts");

        // Validar struct NoteEvaluationResult en impacto exacto
        NoteEvaluationResult fullPerf = rm.EvaluateHitFull(10.0f);
        Assert(fullPerf.result == HitResult.Perfect, "Resultado debe ser Perfect");
        Assert(fullPerf.isHit == true, "isHit debe ser true en Perfect");
        Assert(fullPerf.score == 100, "Puntaje debe ser 100");
        AssertApproximately(fullPerf.timeDifference, 0.0f, 0.001f, "Desfase debe ser 0.0");
        AssertApproximately(fullPerf.accuracyNormalized, 1.0f, 0.001f, "Precision debe ser 1.0");

        // Validar struct en Miss
        SetRhythmTime(rm, 105.30); // 300ms tarde
        NoteEvaluationResult fullMiss = rm.EvaluateHitFull(10.0f);
        Assert(fullMiss.result == HitResult.Miss, "Resultado debe ser Miss");
        Assert(fullMiss.isHit == false, "isHit debe ser false en Miss");
        Assert(fullMiss.score == 0, "Puntaje debe ser 0");

        Console.WriteLine("[PASS] RhythmManager: Evaluacion completa empaquetada (NoteEvaluationResult) y puntajes GDD");
    }

    public static void TestEvaluateNoteHelper()
    {
        RhythmManager rm = CreateTestInstance(120f);
        rm.StartTrack(100.0);
        SetRhythmTime(rm, 102.5); // Beat 5.0

        NoteObject note = new NoteObject();
        note.beatOfThisNote = 5.02f; // 0.01s desfase -> Perfect

        float diff, acc;
        HitResult result = rm.EvaluateNote(note, out diff, out acc);
        Assert(result == HitResult.Perfect, "EvaluateNote con note a 0.01s debe ser Perfect");
        AssertApproximately(diff, 0.01f, 0.001f, "Desfase debe ser 0.01s");

        // Evaluacion con nota nula
        HitResult nullResult = rm.EvaluateNote(null, out diff, out acc);
        Assert(nullResult == HitResult.Miss, "EvaluateNote con nota nula debe retornar Miss");

        Console.WriteLine("[PASS] RhythmManager: Sobrecarga EvaluateNote con NoteObject");
    }

    private static void SetRhythmTime(RhythmManager rm, double currentDsp)
    {
        // Usa reflection para actualizar los campos privados de calculo de Update
        var type = typeof(RhythmManager);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        float songPos = (float)(currentDsp - rm.dspSongStartTime - rm.songOffset);
        float songPosBeats = songPos / rm.secPerBeat;

        type.GetField("songPositionField", flags).SetValue(rm, songPos);
        type.GetField("songPositionInBeatsField", flags).SetValue(rm, songPosBeats);
    }

    public static int RunAll()
    {
        TestBeatDurationCalculation();
        TestBidirectionalConversions();
        TestTrackPlaybackLifecycle();
        TestEvaluateHitTimingWindows();
        TestElectricPotatoWindowReduction();
        TestNoteEvaluationExactBoundariesAndAccuracies();
        TestNoteEvaluationSymmetryEarlyAndLate();
        TestNoteEvaluationFullResultAndScore();
        TestEvaluateNoteHelper();
        return 9;
    }
}
