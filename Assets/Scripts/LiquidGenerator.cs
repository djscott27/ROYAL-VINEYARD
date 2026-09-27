using System.Collections.Generic;
using MEC;
using UnityEngine;

public class LiquidGenerator : MonoBehaviour
{
    [Header("Droplet")]
    [SerializeField] [Range(0, 1)] private float dropletSize = 0.3f;
    [SerializeField] private Color liquidColor;
    [Tooltip("Seconds before each droplet is destroyed. 0 = never.")]
    [SerializeField] [Min(0f)] private float dropletLifetime = 0.5f;

    [Header("Volume")]
    [SerializeField] private Vector2 volumeSize;

    [Header("Stream")]
    [Tooltip("Pour the droplets in over time from a single point instead of filling the volume instantly.")]
    [SerializeField] private bool spawnAsStream = false;
    [Tooltip("Seconds between spawn batches.")]
    [SerializeField] [Min(0f)] private float spawnInterval = 0.02f;
    [Tooltip("Droplets spawned per batch.")]
    [SerializeField] [Min(1)] private int dropletsPerBatch = 1;
    [Tooltip("Offset from the top-centre of the volume where the stream starts.")]
    [SerializeField] private Vector2 streamOffset = Vector2.zero;
    [Tooltip("Initial velocity given to each droplet (needs a Rigidbody2D on the prefab).")]
    [SerializeField] private Vector2 streamVelocity = new Vector2(0f, -2f);
    [Tooltip("Random horizontal jitter so droplets don't stack perfectly.")]
    [SerializeField] [Min(0f)] private float streamSpread = 0.05f;

    private CoroutineHandle streamHandle;

    public void GenerateLiquid()
    {
        GameObject rawDroplet = Resources.Load<GameObject>("LiquidDroplet");
        if (rawDroplet == null)
        {
            Debug.LogError("LiquidGenerator: couldn't find 'LiquidDroplet' in Resources.", this);
            return;
        }

        int dropletsRequired = Mathf.RoundToInt(
            volumeSize.x * volumeSize.y / (Mathf.PI * Mathf.Pow(dropletSize, 2))
        );
        if (dropletsRequired <= 0) return;

        if (spawnAsStream)
        {
            // Stop any stream that's already running before starting a new one.
            Timing.KillCoroutines(streamHandle);
            streamHandle = Timing.RunCoroutine(
                _StreamLiquid(rawDroplet, dropletsRequired).CancelWith(gameObject)
            );
        }
        else
        {
            SpawnGrid(rawDroplet, dropletsRequired);
        }
    }

    public void StopStream()
    {
        Timing.KillCoroutines(streamHandle);
    }

    private void SpawnGrid(GameObject rawDroplet, int dropletsRequired)
    {
        int cols = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(dropletsRequired * (volumeSize.x / volumeSize.y))));
        int rows = Mathf.Max(1, dropletsRequired / cols);

        float cellWidth = volumeSize.x / cols;
        float cellHeight = volumeSize.y / rows;

        Vector3 origin = transform.position + new Vector3(
            -volumeSize.x * 0.5f + cellWidth * 0.5f,
            -volumeSize.y * 0.5f + cellHeight * 0.5f,
            0f
        );

        for (int xi = 0; xi < cols; xi++)
        {
            for (int yi = 0; yi < rows; yi++)
            {
                SpawnDroplet(rawDroplet, new Vector3(
                    origin.x + xi * cellWidth,
                    origin.y + yi * cellHeight,
                    transform.position.z
                ));
            }
        }
    }

    private IEnumerator<float> _StreamLiquid(GameObject rawDroplet, int total)
    {
        int spawned = 0;

        while (spawned < total)
        {
            // Recalculated each batch so the stream follows the generator if it moves.
            Vector3 streamOrigin = GetStreamOrigin();
            int batch = Mathf.Min(dropletsPerBatch, total - spawned);

            for (int i = 0; i < batch; i++)
            {
                Vector3 pos = streamOrigin + new Vector3(Random.Range(-streamSpread, streamSpread), 0f, 0f);
                GameObject droplet = SpawnDroplet(rawDroplet, pos);

                Rigidbody2D rb = droplet.GetComponentInChildren<Rigidbody2D>();
                if (rb != null)
                {
#if UNITY_6000_0_OR_NEWER
                    rb.linearVelocity = streamVelocity;
#else
                    rb.velocity = streamVelocity;
#endif
                }
            }

            spawned += batch;
            yield return Timing.WaitForSeconds(spawnInterval);
        }
    }

    private GameObject SpawnDroplet(GameObject rawDroplet, Vector3 position)
    {
        GameObject newDroplet = Instantiate(rawDroplet, position, Quaternion.identity, transform);
        newDroplet.transform.localScale = Vector3.one * dropletSize;
        newDroplet.GetComponentInChildren<SpriteRenderer>().color = liquidColor;

        if (dropletLifetime > 0f)
            Destroy(newDroplet, dropletLifetime);

        return newDroplet;
    }

    private Vector3 GetStreamOrigin()
    {
        return transform.position + new Vector3(
            streamOffset.x,
            volumeSize.y * 0.5f + streamOffset.y,
            0f
        );
    }

    void OnDrawGizmos()
    {
        Gizmos.color = liquidColor;
        Vector3 sizeZ = volumeSize;
        sizeZ.z = 1;
        Gizmos.DrawCube(transform.position, sizeZ);

        if (spawnAsStream)
        {
            Vector3 streamOrigin = GetStreamOrigin();
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(streamOrigin, dropletSize * 0.5f);
            Gizmos.DrawLine(streamOrigin, streamOrigin + (Vector3)streamVelocity * 0.25f);
        }
    }
}