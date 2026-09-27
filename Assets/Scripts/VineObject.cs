using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VineObject : MonoBehaviour
{
    static VineObject instance;

    void Awake()
    {
        instance = this;
    }

    private VineGridSpace assocSpace;

    [Header("UI")]
    [SerializeField] private Slider soilHealthBar;
    [SerializeField] private Slider irrigationBar;
    [SerializeField] private GameObject waterTank;
    [SerializeField] private Animator pruner;
    [SerializeField] private GameObject harvest;
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private TextMeshPro harvestResultTxt;

    [Header("Sprites - Bottom")]
    [SerializeField] private Sprite unplantedTrellis;
    [SerializeField] private Sprite seedsTrellis;
    [SerializeField] private Sprite regularTrellis;
    [SerializeField] private Sprite overwateredTrellis;
    [SerializeField] private Sprite underwateredTrellis;
    [SerializeField] private Sprite deadTrellis;

    [Header("Sprites - Top")]
    [SerializeField] private Sprite topRegular;
    [SerializeField] private Sprite topOvergrownRegular;
    [SerializeField] private Sprite topBadSoil;
    [SerializeField] private Sprite topOvergrownBadSoil;
    [SerializeField] private Sprite topOverwatered;
    [SerializeField] private Sprite topOvergrownOverwatered;
    [SerializeField] private Sprite topOverwateredBadSoil;
    [SerializeField] private Sprite topUnderwatered;
    [SerializeField] private Sprite topOvergrownUnderwatered;
    [SerializeField] private Sprite topUnderwateredBadSoil;
    [SerializeField] private Sprite topDead;

    [Header("Sprites - Middle")]
    [SerializeField] private Sprite midRegular;
    [SerializeField] private Sprite midOvergrownRegular;
    [SerializeField] private Sprite midBadSoil;
    [SerializeField] private Sprite midOvergrownBadSoil;
    [SerializeField] private Sprite midOverwatered;
    [SerializeField] private Sprite midOvergrownOverwatered;
    [SerializeField] private Sprite midOverwateredBadSoil;
    [SerializeField] private Sprite midUnderwatered;
    [SerializeField] private Sprite midOvergrownUnderwatered;
    [SerializeField] private Sprite midUnderwateredBadSoil;
    [SerializeField] private Sprite midDead;
    [SerializeField] private Sprite emptyTrellis;

    [Header("Sprite Objs")]
    [SerializeField] private SpriteRenderer bottom;
    [SerializeField] private SpriteRenderer top;
    [SerializeField] private SpriteRenderer unfertilized;

    // Pooled middle tiles, index 0 sits directly above the bottom piece.
    private readonly List<SpriteRenderer> middles = new List<SpriteRenderer>();

    public static void AssignGridSpace(VineGridSpace space)
    {
        instance.assocSpace = space;
    }

    public static bool IsGridSpace(VineGridSpace space)
    {
        return instance.assocSpace == space;
    }

    void Update()
    {
        if (assocSpace == null)
            return;

        soilHealthBar.value = assocSpace.GetSoilHealth();
        irrigationBar.value = assocSpace.GetIrrigation();
    }

    void FixedUpdate() => UpdateAppearance();

    public static void UpdateAppearance()
    {
        if (instance != null)
            instance.Refresh();
    }

    private void Refresh()
    {
        if (assocSpace == null)
            return;

        VineGridSpace.VINE_GRID_STATE state = assocSpace.GetState();
        waterTank.SetActive(state == VineGridSpace.VINE_GRID_STATE.HAS_VINE);
        pruner.gameObject.SetActive(state == VineGridSpace.VINE_GRID_STATE.HAS_VINE || state == VineGridSpace.VINE_GRID_STATE.DEAD);
        harvest.SetActive(state == VineGridSpace.VINE_GRID_STATE.HAS_VINE);

        // The trellis frame always shows as many trellises as the player owns, in every state.
        int tiles = Player.GetTrellisCount();
        float tileHeight = bottom.size.y;
        bottom.gameObject.SetActive(true);
        top.gameObject.SetActive(true);
        top.transform.position = bottom.transform.position + (Vector3.up * tiles * tileHeight);

        UpdateStatusText(state, tiles);

        switch (state)
        {
            case VineGridSpace.VINE_GRID_STATE.UNUSED:
                unfertilized.gameObject.SetActive(true);
                bottom.sprite = unplantedTrellis;
                LayoutMiddles(tiles - 1, 0, null);
                top.sprite = emptyTrellis;
                break;

            case VineGridSpace.VINE_GRID_STATE.HAS_SOIL:
                unfertilized.gameObject.SetActive(false);
                bottom.sprite = unplantedTrellis;
                LayoutMiddles(tiles - 1, 0, null);
                top.sprite = emptyTrellis;
                break;

            case VineGridSpace.VINE_GRID_STATE.HAS_VINE:
                unfertilized.gameObject.SetActive(false);

                float growth = assocSpace.GetGrowth();
                if (growth < 1)
                {
                    // Seeds: empty trellis all the way up
                    bottom.sprite = seedsTrellis;
                    LayoutMiddles(tiles - 1, 0, null);
                    top.sprite = emptyTrellis;
                    break;
                }

                // actual plant
                float growthDiff = growth - tiles;
                bool overgrown = growthDiff > 0;

                // Sprite choices
                float irrigation = assocSpace.GetIrrigation();
                bool badSoil = assocSpace.GetSoilHealth() < 1;
                Sprite midSprite;
                Sprite topSprite;

                if (irrigation < 1)
                {
                    bottom.sprite = underwateredTrellis;
                    midSprite = badSoil ? midUnderwateredBadSoil : (overgrown ? midOvergrownUnderwatered : midUnderwatered);
                    topSprite = badSoil ? topUnderwateredBadSoil : (overgrown ? topOvergrownUnderwatered : topUnderwatered);
                }
                else if (irrigation > 2)
                {
                    bottom.sprite = overwateredTrellis;
                    midSprite = badSoil ? midOverwateredBadSoil : (overgrown ? midOvergrownOverwatered : midOverwatered);
                    topSprite = badSoil ? topOverwateredBadSoil : (overgrown ? topOvergrownOverwatered : topOverwatered);
                }
                else
                {
                    bottom.sprite = regularTrellis;
                    midSprite = badSoil ? (overgrown ? midOvergrownBadSoil : midBadSoil) : (overgrown ? midOvergrownRegular : midRegular);
                    topSprite = badSoil ? (overgrown ? topOvergrownBadSoil : topBadSoil) : (overgrown ? topOvergrownRegular : topRegular);
                }

                // Pieces above the bottom, counted bottom-up: middles first, then the top.
                // The first floor(growth) of them get the grown sprite, the rest are empty.
                int grown = Mathf.FloorToInt(growth);
                LayoutMiddles(tiles - 1, grown, midSprite);
                top.sprite = grown >= tiles ? topSprite : emptyTrellis;
                break;

            case VineGridSpace.VINE_GRID_STATE.DEAD:
                unfertilized.gameObject.SetActive(true);
                bottom.sprite = deadTrellis;
                top.sprite = topDead;
                LayoutMiddles(tiles - 1, Mathf.FloorToInt(assocSpace.GetGrowth()), midDead);
                break;
        }
    }

    // ---------- Status text ----------

    private void UpdateStatusText(VineGridSpace.VINE_GRID_STATE state, int tiles)
    {
        string status = BuildStatus(state, tiles);
        if (statusTxt.text != status)
            statusTxt.text = status;
    }

    /// <summary>
    /// Short summary of what's wrong with the vine, e.g. "Underwatered, Poor soil".
    /// Uses the same thresholds as the sprite selection in Refresh().
    /// </summary>
    private string BuildStatus(VineGridSpace.VINE_GRID_STATE state, int tiles)
    {
        if (state == VineGridSpace.VINE_GRID_STATE.DEAD)
            return "<color=red>DEAD</color>";
        if (state != VineGridSpace.VINE_GRID_STATE.HAS_VINE)
            return "UNPLANTED";

        string status = "";

        float irrigation = assocSpace.GetIrrigation();
        if (irrigation < 1)
            status = AddStatus(status, "UNDERWATERED");
        else if (irrigation > 2)
            status = AddStatus(status, "OVERWATERED");

        if (assocSpace.GetSoilHealth() < 1)
            status = AddStatus(status, "POOR SOIL");

        float growth = assocSpace.GetGrowth();
        if (growth >= 1 && growth > tiles)
            status = AddStatus(status, "OVERGROWN");

        return status.Length == 0 ? "<color=green>HEALTHY</color>" : status;
    }

    private static string AddStatus(string status, string part)
    {
        return status.Length == 0 ? part : status + ", " + part;
    }

    // ---------- Middle tiles ----------

    /// <summary>
    /// Shows `count` middle tiles stacked above the bottom piece. Tiles with index
    /// below `grownCount` use `grownSprite`; the rest use emptyTrellis.
    /// </summary>
    private void LayoutMiddles(int count, int grownCount, Sprite grownSprite)
    {
        count = Mathf.Max(0, count);

        while (middles.Count < count)
            middles.Add(CreateMiddle(middles.Count));

        float tileHeight = bottom.size.y;
        for (int i = 0; i < middles.Count; i++)
        {
            SpriteRenderer sr = middles[i];
            bool active = i < count;
            sr.gameObject.SetActive(active);
            if (!active)
                continue;

            sr.sprite = i < grownCount ? grownSprite : emptyTrellis;
            MatchTiling(sr);
            sr.transform.position = bottom.transform.position + (Vector3.up * (i + 1) * tileHeight);
        }
    }

    /// <summary>Copies the bottom piece's draw/tiling setup so every middle is the same tile size.</summary>
    private void MatchTiling(SpriteRenderer sr)
    {
        sr.drawMode = bottom.drawMode;
        sr.tileMode = bottom.tileMode;
        sr.adaptiveModeThreshold = bottom.adaptiveModeThreshold;
        sr.size = bottom.size; // set after the sprite so a sprite swap can't reset it
    }

    private SpriteRenderer CreateMiddle(int index)
    {
        GameObject go = new GameObject($"Middle {index}");
        go.transform.SetParent(bottom.transform.parent, false);
        go.transform.localRotation = bottom.transform.localRotation;
        go.transform.localScale = bottom.transform.localScale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sharedMaterial = bottom.sharedMaterial;
        sr.color = bottom.color;
        sr.sortingLayerID = bottom.sortingLayerID;
        sr.sortingOrder = bottom.sortingOrder;
        sr.sprite = emptyTrellis;
        MatchTiling(sr);
        return sr;
    }

    // ---------- Public API ----------

    public static VineGridSpace.VINE_GRID_STATE GetState()
    {
        if (instance.assocSpace != null)
            return instance.assocSpace.GetState();
        return VineGridSpace.VINE_GRID_STATE.UNUSED;
    }

    public static void FeedSoil()
    {
        if (GetState() == VineGridSpace.VINE_GRID_STATE.UNUSED)
            ChangeState(VineGridSpace.VINE_GRID_STATE.HAS_SOIL);

        instance.assocSpace.ResetSoilHealth();

        AudioManager.Play("Planting", 1, 1, false);
    }

    public static void PlantSeeds()
    {
        if (instance.assocSpace.GetState() == VineGridSpace.VINE_GRID_STATE.HAS_VINE)
            return;

        ChangeState(VineGridSpace.VINE_GRID_STATE.HAS_VINE);

        AudioManager.Play("Planting",1, 1, false);
    }

    public static void ToggleWaterOn()
    {
        instance.assocSpace.TurnOnWater();

        AudioManager.Play("ValveOn", 1, 1, false);
        AudioManager.Unmute("WaterStream");
    }

    public static bool IsWaterOn()
    {
        return instance.assocSpace.IsWaterOn();
    }

    public static void ToggleWaterOff()
    {
        instance.assocSpace.TurnOffWater();
        AudioManager.Play("ValveOff", 1, 1, false);

        AudioManager.Mute("WaterStream");
    }

    public static void ChangeState(VineGridSpace.VINE_GRID_STATE state)
    {
        instance.assocSpace.ChangeState(state);
    }

    // pruning babay
    public void StartPruneAnim()
    {
        pruner.SetBool("isPruning", true);
    }

    public void StopPruneAnim()
    {
        pruner.SetBool("isPruning", false);
    }

    public void Prune()
    {
        StopPruneAnim();
        assocSpace.Prune();
    }

    public void Harvest()
    {
        int added = assocSpace.Harvest();
        harvestResultTxt.text = "+" + added + " Grapes!";
        harvestResultTxt.GetComponent<Animation>().Play();

        AudioManager.Play("Harvest", 1, 1, false);
    }
}