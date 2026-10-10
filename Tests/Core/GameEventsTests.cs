using System;

public static class GameEventsTests
{
    private static int passed;

    public static int RunAll()
    {
        passed = 0;
        Run("OnBeat forwards beat index", TestBeat);
        Run("OnMeasure forwards measure index", TestMeasure);
        Run("OnPotatoLaunched forwards launch data", TestPotatoLaunched);
        Run("OnPotatoReachedHitZone forwards player", TestPotatoReachedHitZone);
        Run("OnPotatoExploded forwards player", TestPotatoExploded);
        Run("OnHitResolved forwards hit data", TestHitResolved);
        Run("OnPlayerMiss forwards player", TestPlayerMiss);
        Run("OnStreakUpdated forwards streak data", TestStreakUpdated);
        Run("OnStreakReset forwards player", TestStreakReset);
        Run("OnPowerUnlocked forwards power data", TestPowerUnlocked);
        Run("OnPowerActivated forwards power data", TestPowerActivated);
        Run("OnPowerExpired forwards power", TestPowerExpired);
        Run("OnHeatUpdated forwards normalized heat", TestHeatUpdated);
        Run("OnRoundStarted forwards round number", TestRoundStarted);
        Run("OnRoundEnded forwards score data", TestRoundEnded);
        Run("OnMatchEnded forwards winner", TestMatchEnded);
        Run("OnMatchReset notifies subscribers", TestMatchReset);
        Run("Events support multiple subscribers", TestMultipleSubscribers);
        Run("ResetAllEvents clears every subscription", TestResetAllEvents);
        return passed;
    }

    private static void Run(string name, Action test)
    {
        GameEvents.ResetAllEvents();
        try
        {
            test();
            passed++;
            Console.WriteLine("[PASS] " + name);
        }
        finally
        {
            GameEvents.ResetAllEvents();
        }
    }

    private static void TestBeat()
    {
        int received = -1;
        GameEvents.OnBeat += value => received = value;
        GameEvents.TriggerBeat(12);
        AssertEqual(12, received, "Beat index was not forwarded.");
    }

    private static void TestMeasure()
    {
        int received = -1;
        GameEvents.OnMeasure += value => received = value;
        GameEvents.TriggerMeasure(4);
        AssertEqual(4, received, "Measure index was not forwarded.");
    }

    private static void TestPotatoLaunched()
    {
        PlayerID receivedFrom = PlayerID.None;
        PlayerID receivedTo = PlayerID.None;
        double receivedTargetTime = 0;
        float receivedTravelBeats = 0;
        GameEvents.OnPotatoLaunched += (from, to, targetTime, travelBeats) =>
        {
            receivedFrom = from;
            receivedTo = to;
            receivedTargetTime = targetTime;
            receivedTravelBeats = travelBeats;
        };

        GameEvents.TriggerPotatoLaunched(PlayerID.Player1, PlayerID.Player2, 42.25, 2.5f);
        AssertEqual(PlayerID.Player1, receivedFrom, "Launch source was not forwarded.");
        AssertEqual(PlayerID.Player2, receivedTo, "Launch destination was not forwarded.");
        AssertEqual(42.25, receivedTargetTime, "Target DSP time was not forwarded.");
        AssertEqual(2.5f, receivedTravelBeats, "Travel beats were not forwarded.");
    }

    private static void TestPotatoReachedHitZone()
    {
        PlayerID received = PlayerID.None;
        GameEvents.OnPotatoReachedHitZone += player => received = player;
        GameEvents.TriggerPotatoReachedHitZone(PlayerID.Player2);
        AssertEqual(PlayerID.Player2, received, "Target player was not forwarded.");
    }

    private static void TestPotatoExploded()
    {
        PlayerID received = PlayerID.None;
        GameEvents.OnPotatoExploded += player => received = player;
        GameEvents.TriggerPotatoExploded(PlayerID.Player1);
        AssertEqual(PlayerID.Player1, received, "Player who received the explosion was not forwarded.");
    }

