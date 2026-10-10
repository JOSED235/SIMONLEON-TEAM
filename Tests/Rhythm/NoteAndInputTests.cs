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
        return 11;
    }
}
