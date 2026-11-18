using UnityEngine;

/// <summary>Trigger that saves a respawn point. Swaps sprite once activated (e.g. a signpost or mushroom).</summary>
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform respawnPosition;   // optional; defaults to this object
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite activeSprite;

    bool activated;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player") || LevelManager.Instance == null) return;
        activated = true;

        Vector3 pos = respawnPosition != null ? respawnPosition.position : transform.position;
        LevelManager.Instance.SetCheckpoint(pos);

        if (spriteRenderer != null && activeSprite != null)
            spriteRenderer.sprite = activeSprite;
    }
}