    private static void TestHitResolved()
    {
        PlayerID receivedPlayer = PlayerID.None;
        HitResult receivedResult = HitResult.Miss;
        float receivedAccuracy = 0;
        GameEvents.OnHitResolved += (player, result, accuracy) =>
        {
            receivedPlayer = player;
            receivedResult = result;
            receivedAccuracy = accuracy;
        };

        GameEvents.TriggerHitResolved(PlayerID.Player2, HitResult.Perfect, 0.95f);
        AssertEqual(PlayerID.Player2, receivedPlayer, "Hit player was not forwarded.");
        AssertEqual(HitResult.Perfect, receivedResult, "Hit result was not forwarded.");
        AssertEqual(0.95f, receivedAccuracy, "Hit accuracy was not forwarded.");
    }

    private static void TestPlayerMiss()
    {
        PlayerID received = PlayerID.None;
        GameEvents.OnPlayerMiss += player => received = player;
        GameEvents.TriggerPlayerMiss(PlayerID.Player2);
        AssertEqual(PlayerID.Player2, received, "Player who missed was not forwarded.");
    }

    private static void TestStreakUpdated()
    {
        PlayerID receivedPlayer = PlayerID.None;
        int receivedStreak = -1;
        bool receivedPowerReady = false;
        GameEvents.OnStreakUpdated += (player, streak, powerReady) =>
        {
            receivedPlayer = player;
            receivedStreak = streak;
            receivedPowerReady = powerReady;
        };

        GameEvents.TriggerStreakUpdated(PlayerID.Player1, 5, true);
        AssertEqual(PlayerID.Player1, receivedPlayer, "Streak player was not forwarded.");
        AssertEqual(5, receivedStreak, "Streak count was not forwarded.");
        AssertEqual(true, receivedPowerReady, "Power availability was not forwarded.");
    }

    private static void TestStreakReset()
    {
        PlayerID received = PlayerID.None;
        GameEvents.OnStreakReset += player => received = player;
        GameEvents.TriggerStreakReset(PlayerID.Player1);
        AssertEqual(PlayerID.Player1, received, "Player whose streak reset was not forwarded.");
    }

    private static void TestPowerUnlocked()
    {
        PlayerID receivedPlayer = PlayerID.None;
        PowerType receivedPower = PowerType.None;
        GameEvents.OnPowerUnlocked += (player, power) =>
        {
            receivedPlayer = player;
            receivedPower = power;
        };

        GameEvents.TriggerPowerUnlocked(PlayerID.Player2, PowerType.RocketPotato);
        AssertEqual(PlayerID.Player2, receivedPlayer, "Power recipient was not forwarded.");
        AssertEqual(PowerType.RocketPotato, receivedPower, "Unlocked power was not forwarded.");
    }

    private static void TestPowerActivated()
    {
        PlayerID receivedPlayer = PlayerID.None;
        PowerType receivedPower = PowerType.None;
        GameEvents.OnPowerActivated += (player, power) =>
        {
            receivedPlayer = player;
            receivedPower = power;
        };

        GameEvents.TriggerPowerActivated(PlayerID.Player1, PowerType.GoldenPotato);
        AssertEqual(PlayerID.Player1, receivedPlayer, "Power user was not forwarded.");
        AssertEqual(PowerType.GoldenPotato, receivedPower, "Activated power was not forwarded.");
    }

    private static void TestPowerExpired()
    {
        PowerType received = PowerType.None;
        GameEvents.OnPowerExpired += power => received = power;
        GameEvents.TriggerPowerExpired(PowerType.ElectricPotato);
        AssertEqual(PowerType.ElectricPotato, received, "Expired power was not forwarded.");
    }

    private static void TestHeatUpdated()
    {
        float received = -1;
        GameEvents.OnHeatUpdated += heat => received = heat;
        GameEvents.TriggerHeatUpdated(0.72f);
        AssertEqual(0.72f, received, "Normalized heat was not forwarded.");
    }

    private static void TestRoundStarted()
    {
        int received = -1;
        GameEvents.OnRoundStarted += round => received = round;
        GameEvents.TriggerRoundStarted(2);
        AssertEqual(2, received, "Round number was not forwarded.");
    }

