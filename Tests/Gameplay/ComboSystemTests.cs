using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Suite de pruebas unitarias para ComboSystem.
/// Verifica: rachas independientes, reinicio por miss, desbloqueo único de poder,
/// máximo de una carga, reset al iniciar ronda y publicación de eventos correctos.
/// </summary>
public static class ComboSystemTests
{
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static int passed;

    public static int RunAll()
    {
        passed = 0;
        Run("Good hit increments player1 streak",           TestGoodHitIncrementsP1);
        Run("Great hit increments player2 streak",          TestGreatHitIncrementsP2);
        Run("Perfect hit increments streak",                TestPerfectHitIncrementsStreak);
        Run("Miss via OnHitResolved resets streak",         TestMissViaHitResolvedResetsStreak);
        Run("Miss via OnPlayerMiss resets streak",          TestPlayerMissResetsStreak);
        Run("Miss via OnNoteMiss resets streak",            TestNoteMissResetsStreak);
        Run("Streak reset not fired if already zero",       TestNoResetEventWhenAlreadyZero);
        Run("Streaks are independent per player",           TestStreaksAreIndependent);
        Run("Power unlocked at threshold",                  TestPowerUnlockedAtThreshold);
        Run("Power unlocked only once (no double charge)",  TestPowerUnlockedOnlyOnce);
        Run("Power stays ready after more hits",            TestPowerStaysReadyAfterMoreHits);
        Run("ConsumePower clears ready flag",               TestConsumePowerClearsFlag);
        Run("RoundStarted resets both streaks and charges", TestRoundStartedResetsAll);
        Run("OnStreakUpdated published on every hit",       TestStreakUpdatedEventOnHit);
        Run("OnStreakUpdated published on reset",           TestStreakUpdatedEventOnReset);
        Run("OnStreakReset published on miss",              TestStreakResetEventPublished);
        Run("OnPowerUnlocked published at threshold",       TestPowerUnlockedEventPublished);
        Run("OnDisable unsubscribes from all events",       TestOnDisableUnsubscribes);
        return passed;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static void Run(string name, Action test)
    {
        GameEvents.ResetAllEvents();
        try
        {
            test();
            passed++;
            Console.WriteLine("[PASS] " + name);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[FAIL] " + name + " — " + ex.Message);
        }
        finally
        {
            GameEvents.ResetAllEvents();
        }
    }

    private static ComboSystem CreateSystem(int threshold = 5)
    {
        ComboSystem cs = new ComboSystem();
        GameConfig cfg = ScriptableObject.CreateInstance<GameConfig>();
        cfg.streakThresholdForPower = threshold;
        cs.config = cfg;
        Invoke(cs, "OnEnable");
        return cs;
    }

    private static void HitN(PlayerID player, int times, HitResult result = HitResult.Perfect)
    {
        for (int i = 0; i < times; i++)
            GameEvents.TriggerHitResolved(player, result, 1f);
    }

    private static void Invoke(object instance, string method)
    {
        typeof(ComboSystem).GetMethod(method, InstanceFlags).Invoke(instance, null);
    }

    // ── Tests ──────────────────────────────────────────────────────────────

    private static void TestGoodHitIncrementsP1()
    {
        ComboSystem cs = CreateSystem();
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Good, 0.8f);
        AssertEqual(1, cs.Streak1, "P1 streak should be 1 after Good hit.");
        AssertEqual(0, cs.Streak2, "P2 streak should remain 0.");
    }

    private static void TestGreatHitIncrementsP2()
    {
        ComboSystem cs = CreateSystem();
        GameEvents.TriggerHitResolved(PlayerID.Player2, HitResult.Great, 0.9f);
        AssertEqual(0, cs.Streak1, "P1 streak should remain 0.");
        AssertEqual(1, cs.Streak2, "P2 streak should be 1 after Great hit.");
    }

