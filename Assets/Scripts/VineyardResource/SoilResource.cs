using UnityEngine;

public class SoilResource : VineyardResource
{
    private SpriteRenderer srenderer;

    void Start()
    {
        srenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Player.HasSoil())
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
        return Player.AddSoil(-1);
    }
}