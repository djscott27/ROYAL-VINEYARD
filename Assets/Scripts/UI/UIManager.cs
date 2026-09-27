using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    static UIManager instance;
    void Awake()
    {
        instance = this;
    }

    // Panels
    [SerializeField] private GameObject vineManagementUI;
    public static void ActivateVineManagementUI(bool v) { instance.vineManagementUI.SetActive(v); }
    [SerializeField] private GameObject regGraphics;
    public static void ActivateRegGraphics(bool v) { UIVisibility.SetGraphicsActive(instance.regGraphics, v); }

    // Text
    [SerializeField] private TextMeshProUGUI coinsTxt; 
    public static void SetCoinDisplay(int coins) { instance.coinsTxt.text = "Coins: " + coins; }
    [SerializeField] private TextMeshProUGUI grapesTxt; 
    public static void SetGrapesDisplay(int grapes) { instance.grapesTxt.text = "Grapes: " + grapes; }
    [SerializeField] private TextMeshProUGUI wineTxt; 
    public static void SetWineDisplay(int wine, int max) { instance.wineTxt.text = "Wine: " + wine + "/" + max; }
    [SerializeField] private TextMeshProUGUI barrelTxt; 
    public static void SetBarrelDisplay(int barrel, int max) { instance.barrelTxt.text = "Barrels: " + barrel + "/" + max; }
    [SerializeField] private TextMeshProUGUI timeTxt;
    public static void SetTimeDisplay(int seconds) { instance.timeTxt.text = 
        seconds > 0 ?
            $"{seconds} seconds remain..." :
            "End of day"; 
        }

    // Buttons
    [SerializeField] private Button nextEventButton;
    public static void SetNextListener(UnityAction listener) { instance.nextEventButton.onClick.AddListener(listener); }
    public static void SetButtonText(string t) {instance.nextEventButton.GetComponentInChildren<TextMeshProUGUI>().text = t; }

    // Animations
    [SerializeField] private Animation vTransition;
    public static Animation GetVTransition() { return instance.vTransition; }
    [SerializeField] private Animation parchment;
    public static void TriggerParchment() { instance.parchment.Play(); }

    public static class UIVisibility
    {
        // Remembers which graphics were on when hidden, so re-showing
        // doesn't turn on graphics that were already off
        private static readonly Dictionary<GameObject, List<Graphic>> hidden =
            new Dictionary<GameObject, List<Graphic>>();

        public static void SetGraphicsActive(GameObject root, bool active)
        {
            if (!active)
            {
                if (hidden.ContainsKey(root))
                    return; // already hidden

                var turnedOff = new List<Graphic>();
                foreach (Graphic g in root.GetComponentsInChildren<Graphic>(true))
                {
                    if (g.enabled)
                    {
                        g.enabled = false;
                        turnedOff.Add(g);
                    }
                }
                hidden[root] = turnedOff;
            }
            else
            {
                if (!hidden.TryGetValue(root, out var turnedOff))
                    return; // nothing to restore

                foreach (Graphic g in turnedOff)
                {
                    if (g != null) // may have been destroyed meanwhile
                        g.enabled = true;
                }
                hidden.Remove(root);
            }
        }
    }
}
