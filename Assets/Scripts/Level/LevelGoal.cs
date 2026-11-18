using UnityEngine;

/// <summary>End of the level (the path out of the forest / the office door).</summary>
[RequireComponent(typeof(Collider2D))]
public class LevelGoal : MonoBehaviour
{
    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && LevelManager.Instance != null)
            LevelManager.Instance.CompleteLevel();
    }
}
