import re

with open("Party Potato/Assets/Scripts/Core/MatchManager.cs", "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace("private Coroutine roundTransitionCoroutine;", "private Coroutine roundTransitionCoroutine;\n    private int beatsElapsedInCountdown = 0;")
content = content.replace("GameEvents.OnMatchReset += HandleMatchReset;", "GameEvents.OnMatchReset += HandleMatchReset;\n        GameEvents.OnBeat += HandleBeat;")
content = content.replace("GameEvents.OnMatchReset -= HandleMatchReset;", "GameEvents.OnMatchReset -= HandleMatchReset;\n        GameEvents.OnBeat -= HandleBeat;")

handle_beat = """
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
"""

# Find StartMatch signature
content = content.replace("public void StartMatch()", handle_beat.strip() + "\n\n    /// <summary>\n    /// Inicia la partida desde la ronda 1 con marcadores en cero.\n    /// </summary>\n    public void StartMatch()")

# Remove the old IEnumerator logic
content = re.sub(r'roundTransitionCoroutine = StartCoroutine\(CountdownRoutine\(\)\);.*?private IEnumerator CountdownRoutine\(\)\s*\{.*?\}', '', content, flags=re.DOTALL)

# Add initialization
content = content.replace("currentState = MatchState.Countdown;", "currentState = MatchState.Countdown;\n        beatsElapsedInCountdown = 0;")

with open("Party Potato/Assets/Scripts/Core/MatchManager.cs", "w", encoding="utf-8") as f:
    f.write(content)
