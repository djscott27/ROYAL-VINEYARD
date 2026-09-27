using UnityEngine;

[ExecuteAlways]
public class DrawCameraBounds : MonoBehaviour
{
    public Color color = Color.cyan;

    void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        Vector3 center = transform.position;

        Vector3 topLeft     = center + Vector3.up * (height / 2f) - Vector3.right * (width / 2f);
        Vector3 topRight    = center + Vector3.up * (height / 2f) + Vector3.right * (width / 2f);
        Vector3 bottomLeft  = center - Vector3.up * (height / 2f) - Vector3.right * (width / 2f);
        Vector3 bottomRight = center - Vector3.up * (height / 2f) + Vector3.right * (width / 2f);

        Gizmos.color = color;
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