    private static void TestRoundEnded()
    {
        PlayerID receivedWinner = PlayerID.None;
        int receivedPlayer1Score = -1;
        int receivedPlayer2Score = -1;
        GameEvents.OnRoundEnded += (winner, player1Score, player2Score) =>
        {
            receivedWinner = winner;
            receivedPlayer1Score = player1Score;
            receivedPlayer2Score = player2Score;
        };

        GameEvents.TriggerRoundEnded(PlayerID.Player2, 1, 2);
        AssertEqual(PlayerID.Player2, receivedWinner, "Round winner was not forwarded.");
        AssertEqual(1, receivedPlayer1Score, "Player 1 score was not forwarded.");
        AssertEqual(2, receivedPlayer2Score, "Player 2 score was not forwarded.");
    }

    private static void TestMatchEnded()
    {
        PlayerID received = PlayerID.None;
        GameEvents.OnMatchEnded += player => received = player;
        GameEvents.TriggerMatchEnded(PlayerID.Player2);
        AssertEqual(PlayerID.Player2, received, "Match winner was not forwarded.");
    }

    private static void TestMatchReset()
    {
        int received = 0;
        GameEvents.OnMatchReset += () => received++;
        GameEvents.TriggerMatchReset();
        AssertEqual(1, received, "Match reset event did not notify its subscriber once.");
    }

    private static void TestMultipleSubscribers()
    {
        int firstSubscriber = 0;
        int secondSubscriber = 0;
        GameEvents.OnBeat += value => firstSubscriber = value;
        GameEvents.OnBeat += value => secondSubscriber = value;
        GameEvents.TriggerBeat(3);
        AssertEqual(3, firstSubscriber, "First subscriber was not called.");
        AssertEqual(3, secondSubscriber, "Second subscriber was not called.");
    }

    private static void TestResetAllEvents()
    {
        int notifications = 0;
        GameEvents.OnBeat += value => notifications++;
        GameEvents.OnMeasure += value => notifications++;
        GameEvents.OnPotatoLaunched += (from, to, targetTime, travelBeats) => notifications++;
        GameEvents.OnPotatoReachedHitZone += player => notifications++;
        GameEvents.OnPotatoExploded += player => notifications++;
        GameEvents.OnHitResolved += (player, result, accuracy) => notifications++;
        GameEvents.OnPlayerMiss += player => notifications++;
        GameEvents.OnStreakUpdated += (player, streak, powerReady) => notifications++;
        GameEvents.OnStreakReset += player => notifications++;
        GameEvents.OnPowerUnlocked += (player, power) => notifications++;
        GameEvents.OnPowerActivated += (player, power) => notifications++;
        GameEvents.OnPowerExpired += power => notifications++;
        GameEvents.OnHeatUpdated += heat => notifications++;
        GameEvents.OnRoundStarted += round => notifications++;
        GameEvents.OnRoundEnded += (winner, player1Score, player2Score) => notifications++;
        GameEvents.OnMatchEnded += winner => notifications++;
        GameEvents.OnMatchReset += () => notifications++;

        GameEvents.ResetAllEvents();

        GameEvents.TriggerBeat(1);
        GameEvents.TriggerMeasure(1);
        GameEvents.TriggerPotatoLaunched(PlayerID.Player1, PlayerID.Player2, 10, 1);
        GameEvents.TriggerPotatoReachedHitZone(PlayerID.Player2);
        GameEvents.TriggerPotatoExploded(PlayerID.Player2);
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Good, 0.5f);
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);
        GameEvents.TriggerStreakUpdated(PlayerID.Player1, 1, false);
        GameEvents.TriggerStreakReset(PlayerID.Player1);
        GameEvents.TriggerPowerUnlocked(PlayerID.Player1, PowerType.RocketPotato);
        GameEvents.TriggerPowerActivated(PlayerID.Player1, PowerType.RocketPotato);
        GameEvents.TriggerPowerExpired(PowerType.RocketPotato);
        GameEvents.TriggerHeatUpdated(0.5f);
        GameEvents.TriggerRoundStarted(1);
        GameEvents.TriggerRoundEnded(PlayerID.Player1, 1, 0);
        GameEvents.TriggerMatchEnded(PlayerID.Player1);
        GameEvents.TriggerMatchReset();

        AssertEqual(0, notifications, "ResetAllEvents left one or more subscribers registered.");
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!object.Equals(expected, actual))
        {
            throw new InvalidOperationException(message + " Expected: " + expected + "; actual: " + actual + ".");
        }
    }
}
