using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Suite de pruebas unitarias para PowerUpManager.
/// Verifica: activación con carga, rechazo sin carga, rechazo con poder ya activo,
/// emisión de eventos OnPowerActivated / OnPowerExpired, cancelación por ronda,
/// independencia entre jugadores y OnDisable.
/// Nota: la duración real (WaitForSeconds) no se verifica en tests de consola;
/// se testea la expiración llamando directamente a ExpirePower por reflexión.
/// </summary>
public static class PowerUpManagerTests
{
    private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static int passed;

    public static int RunAll()
    {
        passed = 0;
        Run("TryActivatePower succeeds when charge is ready",           TestActivateSucceedsWithCharge);
        Run("TryActivatePower fails when no charge available",          TestActivateFailsWithoutCharge);
        Run("TryActivatePower fails when power already active",         TestActivateFailsWhenAlreadyActive);
        Run("TryActivatePower fails for PowerType.None",                TestActivateFailsForNone);
        Run("OnPowerActivated emitted on activation",                   TestPowerActivatedEventEmitted);
        Run("OnPowerExpired emitted on expiry",                         TestPowerExpiredEventEmitted);
        Run("ActivePower1 set and cleared correctly",                   TestActivePower1Lifecycle);
        Run("ActivePower2 independent from Player1",                    TestPlayer2Independence);
        Run("TryActivatePower consumes ComboSystem charge",             TestChargeConsumedOnActivation);
        Run("RoundStarted cancels active power and emits Expired",      TestRoundStartedCancelsPower);
        Run("RoundStarted with no active power does not emit Expired",  TestRoundStartedNoOpWhenIdle);
        Run("OnDisable unsubscribes event handlers",                    TestOnDisableUnsubscribes);
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

    /// <summary>
    /// Crea un ComboSystem con la carga de un jugador ya lista.
    /// </summary>
    private static ComboSystem CreateReadyCombo(PlayerID player, int threshold = 3)
    {
        ComboSystem cs = new ComboSystem();
        GameConfig cfg = ScriptableObject.CreateInstance<GameConfig>();
        cfg.streakThresholdForPower = threshold;
        cs.config = cfg;
        InvokeOn(cs, "OnEnable");

        // Dispara hits para llegar al umbral
        for (int i = 0; i < threshold; i++)
            GameEvents.TriggerHitResolved(player, HitResult.Perfect, 1f);

        GameEvents.ResetAllEvents();   // limpia suscripciones de combo para test limpio
        InvokeOn(cs, "OnDisable");     // desconectar combo del bus (el test lo maneja)
        return cs;
    }

    private static PowerUpManager CreateManager(ComboSystem combo = null)
    {
        PowerUpManager pm = new PowerUpManager();
        GameConfig cfg = ScriptableObject.CreateInstance<GameConfig>();
        pm.config = cfg;
        pm.comboSystem = combo;
        InvokeOn(pm, "Awake");
        InvokeOn(pm, "OnEnable");
        return pm;
    }

    /// <summary>
    /// Simula la expiración del poder sin esperar WaitForSeconds.
    /// </summary>
    private static void ForceExpire(PowerUpManager pm, PlayerID player, PowerType power)
    {
        typeof(PowerUpManager)
            .GetMethod("ExpirePower", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(pm, new object[] { player, power });
    }

    private static void InvokeOn(object instance, string method)
    {
        instance.GetType().GetMethod(method, AllInstance).Invoke(instance, null);
    }

    // ── Tests ──────────────────────────────────────────────────────────────

    private static void TestActivateSucceedsWithCharge()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);
        bool result = pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);
        AssertTrue(result, "TryActivatePower should return true when charge is ready.");
        AssertEqual(PowerType.RocketPotato, pm.ActivePower1, "ActivePower1 should be RocketPotato after activation.");
    }

    private static void TestActivateFailsWithoutCharge()
    {
        ComboSystem cs = new ComboSystem();
        cs.config = ScriptableObject.CreateInstance<GameConfig>();
        PowerUpManager pm = CreateManager(cs);
        bool result = pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);
        AssertTrue(!result, "TryActivatePower should return false when no charge is available.");
        AssertEqual(PowerType.None, pm.ActivePower1, "No power should be active without a charge.");
    }

    private static void TestActivateFailsWhenAlreadyActive()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1, threshold: 1);
        PowerUpManager pm = CreateManager(cs);
        pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);

        // Simular una segunda carga (manipulamos el flag)
        typeof(ComboSystem).GetField("powerReady1", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(cs, true);

        bool second = pm.TryActivatePower(PlayerID.Player1, PowerType.GhostPotato);
        AssertTrue(!second, "TryActivatePower should fail if a power is already active.");
        AssertEqual(PowerType.RocketPotato, pm.ActivePower1, "First active power should not be replaced.");
    }

    private static void TestActivateFailsForNone()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);
        bool result = pm.TryActivatePower(PlayerID.Player1, PowerType.None);
        AssertTrue(!result, "TryActivatePower should reject PowerType.None.");
    }

    private static void TestPowerActivatedEventEmitted()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);

        PlayerID eventPlayer = PlayerID.None;
        PowerType eventPower = PowerType.None;
        GameEvents.OnPowerActivated += (p, t) => { eventPlayer = p; eventPower = t; };

        pm.TryActivatePower(PlayerID.Player1, PowerType.GhostPotato);

        AssertEqual(PlayerID.Player1, eventPlayer, "OnPowerActivated should report Player1.");
        AssertEqual(PowerType.GhostPotato, eventPower, "OnPowerActivated should report GhostPotato.");
    }

    private static void TestPowerExpiredEventEmitted()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);
        pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);

        PowerType expiredPower = PowerType.None;
        GameEvents.OnPowerExpired += t => expiredPower = t;

        ForceExpire(pm, PlayerID.Player1, PowerType.RocketPotato);

        AssertEqual(PowerType.RocketPotato, expiredPower, "OnPowerExpired should report RocketPotato.");
    }

    private static void TestActivePower1Lifecycle()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);

        AssertEqual(PowerType.None, pm.ActivePower1, "No power should be active before activation.");
        pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);
        AssertEqual(PowerType.RocketPotato, pm.ActivePower1, "Power should be active after TryActivatePower.");

        ForceExpire(pm, PlayerID.Player1, PowerType.RocketPotato);
        AssertEqual(PowerType.None, pm.ActivePower1, "Power should be None after expiry.");
    }

    private static void TestPlayer2Independence()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player2, threshold: 2);
        PowerUpManager pm = CreateManager(cs);

        pm.TryActivatePower(PlayerID.Player2, PowerType.GhostPotato);

        AssertEqual(PowerType.None,       pm.ActivePower1, "P1 should have no active power.");
        AssertEqual(PowerType.GhostPotato, pm.ActivePower2, "P2 should have GhostPotato active.");
    }

    private static void TestChargeConsumedOnActivation()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1, threshold: 3);
        PowerUpManager pm = CreateManager(cs);

        AssertTrue(cs.PowerReady1, "Charge should be ready before activation.");
        pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);
        AssertTrue(!cs.PowerReady1, "Charge should be consumed after activation.");
    }

    private static void TestRoundStartedCancelsPower()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);
        pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato);

        PowerType expiredPower = PowerType.None;
        GameEvents.OnPowerExpired += t => expiredPower = t;

        GameEvents.TriggerRoundStarted(2);

        AssertEqual(PowerType.None,       pm.ActivePower1, "Active power should be cancelled on RoundStarted.");
        AssertEqual(PowerType.RocketPotato, expiredPower,  "OnPowerExpired should be emitted on RoundStarted cancel.");
    }

    private static void TestRoundStartedNoOpWhenIdle()
    {
        PowerUpManager pm = CreateManager(null);
        int expiredCount = 0;
        GameEvents.OnPowerExpired += _ => expiredCount++;

        GameEvents.TriggerRoundStarted(1);

        AssertEqual(0, expiredCount, "OnPowerExpired should NOT fire if no power was active.");
    }

    private static void TestOnDisableUnsubscribes()
    {
        ComboSystem cs = CreateReadyCombo(PlayerID.Player1);
        PowerUpManager pm = CreateManager(cs);
        InvokeOn(pm, "OnDisable");

        // Después de deshabilitar, RoundStarted no debería cancelar nada
        pm.TryActivatePower(PlayerID.Player1, PowerType.RocketPotato); // esto no debería activar (sin suscripción)
        // Lo activamos directamente para probar que el handler está desconectado
        typeof(PowerUpManager)
            .GetMethod("ActivatePower", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(pm, new object[] { PlayerID.Player1, PowerType.RocketPotato });

        int expiredCount = 0;
        GameEvents.OnPowerExpired += _ => expiredCount++;
        GameEvents.TriggerRoundStarted(2);

        AssertEqual(PowerType.RocketPotato, pm.ActivePower1, "Power should still be active (OnRoundStarted unsubscribed).");
        AssertEqual(0, expiredCount, "OnPowerExpired should NOT fire when manager is disabled.");
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
