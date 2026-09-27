using UnityEngine;

public abstract class VineyardResource : MonoBehaviour
{
    [SerializeField] private GameObject prefab; 
    void OnMouseDown()
    {
        if (CanSpawn())
            SpawnInResource();
    }

    protected abstract bool CanSpawn();

    private void SpawnInResource()
    {
        GameObject spawn = Instantiate(prefab, transform.position, transform.rotation);
        spawn.GetComponent<Draggable>().StartDrag();
    }
}
