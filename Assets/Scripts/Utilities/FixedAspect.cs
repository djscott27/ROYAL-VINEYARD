using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspect : MonoBehaviour
{
    public float targetAspectX = 16f;
    public float targetAspectY = 9f;

    void Start() => UpdateAspect();

    void Update() => UpdateAspect();

    void UpdateAspect()
    {
        float targetAspect = targetAspectX / targetAspectY;
        float windowAspect = (float)Screen.width / Screen.height;

        Camera cam = GetComponent<Camera>();
        Rect rect = cam.rect;

        if (windowAspect > targetAspect)
        {
            // window too wide → pillarbox
            float scale = targetAspect / windowAspect;
            rect.width = scale;
            rect.height = 1f;
            rect.x = (1f - scale) * 0.5f;
            rect.y = 0f;
        }
        else
        {
            // window too tall → letterbox
            float scale = windowAspect / targetAspect;
            rect.width = 1f;
            rect.height = scale;
            rect.x = 0f;
            rect.y = (1f - scale) * 0.5f;
        }

        cam.rect = rect;
    }
}
