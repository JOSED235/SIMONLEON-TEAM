using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador principal del ciclo de partida, rondas y condiciones de victoria para Beat Potato.
/// Administra el flujo de estados (Countdown -> Playing -> RoundOver -> GameOver)
/// y resuelve las rondas cuando la papa estalla al alcanzar el 100% de calor.
/// </summary>
public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Header("Configuración de Balance")]
    [Tooltip("Asset de configuración GameConfig con tolerancias y reglas de victoria.")]
    public GameConfig config;

    [Header("Modo de Juego")]
    [SerializeField] private GameMode gameMode = GameMode.LocalPVP;

    [Header("Estado de la Partida")]
    [SerializeField] private MatchState currentState = MatchState.Idle;
    [SerializeField] private int currentRound = 1;
    [SerializeField] private int player1Score = 0;
    [SerializeField] private int player2Score = 0;
    [SerializeField] private PlayerID matchWinner = PlayerID.None;

    public MatchState CurrentState { get { return currentState; } }
    public int CurrentRound { get { return currentRound; } }
    public int Player1Score { get { return player1Score; } }
    public int Player2Score { get { return player2Score; } }
    public PlayerID MatchWinner { get { return matchWinner; } }
    public GameMode CurrentGameMode { get { return gameMode; } set { gameMode = value; } }

    private Coroutine roundTransitionCoroutine;
    private int beatsElapsedInCountdown = 0;

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
        GameEvents.OnPotatoExploded += HandlePotatoExploded;
        GameEvents.OnMatchReset += HandleMatchReset;
        GameEvents.OnBeat += HandleBeat;
    }

    private void OnDisable()
    {
        GameEvents.OnPotatoExploded -= HandlePotatoExploded;
        GameEvents.OnMatchReset -= HandleMatchReset;
        GameEvents.OnBeat -= HandleBeat;
    }

    private void Start()
    {
        // Iniciar la partida automáticamente al cargar la escena
        StartMatch();
    }

    private void HandleBeat(int beatIndex)
    {
        if (currentState == MatchState.Countdown)
        {
            beatsElapsedInCountdown++;
            int requiredBeats = (config != null) ? config.countdownBeats : 4;
            
            if (beatsElapsedInCountdown >= requiredBeats)
            {
                currentState = MatchState.Playing;
                Debug.Log("[MatchManager] ¡Partida en curso! Ambos jugadores en ritmo.");
            }
        }
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
        beatsElapsedInCountdown = 0;

        int waitBeats = (config != null) ? config.countdownBeats : 4;
        Debug.Log(string.Format("[MatchManager] Ronda {0} iniciando. Esperando {1} beats...", currentRound, waitBeats));
        GameEvents.TriggerRoundStarted(currentRound);
    }

    /// <summary>
    /// Resuelve el fin de ronda cuando la papa estalla (100% de calor acumulado).
    /// Regla oficial: Quien recibe la explosión pierde la ronda; el rival suma punto.
    /// </summary>
    private void HandlePotatoExploded(PlayerID loser)
    {
        if (currentState != MatchState.Playing) return;

        currentState = MatchState.RoundOver;

        // El jugador contrario gana la ronda
        PlayerID roundWinner = (loser == PlayerID.Player1) ? PlayerID.Player2 : PlayerID.Player1;

        if (roundWinner == PlayerID.Player1) player1Score++;
        else if (roundWinner == PlayerID.Player2) player2Score++;

        Debug.Log(string.Format("[MatchManager] ¡Explosión en campo de {0}! Ronda para {1}. Marcador: P1 [{2}] - P2 [{3}]", loser, roundWinner, player1Score, player2Score));

        GameEvents.TriggerRoundEnded(roundWinner, player1Score, player2Score);

        // Evaluar condición de victoria definitiva
        int roundsNeeded = (config != null) ? config.roundsToWinMatch : 2;
        if (player1Score >= roundsNeeded)
        {
            EndMatch(PlayerID.Player1);
        }
        else if (player2Score >= roundsNeeded)
        {
            EndMatch(PlayerID.Player2);
        }
        else
        {
            // Pasar a la siguiente ronda tras breve pausa
            currentRound++;
            roundTransitionCoroutine = StartCoroutine(WaitAndStartNextRoundRoutine());
        }
    }

    private IEnumerator WaitAndStartNextRoundRoutine()
    {
        float delay = (config != null) ? config.delayBeforeNextRound : 2.5f;
        yield return new WaitForSeconds(delay);
        StartRound();
    }

    /// <summary>
    /// Declara al ganador definitivo de la partida.
    /// </summary>
    private void EndMatch(PlayerID winner)
    {
        currentState = MatchState.GameOver;
        matchWinner = winner;

        Debug.Log(string.Format("[MatchManager] ¡VICTORIA DEFINITIVA para {0}!", matchWinner));
        GameEvents.TriggerMatchEnded(matchWinner);
    }

    /// <summary>
    /// Reinicia la partida completa (ej. botón de revancha en pantalla final).
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
