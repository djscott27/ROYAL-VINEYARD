using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BarrelManager : MonoBehaviour
{
    public const int MaxBarrels = 5;

    public static BarrelManager Instance;
    GameObject[] barells = new GameObject[MaxBarrels];
    public GameObject barrelPrefab;
    public int numBarrels = 0;
    public float minX;
    public float maxX;
    public GameObject rect;

    [Header("Gizmos")]
    public float gizmoRadius = 0.25f;
    public Color rectColor = new Color(0f, 0.7f, 1f, 1f);
    public Color emptySlotColor = new Color(1f, 0.8f, 0f, 1f);
    public Color filledSlotColor = new Color(0.2f, 1f, 0.3f, 1f);

    void Start()
    {
        Instance = this;
        InstanceBarell();
    }

    public void InstanceBarell()
    {
        for (int i = 0; i < MaxBarrels; i++)
        {
            if (barells[i] == null)
            {
                // Only spawn once we know there's a free slot
                var b = Instantiate(barrelPrefab, transform, false);
                b.transform.localPosition = GetSlotLocalPosition(i);
                barells[i] = b;
                numBarrels += 1;
                break;
            }
        }
    }

    // Single source of truth for slot positions (used by spawning AND gizmos)
    public Vector3 GetSlotLocalPosition(int i)
    {
        var halfHeight = rect.transform.localScale.y / 2f;
        var t = (float)i / (MaxBarrels - 1);
        var y = rect.transform.localPosition.y + Mathf.Lerp(-halfHeight, halfHeight, t);
        var x = rect.transform.localPosition.x;
        return new Vector3(x, y, 0);
    }

    void OnDrawGizmos()
    {
        if (rect == null) return;

        // Spawn area, drawn in this object's local space (same space barrels are placed in)
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = rectColor;
        var r = rect.transform;
        Gizmos.DrawWireCube(
            new Vector3(r.localPosition.x, r.localPosition.y, 0),
            new Vector3(r.localScale.x, r.localScale.y, 0));

        // Slots, drawn in world space so spheres aren't squashed by scale
        Gizmos.matrix = Matrix4x4.identity;
        for (int i = 0; i < MaxBarrels; i++)
        {
            Vector3 world = transform.TransformPoint(GetSlotLocalPosition(i));
            bool filled = Application.isPlaying && barells[i] != null;

            Gizmos.color = filled ? filledSlotColor : emptySlotColor;
            if (filled) Gizmos.DrawSphere(world, gizmoRadius);
            else Gizmos.DrawWireSphere(world, gizmoRadius);

#if UNITY_EDITOR
            Handles.color = Gizmos.color;
            Handles.Label(world + Vector3.right * gizmoRadius * 1.5f, "Barrel " + i);
#endif
        }
    }
}