using UnityEngine;

public class SeedResourcePrefab : Draggable
{
    protected override void OnGrab()
    {
        Debug.Log("Attempted to pick up seed");
    }

    protected override void OnRelease()
    {
        Debug.Log("Released seed");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<VineObject>())
        {
            VineObject.PlantSeeds();
            Destroy(gameObject);
        }
    }
}