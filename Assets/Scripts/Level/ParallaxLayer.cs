using UnityEngine;

/// <summary>
/// Moves a background layer along with the camera by a fraction of the camera's movement.
/// Factor 1 = stuck to the camera (far sky), 0 = doesn't move (same as the level).
/// </summary>
[DefaultExecutionOrder(100)] // after CameraFollow, so there's no jitter
public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)] public float factor = 0.5f;

    Transform cam;
    Vector3 camStart;
    Vector3 startPos;

    void Start()
    {
        if (Camera.main == null) { enabled = false; return; }
        cam = Camera.main.transform;
        camStart = cam.position;
        startPos = transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - camStart;
        transform.position = startPos + new Vector3(delta.x * factor, delta.y * factor, 0f);
    }
}