    private static void TestPerfectHitIncrementsStreak()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 3, HitResult.Perfect);
        AssertEqual(3, cs.Streak1, "Streak should be 3 after 3 Perfect hits.");
    }

    private static void TestMissViaHitResolvedResetsStreak()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 3);
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Miss, 0f);
        AssertEqual(0, cs.Streak1, "Streak should reset to 0 on Miss via HitResolved.");
    }

    private static void TestPlayerMissResetsStreak()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 4);
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);
        AssertEqual(0, cs.Streak1, "Streak should reset to 0 on PlayerMiss.");
    }

    private static void TestNoteMissResetsStreak()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player2, 2);
        GameEvents.TriggerNoteMiss(PlayerID.Player2, 1);
        AssertEqual(0, cs.Streak2, "Streak should reset to 0 on NoteMiss.");
    }

    private static void TestNoResetEventWhenAlreadyZero()
    {
        ComboSystem cs = CreateSystem();
        int resetCount = 0;
        GameEvents.OnStreakReset += _ => resetCount++;

        // Dos misses consecutivos cuando la racha ya es 0
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);

        AssertEqual(0, resetCount, "StreakReset should NOT fire when streak is already 0.");
    }

    private static void TestStreaksAreIndependent()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 3);
        HitN(PlayerID.Player2, 2);
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);

        AssertEqual(0, cs.Streak1, "P1 streak should reset independently.");
        AssertEqual(2, cs.Streak2, "P2 streak should be unaffected by P1 miss.");
    }

    private static void TestPowerUnlockedAtThreshold()
    {
        ComboSystem cs = CreateSystem(threshold: 5);
        HitN(PlayerID.Player1, 5);
        AssertTrue(cs.PowerReady1, "Power should be ready after reaching threshold.");
    }

    private static void TestPowerUnlockedOnlyOnce()
    {
        ComboSystem cs = CreateSystem(threshold: 3);
        int unlockCount = 0;
        GameEvents.OnPowerUnlocked += (_, __) => unlockCount++;

        HitN(PlayerID.Player1, 6); // double the threshold
        AssertEqual(1, unlockCount, "Power should only unlock once regardless of extra hits.");
    }

    private static void TestPowerStaysReadyAfterMoreHits()
    {
        ComboSystem cs = CreateSystem(threshold: 3);
        HitN(PlayerID.Player1, 5);
        AssertTrue(cs.PowerReady1, "Power should still be ready after hitting more than threshold.");
    }

    private static void TestConsumePowerClearsFlag()
    {
        ComboSystem cs = CreateSystem(threshold: 3);
        HitN(PlayerID.Player1, 3);
        AssertTrue(cs.PowerReady1, "Power should be ready before consuming.");
        cs.ConsumePower(PlayerID.Player1);
        AssertTrue(!cs.PowerReady1, "Power should not be ready after consuming.");
    }

    private static void TestRoundStartedResetsAll()
    {
        ComboSystem cs = CreateSystem(threshold: 3);
        HitN(PlayerID.Player1, 4);
        HitN(PlayerID.Player2, 3);
        AssertTrue(cs.PowerReady1, "P1 power should be ready before round reset.");
        AssertTrue(cs.PowerReady2, "P2 power should be ready before round reset.");

        GameEvents.TriggerRoundStarted(2);

        AssertEqual(0, cs.Streak1, "P1 streak should reset on new round.");
        AssertEqual(0, cs.Streak2, "P2 streak should reset on new round.");
        AssertTrue(!cs.PowerReady1, "P1 power ready should reset on new round.");
        AssertTrue(!cs.PowerReady2, "P2 power ready should reset on new round.");
    }

    private static void TestStreakUpdatedEventOnHit()
    {
        ComboSystem cs = CreateSystem();
        int eventStreak = -1;
        PlayerID eventPlayer = PlayerID.None;
        GameEvents.OnStreakUpdated += (p, s, _) => { eventPlayer = p; eventStreak = s; };

        GameEvents.TriggerHitResolved(PlayerID.Player2, HitResult.Great, 0.9f);

        AssertEqual(PlayerID.Player2, eventPlayer, "StreakUpdated should report Player2.");
        AssertEqual(1, eventStreak, "StreakUpdated should report streak of 1.");
    }

    private static void TestStreakUpdatedEventOnReset()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 2);

        int lastStreak = -1;
        GameEvents.OnStreakUpdated += (p, s, _) => { if (p == PlayerID.Player1) lastStreak = s; };

        GameEvents.TriggerPlayerMiss(PlayerID.Player1);
        AssertEqual(0, lastStreak, "StreakUpdated should report 0 on reset.");
    }

    private static void TestStreakResetEventPublished()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 3);
        PlayerID resetPlayer = PlayerID.None;
        GameEvents.OnStreakReset += p => resetPlayer = p;

        GameEvents.TriggerPlayerMiss(PlayerID.Player1);
        AssertEqual(PlayerID.Player1, resetPlayer, "StreakReset event should identify the correct player.");
    }

    private static void TestPowerUnlockedEventPublished()
    {
        ComboSystem cs = CreateSystem(threshold: 5);
        PlayerID unlockedPlayer = PlayerID.None;
        GameEvents.OnPowerUnlocked += (p, _) => unlockedPlayer = p;

        HitN(PlayerID.Player2, 5);
        AssertEqual(PlayerID.Player2, unlockedPlayer, "PowerUnlocked event should identify Player2.");
    }

    private static void TestOnDisableUnsubscribes()
    {
        ComboSystem cs = CreateSystem();
        HitN(PlayerID.Player1, 2);
        Invoke(cs, "OnDisable");

        // After disable, new hits should NOT affect the streak
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Perfect, 1f);
        AssertEqual(2, cs.Streak1, "Disabled ComboSystem should not react to new events.");
    }

    // ── Assertions ─────────────────────────────────────────────────────────

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!object.Equals(expected, actual))
            throw new InvalidOperationException(message + " Expected: " + expected + "; actual: " + actual + ".");
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
