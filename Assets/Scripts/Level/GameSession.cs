/// <summary>
/// Totals for the whole run (all levels), shown on the Game Over screen.
/// Simple stand-in until Ethan's timer exists; he can replace or extend this.
/// </summary>
public static class GameSession
{
    public static float TotalTime { get; private set; }
    public static int TotalDeaths { get; private set; }

    public static void Reset()
    {
        TotalTime = 0f;
        TotalDeaths = 0;
    }

    public static void AddLevel(float time, int deaths)
    {
        TotalTime += time;
        TotalDeaths += deaths;
    }

    public static string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60f);
        return $"{minutes}:{seconds % 60f:00.00}";
    }
}
