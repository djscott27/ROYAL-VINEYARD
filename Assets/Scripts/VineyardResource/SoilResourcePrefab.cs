using UnityEngine;

public class SoilResourcePrefab : Draggable
{
    protected override void OnGrab()
    {
        Debug.Log("Attempted to pick up soil");
    }

    protected override void OnRelease()
    {
        Debug.Log("Released soil");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<VineObject>())
        {
            VineObject.FeedSoil();
            Destroy(gameObject);
        }
    }
}