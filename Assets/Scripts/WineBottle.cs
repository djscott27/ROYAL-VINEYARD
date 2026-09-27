using UnityEngine;

public class WineBottle : Draggable
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    const int WINE_PRICE = 25;
    protected override void OnRelease()
    {
        if (canBeSold)
        {
            // Give player money
            Player.AddCoins(WINE_PRICE);
            Player.AddWine(-1);
            // TODO: particles would be nice
            Destroy(gameObject);
        }
    }

    protected override void OnGrab()
    {
        AudioManager.Play("PickUpBottleCastle", 1, 1, false);
        throw new System.NotImplementedException();
    }

    // detection
    bool canBeSold = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("King"))
        {
            canBeSold = true;
            anim.SetBool("ready", true);
            AudioManager.Play("SellingWine", 1, 1, false);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("King"))
        {
            canBeSold = false;
            anim.SetBool("ready", false);
        }
    }
}
