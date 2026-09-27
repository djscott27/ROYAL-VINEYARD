using UnityEngine;

public class SeedResource : VineyardResource
{
    private SpriteRenderer srenderer;

    void Start()
    {
        srenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Player.HasSeeds() && VineObject.GetState() == VineGridSpace.VINE_GRID_STATE.HAS_SOIL)
        {
            srenderer.color = Color.white;
            GetComponent<OutlineOnMouseOver>().enabled = true;
        }
        else
        {
            srenderer.color = Color.gray;
            GetComponent<OutlineOnMouseOver>().enabled = false;
        }
    }

    protected override bool CanSpawn()
    {
        return Player.AddSeeds(-1) && VineObject.GetState() == VineGridSpace.VINE_GRID_STATE.HAS_SOIL;
    }
}