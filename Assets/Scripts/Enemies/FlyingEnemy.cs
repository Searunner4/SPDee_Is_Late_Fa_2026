using UnityEngine;

/// <summary>
/// Flies back and forth between its start point and an offset, with a gentle bob. (Bees, birds, fireflies.)
/// Setup: Rigidbody2D set to Kinematic, a collider. Drag the gizmo endpoint by editing "Travel Offset".
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : EnemyBase
{
    [Header("Path")]
    [SerializeField] Vector2 travelOffset = new Vector2(4f, 0f);
    [SerializeField] float travelSpeed = 1.5f;

    [Header("Bob")]
    [SerializeField] float bobHeight = 0.3f;
    [SerializeField] float bobSpeed = 3f;

    Rigidbody2D rb;
    Vector2 startPos;
    float timeOffset;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        startPos = transform.position;
        timeOffset = Random.Range(0f, 10f); // so a group of bees doesn't move in sync
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float distance = travelOffset.magnitude;
        if (distance < 0.01f) distance = 0.01f;

        float t = Mathf.PingPong((Time.time + timeOffset) * travelSpeed / distance, 1f);
        Vector2 target = startPos + travelOffset * t;
        target.y += Mathf.Sin((Time.time + timeOffset) * bobSpeed) * bobHeight;

        // Face the direction of travel.
        float dx = target.x - rb.position.x;
        if (Mathf.Abs(dx) > 0.001f)
        {
            Vector3 s = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(s.x) * Mathf.Sign(dx), s.y, s.z);
        }

        rb.MovePosition(target);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 from = Application.isPlaying ? (Vector3)startPos : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(from, from + (Vector3)travelOffset);
        Gizmos.DrawWireSphere(from + (Vector3)travelOffset, 0.2f);
    }
}
