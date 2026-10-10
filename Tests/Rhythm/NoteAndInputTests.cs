using System;
using System.Collections.Generic;
using UnityEngine;

public static class NoteAndInputTests
{
    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception("ASSERTION FAILED: " + message);
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

    public static void TestNoteObjectSingleHitResolution()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();

        PlayerID receivedPlayer = PlayerID.None;
        int receivedLane = -1;
        HitResult receivedResult = HitResult.Miss;
        int hitCount = 0;

        GameEvents.OnNoteHit += delegate(PlayerID p, int lane, HitResult r, float acc)
        {
            receivedPlayer = p;
            receivedLane = lane;
            receivedResult = r;
            hitCount++;
        };

        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 2; // Up
        note.laneDirection = LaneDirection.Up;
        note.beatOfThisNote = 5.0f;

        Assert(!note.isResolved, "La nota no debe estar resuelta al inicio");

        // Primer impacto
        note.ResolveHit(HitResult.Perfect, 0.95f);
        Assert(note.isResolved, "La nota debe marcarse como isResolved tras ResolveHit");
        Assert(hitCount == 1, "Debe dispararse exactamente un evento OnNoteHit");
        Assert(receivedPlayer == PlayerID.Player1, "El jugador del evento debe ser Player1");
        Assert(receivedLane == 2, "El carril del evento debe ser 2 (Up)");
        Assert(receivedResult == HitResult.Perfect, "El resultado debe ser Perfect");

        // Intento de doble consumo
        note.ResolveHit(HitResult.Good, 0.5f);
        note.ResolveMiss();
        Assert(hitCount == 1, "No debe dispararse un segundo evento al intentar resolverla nuevamente");

