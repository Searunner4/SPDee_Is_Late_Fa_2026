using UnityEngine;

/// <summary>
/// TEMPORARY stand-in so levels can be tested before Jacob's player is ready.
/// Move: A/D or arrow keys. Jump: Space.
/// Once Jacob's player exists, delete this object from the level and drop his player in
/// (tag "Player", Rigidbody2D, Collider2D), then point the LevelManager and CameraFollow at it.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlaceholderPlayer : MonoBehaviour
{
    public float baseSpeed = 7f;
    public float moveSpeed = 7f;
    public float jumpSpeed = 14f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    Collider2D col;
    float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        if(Input.GetButton("Sprint"))
        {
            moveSpeed = baseSpeed * 1.5f;
        }
        else {
            moveSpeed = baseSpeed;
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.05f);
        Vector2 origin = new Vector2(b.center.x, b.min.y);
        return Physics2D.BoxCast(origin, size, 0f, Vector2.down, 0.1f, groundLayer);
    }
}
