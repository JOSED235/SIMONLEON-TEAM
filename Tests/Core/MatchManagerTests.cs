using System;
using System.Reflection;
using UnityEngine;

public static class MatchManagerTests
{
    private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int passed;

    public static int RunAll()
    {
        passed = 0;
        Run("GameConfig defaults match design values", TestGameConfigDefaults);
        Run("Awake creates a default config and singleton", TestAwakeCreatesDefaults);
        Run("Duplicate MatchManager instance is destroyed", TestDuplicateInstance);
        Run("StartMatch resets the match and begins round one", TestStartMatch);
        Run("Countdown transitions into Playing", TestCountdownTransition);
        Run("Successful hits alternate the active player", TestSuccessfulHitsAlternatePlayers);
        Run("Miss results do not alternate the active player", TestMissResultDoesNotAlternate);
        Run("Hits outside Playing do not change the turn", TestHitsOutsidePlayingAreIgnored);
        Run("A miss scores the opponent and assigns the next serve", TestMissAwardsPoint);
        Run("A miss at the threshold ends the match for Player 1", TestPlayerOneWinsMatch);
        Run("A miss at the threshold ends the match for Player 2", TestPlayerTwoWinsMatch);
        Run("Repeated miss events resolve a round only once", TestRepeatedMissIsIgnored);
        Run("OnDisable unsubscribes MatchManager event handlers", TestOnDisableUnsubscribes);
        Run("MatchReset restarts the match", TestMatchReset);
        return passed;
    }

    private static void Run(string name, Action test)
    {
        GameEvents.ResetAllEvents();
        ResetSingleton();
        try
        {
            test();
            passed++;
            Console.WriteLine("[PASS] " + name);
        }
        finally
        {
            GameEvents.ResetAllEvents();
            ResetSingleton();
        }
    }

