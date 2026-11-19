using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the run's total time and falls on the Game Over screen.</summary>
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] Text statsText;

    void Start()
    {
        if (statsText == null) return;
        statsText.text =
            $"Time: {GameSession.FormatTime(GameSession.TotalTime)}\n" +
            $"Falls: {GameSession.TotalDeaths}";
    }
}
