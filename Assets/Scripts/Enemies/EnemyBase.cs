using UnityEngine;

/// <summary>
/// Shared enemy behaviour: touching the player hurts them, unless the player lands on top
/// (stomp), which defeats the enemy and bounces the player up.
/// Uncheck "Can Be Stomped" for spiky enemies like the hedgehog.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] protected bool canBeStomped = true;
    [SerializeField] protected float stompBounce = 12f;

    protected Collider2D col;
    protected bool isDead;

    protected virtual void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision) => HandleContact(collision.collider);
    protected virtual void OnTriggerEnter2D(Collider2D other) => HandleContact(other);

    void HandleContact(Collider2D other)
    {
        if (isDead || !other.CompareTag("Player")) return;

        Rigidbody2D playerRb = other.attachedRigidbody;
        // Player's feet are above the enemy's middle and they aren't moving upward = stomp.
        bool landedOnTop = playerRb != null
                           && playerRb.velocity.y <= 0.1f
                           && other.bounds.min.y > col.bounds.center.y;

        if (canBeStomped && landedOnTop)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, stompBounce);
            Die();
        }
        else if (LevelManager.Instance != null)
        {
            LevelManager.Instance.PlayerHit();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Simple "squash" so it reads as defeated even without an animation.
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * 0.3f, 1f);
        Destroy(gameObject, 0.4f);
    }
}
