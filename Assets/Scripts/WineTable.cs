using UnityEngine;

public class WineTable : MonoBehaviour
{
    static WineTable instance;
    void Awake()
    {
        instance = this;
    }

    [SerializeField] private float xRange = 2;
    [SerializeField] private float localSpawnHeight = 0.5f;

    void OnDrawGizmosSelected()
    {
        // draw spawn line
        Gizmos.color = Color.purple;
        Gizmos.DrawWireCube(transform.position + Vector3.up * localSpawnHeight, new Vector3(xRange, 0.2f));
    }

    public static Vector2 GetSpawnPosition()
    {
        float halfX = instance.xRange / 2f;
        Vector2 center = instance.transform.position + Vector3.up * instance.localSpawnHeight;
        center.x = center.x + Random.Range(-halfX, halfX);
        return center;
    }
}