    private static void TestGameConfigDefaults()
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        AssertEqual(0.07f, config.perfectTolerance, "Perfect tolerance default is incorrect.");
        AssertEqual(0.15f, config.goodTolerance, "Good tolerance default is incorrect.");
        AssertEqual(0.05f, config.electricPerfectTolerance, "Electric tolerance default is incorrect.");
        AssertEqual(2f, config.defaultTravelBeats, "Default travel beats are incorrect.");
        AssertEqual(1.25f, config.rocketSpeedMultiplier, "Rocket speed multiplier is incorrect.");
        AssertEqual(4, config.countdownBeats, "Countdown beats default is incorrect.");
        AssertEqual(5, config.streakThresholdForPower, "Power streak threshold is incorrect.");
        AssertEqual(2, config.goldenBonusHits, "Golden bonus hits default is incorrect.");
        AssertEqual(0.05f, config.heatGainPerHit, "Heat gain per hit default is incorrect.");
        AssertEqual(0.15f, config.heatGainOnMiss, "Heat gain on miss default is incorrect.");
        AssertEqual(2, config.roundsToWinMatch, "Rounds-to-win default is incorrect.");
        AssertEqual(2.5f, config.delayBeforeNextRound, "Next-round delay default is incorrect.");
        AssertEqual(3f, config.delayBeforeGameOver, "Game-over delay default is incorrect.");
    }

    private static void TestAwakeCreatesDefaults()
    {
        MatchManager manager = new MatchManager();
        Invoke(manager, "Awake");

        AssertEqual(manager, MatchManager.Instance, "Awake did not set the singleton instance.");
        AssertTrue(manager.config != null, "Awake did not create a fallback GameConfig.");
        AssertEqual(5, manager.config.streakThresholdForPower, "Fallback config has unexpected defaults.");
    }

    private static void TestDuplicateInstance()
    {
        MatchManager owner = CreateManager(null);
        MatchManager duplicate = new MatchManager();
        Invoke(duplicate, "Awake");

        AssertEqual(owner, MatchManager.Instance, "Duplicate replaced the original singleton.");
        AssertTrue(duplicate.gameObject.Destroyed, "Duplicate GameObject was not destroyed.");
    }

    private static void TestStartMatch()
    {
        MatchManager manager = CreateManager(CreateConfig(3));
        SetField(manager, "currentRound", 4);
        SetField(manager, "player1Score", 1);
        SetField(manager, "player2Score", 2);
        SetField(manager, "matchWinner", PlayerID.Player2);
        SetField(manager, "servingPlayer", PlayerID.Player2);
        SetField(manager, "activePlayer", PlayerID.Player2);

        int startedRound = -1;
        GameEvents.OnRoundStarted += round => startedRound = round;
        manager.StartMatch();

        AssertEqual(1, manager.CurrentRound, "New match did not start at round one.");
        AssertEqual(0, manager.Player1Score, "Player 1 score was not reset.");
        AssertEqual(0, manager.Player2Score, "Player 2 score was not reset.");
        AssertEqual(PlayerID.None, manager.MatchWinner, "Previous winner was not cleared.");
        AssertEqual(PlayerID.Player1, manager.ServingPlayer, "New match did not assign the first serve to Player 1.");
        AssertEqual(PlayerID.Player1, manager.ActivePlayer, "First active player should be Player 1.");
        AssertEqual(MatchState.Countdown, manager.CurrentState, "New match did not enter countdown.");
        AssertEqual(1, startedRound, "RoundStarted did not report round one.");
    }

    private static void TestCountdownTransition()
    {
        MatchManager manager = CreateManager(CreateConfig(3));
        manager.StartMatch();
        Coroutine countdown = (Coroutine)GetField(manager, "roundTransitionCoroutine");

        AssertEqual(MatchState.Countdown, manager.CurrentState, "StartMatch did not keep the countdown active.");
        AssertTrue(countdown.MoveNext(), "Countdown coroutine ended before its wait.");
        AssertEqual(MatchState.Countdown, manager.CurrentState, "Countdown ended before the wait completed.");
        AssertTrue(!countdown.MoveNext(), "Countdown coroutine should finish after transitioning to Playing.");
        AssertEqual(MatchState.Playing, manager.CurrentState, "Countdown did not transition to Playing.");
    }

    private static void TestSuccessfulHitsAlternatePlayers()
    {
        MatchManager manager = CreatePlayingManager();
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Good, 0.5f);
        AssertEqual(PlayerID.Player2, manager.ActivePlayer, "Player 1 hit did not pass the turn to Player 2.");

        GameEvents.TriggerHitResolved(PlayerID.Player2, HitResult.Perfect, 1f);
        AssertEqual(PlayerID.Player1, manager.ActivePlayer, "Player 2 hit did not pass the turn to Player 1.");
    }

    private static void TestMissResultDoesNotAlternate()
    {
        MatchManager manager = CreatePlayingManager();
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Miss, 0f);
        AssertEqual(PlayerID.Player1, manager.ActivePlayer, "A Miss result unexpectedly changed the active player.");
    }

    private static void TestHitsOutsidePlayingAreIgnored()
    {
        MatchManager manager = CreateManager(CreateConfig(3));
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Perfect, 1f);
        AssertEqual(PlayerID.Player1, manager.ActivePlayer, "Hit changed the turn while the match was Idle.");

        SetField(manager, "currentState", MatchState.RoundOver);
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Good, 0.5f);
        AssertEqual(PlayerID.Player1, manager.ActivePlayer, "Hit changed the turn after the round ended.");
    }

    private static void TestMissAwardsPoint()
    {
        MatchManager manager = CreatePlayingManager();
        PlayerID explodedPlayer = PlayerID.None;
        PlayerID roundWinner = PlayerID.None;
        int finalPlayer1Score = -1;
        int finalPlayer2Score = -1;
        GameEvents.OnPotatoExploded += player => explodedPlayer = player;
        GameEvents.OnRoundEnded += (winner, player1Score, player2Score) =>
        {
            roundWinner = winner;
            finalPlayer1Score = player1Score;
            finalPlayer2Score = player2Score;
        };

        GameEvents.TriggerPlayerMiss(PlayerID.Player1);

        AssertEqual(MatchState.RoundOver, manager.CurrentState, "Miss did not end the round.");
        AssertEqual(0, manager.Player1Score, "Losing player received the point.");
        AssertEqual(1, manager.Player2Score, "Opponent did not receive the point.");
        AssertEqual(PlayerID.Player1, manager.ServingPlayer, "Player who missed did not get the next serve.");
        AssertEqual(2, manager.CurrentRound, "Next round number was not advanced.");
        AssertEqual(PlayerID.Player1, explodedPlayer, "Explosion event did not identify the player who missed.");
        AssertEqual(PlayerID.Player2, roundWinner, "RoundEnded did not report the opponent as winner.");
        AssertEqual(0, finalPlayer1Score, "RoundEnded reported the wrong Player 1 score.");
        AssertEqual(1, finalPlayer2Score, "RoundEnded reported the wrong Player 2 score.");
    }

    private static void TestPlayerOneWinsMatch()
    {
        MatchManager manager = CreatePlayingManager(CreateConfig(1));
        PlayerID matchWinner = PlayerID.None;
        GameEvents.OnMatchEnded += player => matchWinner = player;
        GameEvents.TriggerPlayerMiss(PlayerID.Player2);

        AssertEqual(1, manager.Player1Score, "Player 1 did not receive the winning point.");
        AssertEqual(MatchState.GameOver, manager.CurrentState, "Winning point did not end the match.");
        AssertEqual(PlayerID.Player1, manager.MatchWinner, "MatchManager stored the wrong winner.");
        AssertEqual(PlayerID.Player1, matchWinner, "MatchEnded reported the wrong winner.");
        AssertEqual(1, manager.CurrentRound, "MatchManager advanced the round after game over.");
    }

    private static void TestPlayerTwoWinsMatch()
    {
        MatchManager manager = CreatePlayingManager(CreateConfig(1));
        PlayerID matchWinner = PlayerID.None;
        GameEvents.OnMatchEnded += player => matchWinner = player;
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);

        AssertEqual(1, manager.Player2Score, "Player 2 did not receive the winning point.");
        AssertEqual(MatchState.GameOver, manager.CurrentState, "Winning point did not end the match.");
        AssertEqual(PlayerID.Player2, manager.MatchWinner, "MatchManager stored the wrong winner.");
        AssertEqual(PlayerID.Player2, matchWinner, "MatchEnded reported the wrong winner.");
    }

    private static void TestRepeatedMissIsIgnored()
    {
        MatchManager manager = CreatePlayingManager();
        int explosions = 0;
        GameEvents.OnPotatoExploded += player => explosions++;

        GameEvents.TriggerPlayerMiss(PlayerID.Player1);
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);

        AssertEqual(1, manager.Player2Score, "Repeated miss awarded more than one point.");
        AssertEqual(1, explosions, "Repeated miss triggered more than one explosion.");
    }

    private static void TestOnDisableUnsubscribes()
    {
        MatchManager manager = CreatePlayingManager();
        Invoke(manager, "OnDisable");
        GameEvents.TriggerHitResolved(PlayerID.Player1, HitResult.Perfect, 1f);
        GameEvents.TriggerPlayerMiss(PlayerID.Player1);

        AssertEqual(PlayerID.Player1, manager.ActivePlayer, "Disabled manager still handled hit events.");
        AssertEqual(0, manager.Player2Score, "Disabled manager still handled miss events.");
    }

    private static void TestMatchReset()
    {
        MatchManager manager = CreateManager(CreateConfig(3));
        SetField(manager, "player1Score", 1);
        SetField(manager, "player2Score", 2);
        SetField(manager, "matchWinner", PlayerID.Player2);
        SetField(manager, "currentState", MatchState.GameOver);

        GameEvents.TriggerMatchReset();

        AssertEqual(1, manager.CurrentRound, "Match reset did not return to round one.");
        AssertEqual(0, manager.Player1Score, "Match reset did not clear Player 1 score.");
        AssertEqual(0, manager.Player2Score, "Match reset did not clear Player 2 score.");
        AssertEqual(PlayerID.None, manager.MatchWinner, "Match reset did not clear the winner.");
        AssertEqual(PlayerID.Player1, manager.ServingPlayer, "Match reset did not restore the opening serve.");
        AssertEqual(MatchState.Countdown, manager.CurrentState, "Match reset did not begin a new countdown.");
    }

    private static MatchManager CreatePlayingManager()
    {
        return CreatePlayingManager(CreateConfig(3));
    }

    private static MatchManager CreatePlayingManager(GameConfig config)
    {
        MatchManager manager = CreateManager(config);
        SetField(manager, "currentState", MatchState.Playing);
        return manager;
    }

    private static MatchManager CreateManager(GameConfig config)
    {
        MatchManager manager = new MatchManager();
        manager.config = config;
        Invoke(manager, "Awake");
        Invoke(manager, "OnEnable");
        return manager;
    }

    private static GameConfig CreateConfig(int roundsToWin)
    {
        GameConfig config = ScriptableObject.CreateInstance<GameConfig>();
        config.roundsToWinMatch = roundsToWin;
        return config;
    }

    private static void ResetSingleton()
    {
        PropertyInfo property = typeof(MatchManager).GetProperty("Instance");
        property.GetSetMethod(true).Invoke(null, new object[] { null });
    }

    private static object GetField(object instance, string name)
    {
        return typeof(MatchManager).GetField(name, InstanceFields).GetValue(instance);
    }

    private static void SetField(object instance, string name, object value)
    {
        typeof(MatchManager).GetField(name, InstanceFields).SetValue(instance, value);
    }

    private static void Invoke(object instance, string methodName)
    {
        typeof(MatchManager).GetMethod(methodName, InstanceFields).Invoke(instance, null);
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!object.Equals(expected, actual))
        {
            throw new InvalidOperationException(message + " Expected: " + expected + "; actual: " + actual + ".");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
