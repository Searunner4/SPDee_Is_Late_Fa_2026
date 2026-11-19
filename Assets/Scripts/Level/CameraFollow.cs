using UnityEngine;

/// <summary>Smoothly follows the player, looking a little ahead of them.</summary>
[DefaultExecutionOrder(50)]
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 offset = new Vector2(3f, 1.5f);
    public float smoothTime = 0.15f;
    [Tooltip("The camera never goes lower than this, so falling into a pit doesn't drag the view down.")]
    public float minY = 3f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = new Vector3(
            target.position.x + offset.x,
            Mathf.Max(target.position.y + offset.y, minY),
            transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}
