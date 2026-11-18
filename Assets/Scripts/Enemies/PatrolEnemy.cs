using UnityEngine;

/// <summary>
/// Walks back and forth, turning around at ledges and walls. (Beetle, hedgehog, etc.)
/// Setup: Rigidbody2D (Dynamic, Freeze Rotation Z), a collider, and a child "GroundCheck"
/// placed at the front foot, slightly ahead of the body. The enemy must NOT be on the Ground layer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PatrolEnemy : EnemyBase
{
    [Header("Movement")]
    [SerializeField] float speed = 2f;
    [SerializeField] bool startFacingRight = false;
    [Tooltip("Set this if your sprite art faces left by default.")]
    [SerializeField] bool spriteFacesLeft = false;

    [Header("Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.5f;
    [SerializeField] float wallCheckDistance = 0.1f;

    Rigidbody2D rb;
    int direction;
    float lastFlipTime;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        direction = startFacingRight ? 1 : -1;
        ApplyFacing();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        rb.velocity = new Vector2(direction * speed, rb.velocity.y);

        if (groundCheck == null || Time.time - lastFlipTime < 0.2f) return;

        bool groundAhead = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);

        Vector2 wallOrigin = col.bounds.center;
        float wallDistance = col.bounds.extents.x + wallCheckDistance;
        bool wallAhead = Physics2D.Raycast(wallOrigin, Vector2.right * direction, wallDistance, groundLayer);

        if (!groundAhead || wallAhead) Flip();
    }

    void Flip()
    {
        direction = -direction;
        lastFlipTime = Time.time;
        ApplyFacing();
    }

    void ApplyFacing()
    {
        // Flipping the whole object also moves the GroundCheck child to the new front.
        float facing = spriteFacesLeft ? -direction : direction;
        Vector3 s = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(s.x) * facing, s.y, s.z);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
    }
}
