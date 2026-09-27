using TMPro;
using UnityEngine;
using MEC;
using System.Collections.Generic;

public class Barrel : MonoBehaviour
{
    const int GRAPE_COST = 10; // how much grapes to use?
    private bool inUse = false; // does this barrel got grapes?

    private const float agePerFrame = 0.012f;

    [SerializeField] private SpriteRenderer grphx;

    [Header("UI")]
    [SerializeField] private TieredBar agedBar;
    [SerializeField] private TextMeshProUGUI actionTxt;

    void Start()
    {
        gameObject.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        agedBar.gameObject.SetActive(false);
    }

    bool played1 = false;
    bool played2 = false;
    bool played3 = false;

    void Update()
    {
        actionTxt.text = "AGE";
        if (!inUse)
            return;

        actionTxt.text = agedBar.isAgedCorrectly() ? "GET WINE" : "DISPOSE";

        if (agedBar.isUnderdone() && !played1) 
        {
            AudioManager.Play("AgingGrapesPhase1", 1, 1, false);
            played1 = true;
        }

        if (agedBar.isAgedCorrectly() && !played2)
        {
            AudioManager.Play("AgingGrapesPhase2", 1, 1, false);
            played2 = true;
        }

        if (agedBar.isOverdone() && !played3)
        {
            AudioManager.Play("AgingGrapesPhase3", 1, 1, false);
            played3 = true;
        }
    }

    public void Use()
    {
        if (inUse)
        {
            if (agedBar.isAgedCorrectly())
            {
                // Give the player some wine! 
                Player.AddWine(1);
                AudioManager.Play("CollectWine", 1, 1, false);
                GameObject wine = Resources.Load<GameObject>("WineBottle");
                Instantiate(wine, WineTable.GetSpawnPosition(), Quaternion.identity);
            }
            
            agedBar.gameObject.SetActive(false);
            inUse = false;
        }
        else
        {
            // Use grapes
            if (Player.AddGrapes(-GRAPE_COST))
            {
                played1 = false;
                played2 = false;
                played3 = false;
                Timing.RunCoroutine(StartAging());
            }
        }
    }

    IEnumerator<float> StartAging()
    {
        AudioManager.Play("GrapesIntoCrusher", 1, 1, false);
        grphx.enabled = false;
        Crusher.Crush();
        yield return Timing.WaitForSeconds(1.2f);
        agedBar.gameObject.SetActive(true);
        agedBar.StartProgress(0f, agePerFrame);
        inUse = true;
        grphx.enabled = true;
    }
}
