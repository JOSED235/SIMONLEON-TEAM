using System;

public static class GameEnumsTests
{
    public static int RunAll()
    {
        AssertEnumValues(typeof(PlayerID), new[] { "None=0", "Player1=1", "Player2=2" });
        AssertEnumValues(typeof(HitResult), new[] { "Miss=0", "Good=1", "Great=2", "Perfect=3" });
        AssertEnumValues(typeof(LaneDirection), new[] { "Left=0", "Down=1", "Up=2", "Right=3" });
        AssertEnumValues(typeof(GameMode), new[] { "LocalPVP=0", "SoloVsCPU=1" });
        AssertEnumValues(typeof(AIDifficulty), new[] { "Easy=0", "Normal=1", "Hard=2" });
        AssertEnumValues(typeof(PowerType), new[]
        {
            "None=0",
            "RocketPotato=1",
            "GhostPotato=2",
            "ElectricPotato=3",
            "FreezePotato=4",
            "GoldenPotato=5"
        });
        AssertEnumValues(typeof(MatchState), new[]
        {
            "Idle=0",
            "Countdown=1",
            "Playing=2",
            "RoundOver=3",
            "GameOver=4"
        });
        AssertEnumValues(typeof(PotatoState), new[]
        {
            "Idle=0",
            "Traveling=1",
            "InHitZone=2",
            "HitResolved=3",
            "Exploded=4"
        });

        Console.WriteLine("[PASS] Enum names and numeric values");
        return 1;
    }

    private static void AssertEnumValues(Type enumType, string[] expected)
    {
        string[] names = Enum.GetNames(enumType);
        Array values = Enum.GetValues(enumType);

        if (names.Length != expected.Length)
        {
            throw new InvalidOperationException(
                enumType.Name + " has " + names.Length + " values; expected " + expected.Length + ".");
        }

        for (int i = 0; i < names.Length; i++)
        {
            string actual = names[i] + "=" + Convert.ToInt32(values.GetValue(i));
            if (actual != expected[i])
            {
                throw new InvalidOperationException(
                    enumType.Name + " value " + i + " is " + actual + "; expected " + expected[i] + ".");
            }
        }
    }
}
