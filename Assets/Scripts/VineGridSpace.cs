using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VineGridSpace : MonoBehaviour
{
    public enum VINE_GRID_STATE {
        NOT_PURCHASED, // Player doesn't own it
        UNUSED,        // Player owns it. Hasn't done anything with it
        HAS_SOIL,      // Player has laid soil
        HAS_VINE,      // Player has a vine at this spot
        DEAD           // Dead vines -> UNUSED
    }
    private VINE_GRID_STATE vineState = VINE_GRID_STATE.NOT_PURCHASED;

    [Header("Sprites")]
    [SerializeField] private Sprite notPurchasedSpr;
    [SerializeField] private Sprite unusedSpr;
    [SerializeField] private Sprite soilSpr;
    [SerializeField] private Sprite vineSpr;

    // Components
    private SpriteRenderer srenderer;

    [Header("UI")]
    [SerializeField] private Animator UIManagement;
    [SerializeField] private TextMeshProUGUI ownershipActionTxt;
    [SerializeField] private Button managementButton;

    // Stats
    [Range(0,2)] private float soilHealth; const float soilDecreaseRate = 0.02f;
    [Range(0,3)] private float waterValue = 1.5f; const float waterDecreaseRate = 0.03f;
    private int waterMult = 0;
    private float growth = 0; const float growthRate = 0.04f; 

    void Start()
    {
        managementButton.onClick.AddListener(() => FindAnyObjectByType<ViewSelector>().GoToView(4));
        srenderer = GetComponent<SpriteRenderer>();
        gameObject.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
        ChangeState(VINE_GRID_STATE.NOT_PURCHASED);
        CloseWindow();
    }

    public void ChangeState(VINE_GRID_STATE state)
    {
        vineState = state;
        if (vineState == VINE_GRID_STATE.UNUSED)
        {
            NoWater();
            waterValue = 1.5f;
        } 
        else if (vineState == VINE_GRID_STATE.HAS_VINE)
        {
            growth = 0;
            TurnOffWater();
        }
        else if (vineState == VINE_GRID_STATE.DEAD)
        {
            NoWater();
            waterValue = 1.5f;
            soilHealth = 0f;
        }

        UpdateAppearance();
        if (VineObject.IsGridSpace(this))
            VineObject.UpdateAppearance();
    }

    public VINE_GRID_STATE GetState()
    {
        return vineState;
    }

    private void UpdateAppearance()
    {
        switch (vineState)
        {
            case VINE_GRID_STATE.NOT_PURCHASED:
                srenderer.sprite = notPurchasedSpr;
                break;
            case VINE_GRID_STATE.UNUSED: case VINE_GRID_STATE.DEAD:
                srenderer.sprite = unusedSpr;
                break;
            case VINE_GRID_STATE.HAS_SOIL:
                srenderer.sprite = soilSpr;
                break;
            case VINE_GRID_STATE.HAS_VINE:
                srenderer.sprite = vineSpr;
                break;
        }
    }

    const int PLOT_CHARGE = 50;
    private void Purchase()
    {
        if (vineState != VINE_GRID_STATE.NOT_PURCHASED)
            return;

        if (Player.AddCoins(-PLOT_CHARGE))
        {
            ChangeState(VINE_GRID_STATE.UNUSED);
            ownershipActionTxt.text = "SELL";
            managementButton.interactable = true;
        }
    }

    private void Sell()
    {
        if (vineState == VINE_GRID_STATE.NOT_PURCHASED)
            return;
        
        Player.AddCoins(PLOT_CHARGE);
        ChangeState(VINE_GRID_STATE.NOT_PURCHASED);
        ownershipActionTxt.text = "PURCHASE";
        managementButton.interactable = false;
    }

    public void ChangeOwnership()
    {
        if (vineState == VINE_GRID_STATE.NOT_PURCHASED)
            Purchase();
        else
            Sell();
    }

    bool isMouseOver = false;
    void OnMouseEnter()
    {
        isMouseOver = true;
        AudioManager.Play("GridHover", 1, 1, false);
    }

    void OnMouseExit()
    {
        isMouseOver = false;
    }

    public void ResetSoilHealth()
    {
        soilHealth = 2f;
    }

    public float GetSoilHealth()
    {
        return soilHealth;
    }

    public float GetIrrigation()
    {
        return waterValue;
    }

    public void TurnOnWater() { waterMult = 1; }
    public void TurnOffWater() { waterMult = -1; }
    public void NoWater() { waterMult = 0; }
    public bool IsWaterOn() { return waterMult == 1; }

    public float GetGrowth()
    {
        return growth;
    }

    public void Prune()
    {
        growth = Mathf.Max(Mathf.Floor(growth - 1f), 1);
        Debug.Log(growth);

        if (GetState() == VINE_GRID_STATE.DEAD)
        {
            ChangeState(VINE_GRID_STATE.UNUSED);
            growth = 0;
        }
    }

    public int Harvest()
    {
        int grapesToAdd = Mathf.RoundToInt(Mathf.Floor(growth) * 10 + Random.Range(-5f, 5f));
        grapesToAdd = Mathf.Max(grapesToAdd, 0);
        Player.AddGrapes(grapesToAdd);
        growth = 0; // reset growth
        return grapesToAdd;
    }

    int lastGrowthState = 0;
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMouseOver && !GameManager.IsPointerOverUI())
            CloseWindow();
        
        if (vineState == VINE_GRID_STATE.NOT_PURCHASED)
            return;

        // Soil
        soilHealth -= soilDecreaseRate * Time.deltaTime;
        if (soilHealth <= 0 && vineState != VINE_GRID_STATE.UNUSED)
        {
            if (growth > 1)
                ChangeState(VINE_GRID_STATE.DEAD);
            else
                ChangeState(VINE_GRID_STATE.UNUSED);
        }

        // Water
        waterValue += waterMult * waterDecreaseRate * Time.deltaTime;
        waterValue = Mathf.Clamp(waterValue,0,3);
        if ((waterValue <= 0 || waterValue >= 3) && vineState != VINE_GRID_STATE.UNUSED)
        {
            ChangeState(VINE_GRID_STATE.DEAD);
        }

        if (vineState != VINE_GRID_STATE.HAS_VINE && vineState != VINE_GRID_STATE.DEAD)
            return;

        // Growth
        growth += growthRate * Time.deltaTime;
        if (Mathf.FloorToInt(growth) != lastGrowthState)
            AudioManager.Play("VineGrowthPhase3", 1, 1, false);

        if (growth - Player.GetTrellisCount() > 2)
            ChangeState(VINE_GRID_STATE.DEAD);
        lastGrowthState = Mathf.FloorToInt(growth);
    }

    // UI
    void OnMouseDown()
    {
        // ui overlap check
        if (GameManager.IsPointerOverUI())
            return;

        // open the management window
        UIManagement.SetBool("isOpen", true);
        managementButton.interactable = vineState != VINE_GRID_STATE.NOT_PURCHASED;
        VineObject.AssignGridSpace(this);
        GetComponent<OutlineOnMouseOver>().BoldOutline();
    }

    public void CloseWindow()
    {
        UIManagement.SetBool("isOpen", false);
        GetComponent<OutlineOnMouseOver>().ResetOutline();
    }
}