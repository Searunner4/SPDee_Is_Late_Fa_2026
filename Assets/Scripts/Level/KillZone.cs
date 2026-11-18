using UnityEngine;

/// <summary>
/// Trigger that sends the player back to the last checkpoint.
/// Use a wide one under the level for falling, or small ones for thorn bushes, spikes, water, etc.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && LevelManager.Instance != null)
            LevelManager.Instance.PlayerHit();
    }
}
