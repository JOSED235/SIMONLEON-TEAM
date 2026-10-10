using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador principal del ciclo de partida, rondas, turnos y condiciones de victoria.
/// Administra el flujo entre estados (Countdown -> Playing -> RoundOver -> GameOver).
/// </summary>
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Header("Configuración de Balance")]
    [Tooltip("Asset de configuración GameConfig con tolerancias y reglas de victoria.")]
    public GameConfig config;

    [Header("Estado de la Partida")]
    [SerializeField] private MatchState currentState = MatchState.Idle;
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int player1Score = 0;
    [SerializeField] private int player2Score = 0;
    [SerializeField] private PlayerID activePlayer = PlayerID.Player1;
    [SerializeField] private PlayerID servingPlayer = PlayerID.Player1;
    [SerializeField] private PlayerID matchWinner = PlayerID.None;

    public MatchState CurrentState => currentState;
    public int CurrentRound => currentRound;
    public int Player1Score => player1Score;
    public int Player2Score => player2Score;
    public PlayerID ActivePlayer => activePlayer;
    public PlayerID ServingPlayer => servingPlayer;
    public PlayerID MatchWinner => matchWinner;

    private Coroutine roundTransitionCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Crear configuración por defecto en memoria si no se asignó en el Inspector
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<GameConfig>();
            Debug.LogWarning("[MatchManager] No se asignó GameConfig en el Inspector. Usando valores predeterminados.");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnHitResolved += HandleHitResolved;
        GameEvents.OnPlayerMiss += HandlePlayerMiss;
        GameEvents.OnMatchReset += HandleMatchReset;
    }

    private void OnDisable()
    {
        GameEvents.OnHitResolved -= HandleHitResolved;
        GameEvents.OnPlayerMiss -= HandlePlayerMiss;
        GameEvents.OnMatchReset -= HandleMatchReset;
    }

    private void Start()
    {
        // Iniciar la partida automáticamente al cargar la escena
        StartMatch();
    }

    /// <summary>
    /// Inicia la partida desde la ronda 1 con marcadores en cero.
    /// </summary>
    public void StartMatch()
    {
        currentRound = 1;
        player1Score = 0;
        player2Score = 0;
        matchWinner = PlayerID.None;
        servingPlayer = PlayerID.Player1;
        activePlayer = servingPlayer;

        StartRound();
    }

    /// <summary>
    /// Inicia una nueva ronda, notificando a los sistemas mediante GameEvents.
    /// </summary>
    public void StartRound()
    {
        if (roundTransitionCoroutine != null)
        {
            StopCoroutine(roundTransitionCoroutine);
            roundTransitionCoroutine = null;
        }

        currentState = MatchState.Countdown;
        activePlayer = servingPlayer;

        Debug.Log($"[MatchManager] 🥊 Ronda {currentRound} iniciando. Saque para: {servingPlayer}");
        GameEvents.TriggerRoundStarted(currentRound);

        // Iniciar la cuenta regresiva antes de habilitar la jugabilidad
        roundTransitionCoroutine = StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        // Pausa breve simulando conteo o anticipación de saque
        yield return new WaitForSeconds(1.5f);

        currentState = MatchState.Playing;
        Debug.Log($"[MatchManager] 🚀 ¡Partida en curso! Turno de impacto: {activePlayer}");
    }

    /// <summary>
    /// Gestiona el intercambio cuando un jugador acierta el golpe.
    /// </summary>
    private void HandleHitResolved(PlayerID player, HitResult result, float accuracy)
    {
        if (currentState != MatchState.Playing) return;

        if (result != HitResult.Miss)
        {
            // Alternar el turno al jugador contrario
            activePlayer = (player == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;
            Debug.Log($"[MatchManager] Golpe acertado ({result}). Próximo turno: {activePlayer}");
        }
    }

    /// <summary>
    /// Gestiona el fallo del jugador activo (explosión de la papa y fin de ronda).
    /// Regla: Cada llegada se resuelve una sola vez.
    /// </summary>
    private void HandlePlayerMiss(PlayerID missingPlayer)
    {
        if (currentState != MatchState.Playing) return;

        currentState = MatchState.RoundOver;

        // El jugador rival se lleva el punto de la ronda
        PlayerID roundWinner = (missingPlayer == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;

        if (roundWinner == PlayerID.Player1) player1Score++;
        else if (roundWinner == PlayerID.Player2) player2Score++;

        Debug.Log($"[MatchManager] 💥 Fallo de {missingPlayer}! Ronda para {roundWinner}. Marcador: P1 [{player1Score}] - P2 [{player2Score}]");

        // Disparar eventos de explosión y fin de ronda
        GameEvents.TriggerPotatoExploded(missingPlayer);
        GameEvents.TriggerRoundEnded(roundWinner, player1Score, player2Score);

        // El que falló obtiene el saque de la siguiente ronda
        servingPlayer = missingPlayer;

        // Evaluar condición de victoria definitiva
        if (player1Score >= config.roundsToWinMatch)
        {
            EndMatch(PlayerID.Player1);
        }
        else if (player2Score >= config.roundsToWinMatch)
        {
            EndMatch(PlayerID.Player2);
        }
        else
        {
            // Pasar a la siguiente ronda tras una breve espera para apreciar la explosión
            currentRound++;
            roundTransitionCoroutine = StartCoroutine(WaitAndStartNextRoundRoutine());
        }
    }

    private IEnumerator WaitAndStartNextRoundRoutine()
    {
        yield return new WaitForSeconds(config.delayBeforeNextRound);
        StartRound();
    }

    /// <summary>
    /// Declara al ganador definitivo de la partida.
    /// </summary>
    private void EndMatch(PlayerID winner)
    {
        currentState = MatchState.GameOver;
        matchWinner = winner;

        Debug.Log($"[MatchManager] 🏆 ¡VICTORIA DEFINITIVA para {matchWinner}!");
        GameEvents.TriggerMatchEnded(matchWinner);
    }

    /// <summary>
    /// Reinicia la partida desde cero (ej. botón de Revancha en UI).
    /// </summary>
    public void RestartMatch()
    {
        GameEvents.TriggerMatchReset();
    }

    private void HandleMatchReset()
    {
        StartMatch();
    }
}
