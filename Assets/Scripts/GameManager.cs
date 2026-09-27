using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    static GameManager instance;

    public const int CYCLE_LENGTH = 300;
    public float timeSeconds { get; private set; } = 0f;
    public bool isTimePaused;

    [SerializeField]
    private LayerMask uiBlockLayers;

    void Start()
    {
        instance = this;

        // Create a player instance
        new Player(100);
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return null; // resumes next frame, after every Start has run
        AudioManager.Play("CastleTheme", 1, 1, true);
        AudioManager.Play("ShopTheme", 1, 1, true);
        AudioManager.Play("MainTheme", 1, 1, true);
        AudioManager.Play("VineViewTheme", 1, 1, true);
        AudioManager.Play("CellarTheme", 1, 1, true);

        AudioManager.Play("WaterStream", 1, 1, true);
        AudioManager.Mute("WaterStream");
    }

    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            GameObject hit = result.gameObject;
            if (((1 << hit.layer) & instance.uiBlockLayers.value) != 0)
                return true;
        }

        return false;
    }
    private void Update()
    {
        if (!isTimePaused)
        {
            timeSeconds += Time.deltaTime;
            UIManager.SetTimeDisplay(Mathf.FloorToInt(CYCLE_LENGTH - timeSeconds));
            if (Mathf.FloorToInt(timeSeconds) % CYCLE_LENGTH == 0 && Mathf.FloorToInt(timeSeconds) != 0)
            {

                AudioManager.Play("DayComplete", 1, 1, false);
                timeSeconds += 1;
                isTimePaused = true;
                cycles++;
                TaxEvent.UpdateTaxEvent();
                ViewSelector.Instance.GoToView(ViewSelector.VIEW_TAX_RESULT);
                AudioManager.Play("ParchmentOpen", 1.0f, 1.0f, false);
            }
            if (Mathf.FloorToInt(timeSeconds) - (CYCLE_LENGTH / 2) % CYCLE_LENGTH == 0)
            {
                AudioManager.Play("MidDayReminder", 1.0f, 1.0f, false);
                timeSeconds += 1;
            }
        }
    }

    int cycles = 0;
    public static void UnpauseTime()
    {
        if (instance.isTimePaused)
        {
            instance.isTimePaused = false;
            AudioManager.Play("ParchmentClose",1,1,false);
            ViewSelector.Instance.GoToView(ViewSelector.VIEW_VINEYARD_TOPDOWN);
            instance.timeSeconds = 0;
            AudioManager.Play("DayCycleWind", 1.0f, 1.0f, false);
        }
    }
    public static int ComputeTax()
    {
        return instance.cycles * 10;
    }
}