        Console.WriteLine("[PASS] NoteObject: Resolucion unica de impacto (sin doble consumo)");
    }

    public static void TestNoteObjectAutoMissResolution()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();

        PlayerID missedPlayer = PlayerID.None;
        int missedLane = -1;
        int missEventCount = 0;

        GameEvents.OnNoteMiss += delegate(PlayerID p, int lane)
        {
            missedPlayer = p;
            missedLane = lane;
            missEventCount++;
        };

        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player2;
        note.laneID = 1; // Down
        note.laneDirection = LaneDirection.Down;
        note.beatOfThisNote = 8.0f;

        note.ResolveMiss();
        Assert(note.isResolved, "La nota debe estar resuelta tras ResolveMiss");
        Assert(missEventCount == 1, "Debe dispararse OnNoteMiss una sola vez");
        Assert(missedPlayer == PlayerID.Player2, "El jugador con miss debe ser Player2");
        Assert(missedLane == 1, "El carril con miss debe ser 1 (Down)");

        // Segundo intento de resolucion
        note.ResolveMiss();
        Assert(missEventCount == 1, "ResolveMiss duplicado no debe disparar eventos adicionales");

        Console.WriteLine("[PASS] NoteObject: Resolucion de Miss unico por auto-miss");
    }

    public static void TestNoteRegistryAndSearch()
    {
        NoteObject.ActiveNotes.Clear();

        NoteObject note1 = new NoteObject();
        note1.targetPlayer = PlayerID.Player1;
        note1.laneID = 0; // Left
        note1.beatOfThisNote = 10.0f;
        NoteObject.ActiveNotes.Add(note1);

        NoteObject note2 = new NoteObject();
        note2.targetPlayer = PlayerID.Player2;
        note2.laneID = 0; // Left (para el otro jugador)
        note2.beatOfThisNote = 10.0f;
        NoteObject.ActiveNotes.Add(note2);

        NoteObject note3 = new NoteObject();
        note3.targetPlayer = PlayerID.Player1;
        note3.laneID = 1; // Down
        note3.beatOfThisNote = 10.1f;
        NoteObject.ActiveNotes.Add(note3);

        // Buscar nota para P1 en carril 0 cerca del beat 10.02
        NoteObject found = NoteObject.GetClosestActiveNote(PlayerID.Player1, 0, 10.02f, 0.2f);
        Assert(found == note1, "Debe encontrar note1 para P1 carril 0");

        // Buscar nota para P2 en carril 0 cerca del beat 10.0
        NoteObject foundP2 = NoteObject.GetClosestActiveNote(PlayerID.Player2, 0, 10.0f, 0.2f);
        Assert(foundP2 == note2, "Debe encontrar note2 para P2 carril 0");

        // Buscar fuera de tolerancia
        NoteObject notFound = NoteObject.GetClosestActiveNote(PlayerID.Player1, 0, 12.0f, 0.2f);
        Assert(notFound == null, "No debe retornar notas fuera de la ventana maxima de tolerancia");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] NoteObject: Registro y busqueda de nota activa por jugador y carril");
    }

    public static void TestPlayerInputSuccessfulHit()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        // Ponemos el reloj en el Beat 4.0 exacto (DSP 102.0)
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 2.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 4.0f);

        // Creamos una nota para P1 en carril 3 (Right) en Beat 4.02 (desfase 0.01s = Perfect)
        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 3;
        note.laneDirection = LaneDirection.Right;
        note.beatOfThisNote = 4.02f;
        NoteObject.ActiveNotes.Add(note);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = true;

        bool hitDetected = false;
        HitResult hitResult = HitResult.Miss;
        GameEvents.OnNoteHit += delegate(PlayerID p, int lane, HitResult r, float acc)
        {
            if (p == PlayerID.Player1 && lane == 3)
            {
                hitDetected = true;
                hitResult = r;
            }
        };

        // Jugador 1 presiona carril 3
        input.HandleLanePress(PlayerID.Player1, 3);

        Assert(hitDetected, "PlayerInput debio resolver el acierto de la nota");
        Assert(hitResult == HitResult.Perfect, "El golpe con 0.01s de desfase debio calificarse como Perfect");
        Assert(note.isResolved, "La nota debio marcarse como resuelta");

        // Si se presiona nuevamente en el mismo carril, la nota ya esta resuelta y no vuelve a impactarse
        int secondHitCount = 0;
        GameEvents.OnNoteHit += delegate { secondHitCount++; };
        input.HandleLanePress(PlayerID.Player1, 3);
        Assert(secondHitCount == 0, "Una nota ya resuelta no debe generar aciertos repetidos");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] PlayerInput: Impacto exitoso en carril y prevencion de doble acierto");
    }

    public static void TestPlayerInputGhostTapPenalty()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = true;

        int missCount = 0;
        PlayerID penalizedPlayer = PlayerID.None;
        GameEvents.OnPlayerMiss += delegate(PlayerID p)
        {
            missCount++;
            penalizedPlayer = p;
        };

        // Presionar carril 0 cuando no hay ninguna nota activa
        input.HandleLanePress(PlayerID.Player1, 0);

        Assert(missCount == 1, "Un Ghost Tap con penalizacion debe disparar OnPlayerMiss");
        Assert(penalizedPlayer == PlayerID.Player1, "El jugador penalizado debe ser Player1");

        Console.WriteLine("[PASS] PlayerInput: Penalizacion de Ghost Tap cuando no hay notas en ventana");
    }

    /// <summary>
    /// Verifica que GetClosestActiveNoteByTime usa segundos como unidad de busqueda
    /// y coincide con la ventana de GameConfig sin requerir conversion manual en el sitio de llamada.
    /// </summary>
    public static void TestGetClosestActiveNoteByTime()
    {
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        // Simular RhythmManager en Beat 10.0 exacto
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 5.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 10.0f);

        // Nota a 0.10s del beat actual (10.2 beats a 120BPM = 0.10s de desfase)
        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 2; // Up
        note.beatOfThisNote = 10.2f;
        NoteObject.ActiveNotes.Add(note);

        // Con ventana de 0.15s (Good tolerance) debe encontrarla (0.10s < 0.15s)
        NoteObject found = NoteObject.GetClosestActiveNoteByTime(PlayerID.Player1, 2, 0.15f);
        Assert(found == note, "GetClosestActiveNoteByTime debe encontrar la nota dentro de Good tolerance (0.15s)");

        // Con ventana de 0.08s (menor que 0.10s) no debe encontrarla
        NoteObject notFound = NoteObject.GetClosestActiveNoteByTime(PlayerID.Player1, 2, 0.08f);
        Assert(notFound == null, "GetClosestActiveNoteByTime no debe retornar nota fuera de la ventana indicada");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] NoteObject: GetClosestActiveNoteByTime usa segundos correctamente");
    }

    /// <summary>
    /// Verifica que NoteSpawner.HandlePowerExpired solo revierte el modificador
    /// del jugador victima registrado, dejando intacto al otro jugador.
    /// </summary>
    public static void TestNoteSpawnerPowerExpiryTargetsCorrectVictim()
    {
        GameEvents.ResetAllEvents();

        NoteSpawner spawner = new NoteSpawner();
        spawner.ResetSpawner();

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var type  = typeof(NoteSpawner);

        // Invocar HandlePowerActivated directamente (P1 activa -> victima es P2)
        var activateMethod = type.GetMethod("HandlePowerActivated", flags);
        activateMethod.Invoke(spawner, new object[] { PlayerID.Player1, PowerType.RocketPotato });

        float p1Mult  = (float)type.GetField("p1SpeedMultiplier", flags).GetValue(spawner);
        float p2Mult  = (float)type.GetField("p2SpeedMultiplier", flags).GetValue(spawner);
        PlayerID victim = (PlayerID)type.GetField("rocketVictim", flags).GetValue(spawner);

        Assert(p2Mult > 1.0f,              "P2SpeedMultiplier debe ser > 1 tras Papa Cohete contra P2");
        Assert(victim == PlayerID.Player2, "rocketVictim debe registrarse como Player2");
        Assert(p1Mult == 1.0f,             "P1SpeedMultiplier no debe alterarse si P2 es la victima");

        // Expirar el poder: solo P2 debe restaurarse; P1 queda intacto
        var expireMethod = type.GetMethod("HandlePowerExpired", flags);
        expireMethod.Invoke(spawner, new object[] { PowerType.RocketPotato });

        p2Mult = (float)type.GetField("p2SpeedMultiplier", flags).GetValue(spawner);
        victim = (PlayerID)type.GetField("rocketVictim", flags).GetValue(spawner);

        Assert(p2Mult == 1.0f,        "P2SpeedMultiplier debe volver a 1 tras expirar Papa Cohete");
        Assert(victim == PlayerID.None, "rocketVictim debe limpiarse a None tras expiracion");

        Console.WriteLine("[PASS] NoteSpawner: HandlePowerExpired revierte solo la victima correcta");
    }

    /// <summary>
    /// Verifica que PlayerInput.HandlePowerExpired revierte ElectricPotato
    /// solo en el jugador victima registrado, dejando intacto al otro jugador.
    /// </summary>
    public static void TestPlayerInputElectricPotatoExpiryTargetsCorrectVictim()
    {
        GameEvents.ResetAllEvents();

        PlayerInput input = new PlayerInput();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var type  = typeof(PlayerInput);

        // P2 activa ElectricPotato -> victima es P1
        var activateMethod = type.GetMethod("HandlePowerActivated", flags);
        activateMethod.Invoke(input, new object[] { PlayerID.Player2, PowerType.ElectricPotato });

        bool p1Electric = (bool)type.GetField("p1ElectricActive", flags).GetValue(input);
        bool p2Electric = (bool)type.GetField("p2ElectricActive", flags).GetValue(input);
        PlayerID victim = (PlayerID)type.GetField("electricVictim", flags).GetValue(input);

        Assert(p1Electric,              "p1ElectricActive debe estar true (P1 es victima)");
        Assert(!p2Electric,             "p2ElectricActive no debe alterarse (P2 es atacante)");
        Assert(victim == PlayerID.Player1, "electricVictim debe registrarse como Player1");

        // Expirar el poder: solo P1 debe restaurarse
        var expireMethod = type.GetMethod("HandlePowerExpired", flags);
        expireMethod.Invoke(input, new object[] { PowerType.ElectricPotato });

        p1Electric = (bool)type.GetField("p1ElectricActive", flags).GetValue(input);
        victim     = (PlayerID)type.GetField("electricVictim", flags).GetValue(input);

        Assert(!p1Electric,           "p1ElectricActive debe volver a false tras expirar ElectricPotato");
        Assert(victim == PlayerID.None, "electricVictim debe limpiarse a None tras expiracion");

        Console.WriteLine("[PASS] PlayerInput: HandlePowerExpired revierte ElectricPotato solo en la victima correcta");
    }

    /// <summary>
    /// Verifica que HandlePowerUnlocked respeta la restriccion de maxima 1 carga
    /// por jugador: si ya hay un poder almacenado, el nuevo se descarta.
    /// </summary>
    public static void TestPlayerInputMaxOnePowerCharge()
    {
        GameEvents.ResetAllEvents();

        PlayerInput input = new PlayerInput();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var type  = typeof(PlayerInput);

        var unlockMethod = type.GetMethod("HandlePowerUnlocked", flags);

        // Primer desbloqueo: se almacena RocketPotato
        unlockMethod.Invoke(input, new object[] { PlayerID.Player1, PowerType.RocketPotato });
        PowerType p1Power = (PowerType)type.GetField("p1AvailablePower", flags).GetValue(input);
        Assert(p1Power == PowerType.RocketPotato, "Primer poder desbloqueado debe almacenarse");

        // Segundo desbloqueo mientras ya hay carga: debe ignorarse
        unlockMethod.Invoke(input, new object[] { PlayerID.Player1, PowerType.GhostPotato });
        p1Power = (PowerType)type.GetField("p1AvailablePower", flags).GetValue(input);
        Assert(p1Power == PowerType.RocketPotato, "El poder almacenado no debe reemplazarse si ya hay una carga");

        Console.WriteLine("[PASS] PlayerInput: Maximo una carga de poder por jugador");
    }

    /// <summary>
    /// Verifica el ciclo completo de activacion de poder:
    /// desbloqueo -> carga disponible -> pulsacion de tecla -> consumo y evento.
    /// </summary>
    public static void TestPlayerInputPowerActivationCycle()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();

        PlayerInput input = new PlayerInput();
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var type  = typeof(PlayerInput);

        // Desbloquear poder para P1 directamente via evento
        var unlockMethod = type.GetMethod("HandlePowerUnlocked", flags);
        unlockMethod.Invoke(input, new object[] { PlayerID.Player1, PowerType.GhostPotato });

        PlayerID activatedBy = PlayerID.None;
        PowerType activatedPower = PowerType.None;
        int activationCount = 0;
        GameEvents.OnPowerActivated += delegate(PlayerID p, PowerType pw)
        {
            activatedBy    = p;
            activatedPower = pw;
            activationCount++;
        };

        // Simular pulsacion de tecla de poder para P1
        var pressMethod = type.GetMethod("HandlePowerPress", flags);
        pressMethod.Invoke(input, new object[] { PlayerID.Player1 });

        Assert(activationCount == 1,                      "Debe dispararse exactamente un OnPowerActivated");
        Assert(activatedBy == PlayerID.Player1,           "El activador debe ser Player1");
        Assert(activatedPower == PowerType.GhostPotato,   "El poder activado debe ser GhostPotato");

        // La carga debe haberse consumido
        PowerType p1Power = (PowerType)type.GetField("p1AvailablePower", flags).GetValue(input);
        Assert(p1Power == PowerType.None, "La carga de P1 debe quedar en None tras activar el poder");

        // Una segunda pulsacion no debe generar un nuevo evento
        pressMethod.Invoke(input, new object[] { PlayerID.Player1 });
        Assert(activationCount == 1, "Sin carga disponible no debe dispararse un segundo OnPowerActivated");

        Console.WriteLine("[PASS] PlayerInput: Ciclo completo de activacion de poder (desbloqueo, uso y consumo)");
    }

    /// <summary>
    /// Verifica que los cuatro carriles de P2 (WASD) se resuelven independientemente
    /// y que la nota correcta por carril se consume sin afectar otros carriles.
    /// </summary>
    public static void TestPlayerInputP2FourLaneIndependence()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        // Simular beat en 8.0 exacto
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField",      flags).SetValue(RhythmManager.Instance, 4.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 8.0f);

        // Crear una nota para P2 en cada uno de los 4 carriles, todas en Beat 8.0
        NoteObject[] notes = new NoteObject[4];
        for (int i = 0; i < 4; i++)
        {
            notes[i] = new NoteObject();
            notes[i].targetPlayer  = PlayerID.Player2;
            notes[i].laneID        = i;
            notes[i].beatOfThisNote = 8.0f;
            NoteObject.ActiveNotes.Add(notes[i]);
        }

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = false;

        int hitCount = 0;
        GameEvents.OnNoteHit += delegate(PlayerID p, int lane, HitResult r, float acc)
        {
            if (p == PlayerID.Player2) hitCount++;
        };

        // Presionar carril 1 (Down) de P2: solo esa nota debe resolverse
        input.HandleLanePress(PlayerID.Player2, 1);

        Assert(notes[1].isResolved,  "La nota del carril 1 (Down) de P2 debe estar resuelta");
        Assert(!notes[0].isResolved, "La nota del carril 0 (Left) de P2 no debe estar resuelta");
        Assert(!notes[2].isResolved, "La nota del carril 2 (Up) de P2 no debe estar resuelta");
        Assert(!notes[3].isResolved, "La nota del carril 3 (Right) de P2 no debe estar resuelta");
        Assert(hitCount == 1, "Debe registrarse exactamente un acierto");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] PlayerInput: Los cuatro carriles de P2 son independientes");
    }

    /// <summary>
    /// Verifica que un acierto valido despacha OnNoteHit y OnHitResolved con la precision y calificacion correctas,
    /// y NUNCA dispara eventos de fallo (OnNoteMiss ni OnPlayerMiss).
    /// </summary>
    public static void TestNoteEvaluationEventDispatchOnHit()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 2.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 4.0f);

        // Nota en carril 2 (Up) a 0.01s (Beat 4.02) -> Perfect (precision >= 90%)
        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 2;
        note.laneDirection = LaneDirection.Up;
        note.beatOfThisNote = 4.02f;
        NoteObject.ActiveNotes.Add(note);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = true;

        PlayerID hitPlayer = PlayerID.None;
        int hitLane = -1;
        HitResult hitResult = HitResult.Miss;
        float hitAccuracy = 0f;
        int noteHitCount = 0;
        int hitResolvedCount = 0;
        int missCount = 0;

        GameEvents.OnNoteHit += delegate(PlayerID p, int l, HitResult r, float acc)
        {
            hitPlayer = p;
            hitLane = l;
            hitResult = r;
            hitAccuracy = acc;
            noteHitCount++;
        };

        GameEvents.OnHitResolved += delegate(PlayerID p, HitResult r, float acc)
        {
            hitResolvedCount++;
        };

        GameEvents.OnNoteMiss += delegate { missCount++; };
        GameEvents.OnPlayerMiss += delegate { missCount++; };

        HitResult returnedResult = input.HandleLanePress(PlayerID.Player1, 2);

        Assert(returnedResult == HitResult.Perfect, "HandleLanePress debe retornar Perfect");
        Assert(noteHitCount == 1, "Debe dispararse exactamente un OnNoteHit");
        Assert(hitResolvedCount == 1, "Debe dispararse exactamente un OnHitResolved");
        Assert(hitPlayer == PlayerID.Player1, "El jugador del impacto debe ser Player1");
        Assert(hitLane == 2, "El carril del impacto debe ser 2 (Up)");
        Assert(hitResult == HitResult.Perfect, "El resultado debe ser Perfect");
        Assert(hitAccuracy >= 0.90f, "La precision de Perfect debe ser >= 90% segun GDD");
        Assert(missCount == 0, "No debe dispararse ningun evento de fallo en un impacto exitoso");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] NoteEvaluation: Despacho de eventos de acierto (OnNoteHit y OnHitResolved)");
    }

    /// <summary>
    /// Verifica que una nota en ventana de busqueda pero con desfase fuera de Good tolerance (>0.15s)
    /// se consume como Miss forzado, disparando OnNoteMiss, OnPlayerMiss y OnHitResolved(Miss),
    /// y NUNCA dispara OnNoteHit.
    /// </summary>
    public static void TestNoteEvaluationEventDispatchOnForcedMiss()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 2.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 4.0f);

        // Desfase de 0.16s: 0.16s / 0.5s = 0.32 beats -> Beat 4.32
        // Esta dentro de searchWindowSec (0.15 + 0.02 = 0.17s) pero supera goodTol (0.15s)
        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 1; // Down
        note.laneDirection = LaneDirection.Down;
        note.beatOfThisNote = 4.32f;
        NoteObject.ActiveNotes.Add(note);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = true;

        int noteHitCount = 0;
        int noteMissCount = 0;
        int playerMissCount = 0;
        int hitResolvedCount = 0;
        HitResult resolvedRating = HitResult.Good;

        GameEvents.OnNoteHit += delegate { noteHitCount++; };
        GameEvents.OnNoteMiss += delegate(PlayerID p, int l)
        {
            if (p == PlayerID.Player1 && l == 1) noteMissCount++;
        };
        GameEvents.OnPlayerMiss += delegate(PlayerID p)
        {
            if (p == PlayerID.Player1) playerMissCount++;
        };
        GameEvents.OnHitResolved += delegate(PlayerID p, HitResult r, float acc)
        {
            if (p == PlayerID.Player1)
            {
                hitResolvedCount++;
                resolvedRating = r;
            }
        };

        HitResult returnedResult = input.HandleLanePress(PlayerID.Player1, 1);

        Assert(returnedResult == HitResult.Miss, "HandleLanePress debe retornar Miss para nota fuera de Good");
        Assert(noteHitCount == 0, "No debe dispararse OnNoteHit en un Miss forzado");
        Assert(noteMissCount == 1, "Debe dispararse OnNoteMiss para el carril 1");
        Assert(playerMissCount == 1, "Debe dispararse OnPlayerMiss para Player1");
        Assert(hitResolvedCount == 1, "Debe dispararse OnHitResolved con calificacion Miss");
        Assert(resolvedRating == HitResult.Miss, "La calificacion de OnHitResolved debe ser Miss");
        Assert(note.isResolved, "La nota debio consumirse como resuelta");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] NoteEvaluation: Despacho de eventos de Miss forzado fuera de ventana");
    }

    /// <summary>
    /// Verifica que presionar fuera de ventana (sin notas) NUNCA otorga aciertos y aplica la regla de Ghost Tap.
    /// </summary>
    public static void TestNoteEvaluationOutsideWindowNeverAwardsHit()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = true;

        int hitCount = 0;
        int missCount = 0;
        GameEvents.OnNoteHit += delegate { hitCount++; };
        GameEvents.OnPlayerMiss += delegate { missCount++; };

        // Presionar carril vacio con penalizacion activa
        HitResult res = input.HandleLanePress(PlayerID.Player1, 0);
        Assert(res == HitResult.Miss, "Pulsar sin notas debe retornar Miss");
        Assert(hitCount == 0, "No debe otorgarse acierto sin nota valida en ventana");
        Assert(missCount == 1, "Debe penalizarse con OnPlayerMiss");

        // Presionar carril vacio con penalizacion desactivada
        input.penalizeGhostTaps = false;
        int eventCount = 0;
        GameEvents.OnPlayerMiss += delegate { eventCount++; };
        GameEvents.OnHitResolved += delegate { eventCount++; };
        input.HandleLanePress(PlayerID.Player1, 0);
        Assert(eventCount == 0, "Sin penalizeGhostTaps no deben dispararse eventos");

        Console.WriteLine("[PASS] NoteEvaluation: Proteccion anti-spam y no otorgar aciertos fuera de ventana");
    }

    /// <summary>
    /// Verifica el helper NoteObject.EvaluateAndResolve:
    /// evalua directamente contra RhythmManager, consume la nota y no puede ser evaluada dos veces.
    /// </summary>
    public static void TestNoteEvaluationEvaluateAndResolveHelper()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 3.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 6.0f);

        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player2;
        note.laneID = 3; // Right
        note.beatOfThisNote = 6.01f; // 0.005s desfase -> Perfect
        NoteObject.ActiveNotes.Add(note);

        int hitCount = 0;
        GameEvents.OnNoteHit += delegate(PlayerID p, int l, HitResult r, float acc)
        {
            if (p == PlayerID.Player2 && l == 3 && r == HitResult.Perfect) hitCount++;
        };

        HitResult result = note.EvaluateAndResolve(false);
        Assert(result == HitResult.Perfect, "EvaluateAndResolve debe evaluar a Perfect");
        Assert(note.isResolved, "La nota debe quedar resuelta");
        Assert(hitCount == 1, "Debe dispararse OnNoteHit");

        // Segundo intento de evaluacion en la misma nota
        HitResult secondResult = note.EvaluateAndResolve(false);
        Assert(secondResult == HitResult.Miss, "Segunda evaluacion en nota resuelta debe retornar Miss");
        Assert(hitCount == 1, "No debe dispararse ningun evento adicional en nota ya resuelta");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] NoteEvaluation: Helper EvaluateAndResolve unifica flujo y previene doble consumo");
    }

    /// <summary>
    /// Verifica que pasar HitResult.Miss a ResolveHit se redirige a ResolveMiss por proteccion,
    /// disparando eventos de fallo y nunca OnNoteHit.
    /// </summary>
    public static void TestNoteEvaluationResolveHitGuardsAgainstMiss()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();

        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 0;

        int noteHitCount = 0;
        int noteMissCount = 0;
        int playerMissCount = 0;

        GameEvents.OnNoteHit += delegate { noteHitCount++; };
        GameEvents.OnNoteMiss += delegate { noteMissCount++; };
        GameEvents.OnPlayerMiss += delegate { playerMissCount++; };

        // Llamar ResolveHit pasando HitResult.Miss
        note.ResolveHit(HitResult.Miss, 0f);

        Assert(note.isResolved, "La nota debe estar resuelta");
        Assert(noteHitCount == 0, "ResolveHit con Miss NUNCA debe disparar OnNoteHit");
        Assert(noteMissCount == 1, "Debe disparar OnNoteMiss como proteccion de fallo");
        Assert(playerMissCount == 1, "Debe disparar OnPlayerMiss");

        Console.WriteLine("[PASS] NoteEvaluation: ResolveHit redirige Miss a ResolveMiss por integridad");
    }

    /// <summary>
    /// [Tema: Cuatro carriles]
    /// Verifica la independencia y mapeo exacto de los 4 carriles de Player 1 (Left=0, Down=1, Up=2, Right=3)
    /// alineados con la enumeracion LaneDirection del GDD.
    /// </summary>
    public static void TestFourLanesP1IndependenceAndDirectionMapping()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 4.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 8.0f);

        // Crear una nota para cada carril de P1
        LaneDirection[] directions = new LaneDirection[] { LaneDirection.Left, LaneDirection.Down, LaneDirection.Up, LaneDirection.Right };
        NoteObject[] notes = new NoteObject[4];
        for (int i = 0; i < 4; i++)
        {
            notes[i] = new NoteObject();
            notes[i].targetPlayer = PlayerID.Player1;
            notes[i].laneID = i;
            notes[i].laneDirection = directions[i];
            notes[i].beatOfThisNote = 8.0f;
            NoteObject.ActiveNotes.Add(notes[i]);
        }

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = false;

        // Golpear carril 2 (Up)
        HitResult rUp = input.HandleLanePress(PlayerID.Player1, (int)LaneDirection.Up);
        Assert(rUp == HitResult.Perfect, "El golpe en carril Up debe ser Perfect");
        Assert(notes[2].isResolved, "La nota del carril Up debe estar resuelta");
        Assert(!notes[0].isResolved, "La nota del carril Left debe seguir activa");
        Assert(!notes[1].isResolved, "La nota del carril Down debe seguir activa");
        Assert(!notes[3].isResolved, "La nota del carril Right debe seguir activa");

        // Golpear carril 0 (Left)
        HitResult rLeft = input.HandleLanePress(PlayerID.Player1, (int)LaneDirection.Left);
        Assert(rLeft == HitResult.Perfect, "El golpe en carril Left debe ser Perfect");
        Assert(notes[0].isResolved, "La nota del carril Left debe estar resuelta");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] Cuatro Carriles: Independencia y mapeo de direcciones de Player 1");
    }

    /// <summary>
    /// [Tema: Cuatro carriles y aislamiento]
    /// Verifica que cuando ambos jugadores tienen notas activas en el mismo carril en el mismo instante,
    /// la accion de un jugador impacta unicamente su propia nota y no afecta la del oponente.
    /// </summary>
    public static void TestCrossPlayerLaneIsolation()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 5.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 10.0f);

        // Ambas notas en carril 1 (Down), ambas en Beat 10.0
        NoteObject noteP1 = new NoteObject();
        noteP1.targetPlayer = PlayerID.Player1;
        noteP1.laneID = 1;
        noteP1.beatOfThisNote = 10.0f;
        NoteObject.ActiveNotes.Add(noteP1);

        NoteObject noteP2 = new NoteObject();
        noteP2.targetPlayer = PlayerID.Player2;
        noteP2.laneID = 1;
        noteP2.beatOfThisNote = 10.0f;
        NoteObject.ActiveNotes.Add(noteP2);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = false;

        // Player 1 presiona carril 1
        input.HandleLanePress(PlayerID.Player1, 1);

        Assert(noteP1.isResolved, "La nota de Player 1 debe haberse resuelto");
        Assert(!noteP2.isResolved, "La nota de Player 2 debe permanecer intacta tras la pulsacion de Player 1");

        // Player 2 presiona carril 1
        input.HandleLanePress(PlayerID.Player2, 1);
        Assert(noteP2.isResolved, "La nota de Player 2 se resuelve tras la pulsacion de Player 2");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] Cuatro Carriles: Aislamiento estricto de notas entre Player 1 y Player 2");
    }

    /// <summary>
    /// [Tema: Asignacion de controles]
    /// Verifica que las asignaciones predeterminadas de teclas coinciden con el GDD
    /// y que la reasignacion / inversion de controles funciona limpiamente.
    /// </summary>
    public static void TestPlayerKeyBindingsDefaultsAndCustomReassignment()
    {
        PlayerInput input = new PlayerInput();

        // Validar valores por defecto de P1 segun GDD
        Assert(input.player1Keys.leftKey    == KeyCode.LeftArrow,  "P1 default leftKey debe ser LeftArrow");
        Assert(input.player1Keys.downKey    == KeyCode.DownArrow,  "P1 default downKey debe ser DownArrow");
        Assert(input.player1Keys.upKey      == KeyCode.UpArrow,    "P1 default upKey debe ser UpArrow");
        Assert(input.player1Keys.rightKey   == KeyCode.RightArrow, "P1 default rightKey debe ser RightArrow");
        Assert(input.player1Keys.powerKey   == KeyCode.Return,     "P1 default powerKey debe ser Return/Enter");
        Assert(input.player1Keys.altPowerKey == KeyCode.RightShift, "P1 default altPowerKey debe ser RightShift");

        // Validar valores por defecto de P2 segun GDD
        Assert(input.player2Keys.leftKey    == KeyCode.A,          "P2 default leftKey debe ser A");
        Assert(input.player2Keys.downKey    == KeyCode.S,          "P2 default downKey debe ser S");
        Assert(input.player2Keys.upKey      == KeyCode.W,          "P2 default upKey debe ser W");
        Assert(input.player2Keys.rightKey   == KeyCode.D,          "P2 default rightKey debe ser D");
        Assert(input.player2Keys.powerKey   == KeyCode.Space,      "P2 default powerKey debe ser Space");
        Assert(input.player2Keys.altPowerKey == KeyCode.LeftShift,  "P2 default altPowerKey debe ser LeftShift");

        // Reasignacion / inversion de controles (P1 con WASD y P2 con Flechas)
        PlayerKeyBindings invertedP1 = new PlayerKeyBindings
        {
            leftKey = KeyCode.A, downKey = KeyCode.S, upKey = KeyCode.W, rightKey = KeyCode.D, powerKey = KeyCode.Space
        };
        input.player1Keys = invertedP1;
        Assert(input.player1Keys.leftKey == KeyCode.A, "Reasignacion de controles debe reflejarse correctamente");

        Console.WriteLine("[PASS] Asignacion de Controles: Mapeos oficiales del GDD y soporte de reasignacion");
    }

    /// <summary>
    /// [Tema: Auto-miss]
    /// Verifica el ciclo de vida del Auto-Miss en NoteObject.Update():
    /// la nota se mantiene activa antes de missThreshold (1.25) y se resuelve como Miss automatico
    /// tan pronto supera dicho umbral.
    /// </summary>
    public static void TestNoteObjectAutoMissThresholdLifecycle()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var updateMethod = typeof(NoteObject).GetMethod("Update", flags);

        NoteObject note = new NoteObject();
        note.targetPlayer = PlayerID.Player1;
        note.laneID = 0;
        note.beatOfThisNote = 10.0f;
        note.beatsOnScreen = 2.0f;
        note.speedMultiplier = 1.0f;
        note.missThreshold = 1.25f;
        NoteObject.ActiveNotes.Add(note);

        int missCount = 0;
        GameEvents.OnNoteMiss += delegate { missCount++; };

        // 1. Beat 10.4: progreso = (10.4 - 8.0) / 2.0 = 1.20f (< 1.25f) -> AUN NO debe ser miss
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 10.4f);
        updateMethod.Invoke(note, null);
        Assert(!note.isResolved, "La nota no debe marcarse como resuelta antes de missThreshold");
        Assert(missCount == 0, "No debe dispararse OnNoteMiss antes de missThreshold");

        // 2. Beat 10.6: progreso = (10.6 - 8.0) / 2.0 = 1.30f (>= 1.25f) -> DEBE ser auto-miss
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 10.6f);
        updateMethod.Invoke(note, null);
        Assert(note.isResolved, "La nota debe estar resuelta tras superar missThreshold");
        Assert(missCount == 1, "Debe dispararse OnNoteMiss exactamente una vez");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] Auto-miss: Ciclo de vida y detonacion exacta segun missThreshold");
    }

    /// <summary>
    /// [Tema: Pulsaciones repetidas / Anti-spam]
    /// Verifica dos escenarios criticos:
    /// 1. Machacado repetido sobre una nota ya resuelta no genera aciertos duplicados.
    /// 2. Notas secuenciales en el mismo carril: golpear la primera nota no consume prematuramente la siguiente.
    /// </summary>
    public static void TestRepeatedPressesAntiSpamAndSequentialNotes()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f); // 0.5s por beat

        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 2.0f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 4.0f);

        // Dos notas secuenciales en el mismo carril (Carril 2): Nota A en Beat 4.0, Nota B en Beat 6.0
        NoteObject noteA = new NoteObject();
        noteA.targetPlayer = PlayerID.Player1;
        noteA.laneID = 2;
        noteA.beatOfThisNote = 4.0f;
        NoteObject.ActiveNotes.Add(noteA);

        NoteObject noteB = new NoteObject();
        noteB.targetPlayer = PlayerID.Player1;
        noteB.laneID = 2;
        noteB.beatOfThisNote = 6.0f; // A 2 beats (1.0s) de distancia
        NoteObject.ActiveNotes.Add(noteB);

        PlayerInput input = new PlayerInput();
        input.penalizeGhostTaps = true;

        int hitCount = 0;
        int missCount = 0;
        GameEvents.OnNoteHit += delegate { hitCount++; };
        GameEvents.OnPlayerMiss += delegate { missCount++; };

        // 1. Primera pulsacion en Beat 4.0: impacta Nota A con exito
        HitResult res1 = input.HandleLanePress(PlayerID.Player1, 2);
        Assert(res1 == HitResult.Perfect, "Primera pulsacion debe impactar Nota A con Perfect");
        Assert(noteA.isResolved, "Nota A debe quedar resuelta");
        Assert(!noteB.isResolved, "Nota B debe permanecer intacta");
        Assert(hitCount == 1, "Debe registrarse 1 acierto");

        // 2. Machacado repetido inmediatamente en Beat 4.02 (Nota A resuelta, Nota B lejana a 0.98s)
        // Debe penalizarse como Ghost Tap sin consumir Nota B
        typeof(RhythmManager).GetField("songPositionField", flags).SetValue(RhythmManager.Instance, 2.01f);
        typeof(RhythmManager).GetField("songPositionInBeatsField", flags).SetValue(RhythmManager.Instance, 4.02f);

        HitResult res2 = input.HandleLanePress(PlayerID.Player1, 2);
        Assert(res2 == HitResult.Miss, "Pulsacion repetida sin notas en ventana debe retornar Miss");
        Assert(!noteB.isResolved, "Nota B NO debe ser consumida por pulsacion anticipada fuera de ventana");
        Assert(hitCount == 1, "El contador de aciertos debe mantenerse en 1");
        Assert(missCount == 1, "Debe registrarse 1 penalizacion de fallo por machacado");

        NoteObject.ActiveNotes.Clear();
        Console.WriteLine("[PASS] Pulsaciones Repetidas: Proteccion anti-spam y no consumo de notas secuenciales");
    }

    /// <summary>
    /// [Tema: Resolucion unica de notas]
    /// Prueba de estres exhaustiva que verifica que una nota emite estrictamente como maximo UNA resolucion
    /// ante cualquier permutacion de llamadas a ResolveHit, ResolveMiss o EvaluateAndResolve.
    /// </summary>
    public static void TestExhaustiveSingleResolutionInvariance()
    {
        GameEvents.ResetAllEvents();
        NoteObject.ActiveNotes.Clear();
        SetupTestRhythmManager(120f);

        // Seguimiento global de eventos
        int totalEvents = 0;
        GameEvents.OnHitResolved += delegate { totalEvents++; };

        // Escenario 1: ResolveHit primero, luego intentos repetidos
        NoteObject note1 = new NoteObject();
        note1.targetPlayer = PlayerID.Player1;
        note1.laneID = 0;
        note1.beatOfThisNote = 5.0f;

        totalEvents = 0;
        note1.ResolveHit(HitResult.Great, 0.8f);
        note1.ResolveHit(HitResult.Perfect, 1.0f);
        note1.ResolveMiss();
        note1.EvaluateAndResolve();
        Assert(totalEvents == 1, "Note1: Multiples resoluciones tras ResolveHit deben disparar exactamente 1 evento");
        Assert(note1.isResolved, "Note1 debe estar resuelta");

        // Escenario 2: ResolveMiss primero, luego intentos repetidos
        NoteObject note2 = new NoteObject();
        note2.targetPlayer = PlayerID.Player2;
        note2.laneID = 1;
        note2.beatOfThisNote = 6.0f;

        totalEvents = 0;
        note2.ResolveMiss();
        note2.ResolveHit(HitResult.Perfect, 1.0f);
        note2.ResolveMiss();
        note2.EvaluateAndResolve();
        Assert(totalEvents == 1, "Note2: Multiples resoluciones tras ResolveMiss deben disparar exactamente 1 evento");
        Assert(note2.isResolved, "Note2 debe estar resuelta");

        // Escenario 3: EvaluateAndResolve primero, luego llamadas redundantes
        NoteObject note3 = new NoteObject();
        note3.targetPlayer = PlayerID.Player1;
        note3.laneID = 2;
        note3.beatOfThisNote = 0f; // Beat 0 con reloj en Beat 0 -> Perfect

        totalEvents = 0;
        note3.EvaluateAndResolve();
        note3.ResolveHit(HitResult.Good, 0.5f);
        note3.ResolveMiss();
        Assert(totalEvents == 1, "Note3: EvaluateAndResolve debe emitir exactamente 1 evento");
        Assert(note3.isResolved, "Note3 debe estar resuelta");

        Console.WriteLine("[PASS] Resolucion Unica: Invariante garantizado ante cualquier permutacion de llamadas");
    }

    public static int RunAll()
    {
        TestNoteObjectSingleHitResolution();
        TestNoteObjectAutoMissResolution();
        TestNoteRegistryAndSearch();
        TestPlayerInputSuccessfulHit();
        TestPlayerInputGhostTapPenalty();
        TestGetClosestActiveNoteByTime();
        TestNoteSpawnerPowerExpiryTargetsCorrectVictim();
        TestPlayerInputElectricPotatoExpiryTargetsCorrectVictim();
        TestPlayerInputMaxOnePowerCharge();
        TestPlayerInputPowerActivationCycle();
        TestPlayerInputP2FourLaneIndependence();
        TestNoteEvaluationEventDispatchOnHit();
        TestNoteEvaluationEventDispatchOnForcedMiss();
        TestNoteEvaluationOutsideWindowNeverAwardsHit();
        TestNoteEvaluationEvaluateAndResolveHelper();
        TestNoteEvaluationResolveHitGuardsAgainstMiss();
        TestFourLanesP1IndependenceAndDirectionMapping();
        TestCrossPlayerLaneIsolation();
        TestPlayerKeyBindingsDefaultsAndCustomReassignment();
        TestNoteObjectAutoMissThresholdLifecycle();
        TestRepeatedPressesAntiSpamAndSequentialNotes();
        TestExhaustiveSingleResolutionInvariance();
        return 22;
    }
}
