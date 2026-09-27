using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MEC;

public class ViewSelector : MonoBehaviour
{

    public static ViewSelector Instance;
    [SerializeField] private Button[] selectorButtons; // parallel
    [SerializeField] private Transform[] viewObjs; // parallel

    // Special Views
    public const int VIEW_VINEYARD_TOPDOWN = 0;
    public const int VIEW_VINEYARD_FRONT = 4;
    public const int VIEW_TAX_RESULT = 5;

    void Start()
    {
        Instance = this;
        GoToView(0, false);
        UIManager.ActivateVineManagementUI(false);
    }

    public void GoToView(int v)
    {
        GoToView(v, true);
    }

    public void GoToView(int v, bool transition)
    {
        if (v <= 3)
        {
            AudioManager.Play("ClickView", 1.0f, 1.0f, false);
        }
        // disable/enable buttons
        if (v != VIEW_VINEYARD_FRONT)
        {
            for (int i = 0; i < selectorButtons.Length; i++)
            {
                selectorButtons[i].interactable = i != v;
            }
        }

        // Swap view
        if (transition)
            Timing.RunCoroutine(_SwapView(v));
        else
            SwapCameraPosition(v);
    }

    IEnumerator<float> _SwapView(int v)
    {
        AudioManager.Play("ClickingIntoScene", 1, 1, false);

        Animation vTransition = UIManager.GetVTransition();
        vTransition.Play();
        yield return Timing.WaitForSeconds(vTransition.clip.length/2);
            
        if (v == VIEW_VINEYARD_FRONT)
        {
            VineObject.UpdateAppearance();
            UIManager.ActivateVineManagementUI(true);
        }
        else
        {
            UIManager.ActivateVineManagementUI(false);
        }
        UIManager.ActivateRegGraphics(v != VIEW_TAX_RESULT);
        if (v == VIEW_TAX_RESULT)
        {
            UIManager.TriggerParchment();
        }

        SwapCameraPosition(v);

        if (v == 0) 
        {
            AudioManager.Mute("CastleTheme");
            AudioManager.Mute("CellarTheme");
            AudioManager.Unmute("MainTheme");
            AudioManager.Mute("ShopTheme");
            AudioManager.Mute("VineViewTheme");
        }

        if (v == 1)
        {
            AudioManager.Mute("CastleTheme");
            AudioManager.Mute("CellarTheme");
            AudioManager.Mute("MainTheme");
            AudioManager.Unmute("ShopTheme");
            AudioManager.Mute("VineViewTheme");
        }

        if (v == 2)
        {
            AudioManager.Mute("CastleTheme");
            AudioManager.Unmute("CellarTheme");
            AudioManager.Mute("MainTheme");
            AudioManager.Mute("ShopTheme");
            AudioManager.Mute("VineViewTheme");
        }

        if (v == 3)
        {
            AudioManager.Unmute("CastleTheme");
            AudioManager.Mute("CellarTheme");
            AudioManager.Mute("MainTheme");
            AudioManager.Mute("ShopTheme");
            AudioManager.Mute("VineViewTheme");
        }

        if (v == 4)
        {
            AudioManager.Mute("CastleTheme");
            AudioManager.Mute("CellarTheme");
            AudioManager.Mute("MainTheme");
            AudioManager.Mute("ShopTheme");
            AudioManager.Unmute("VineViewTheme");
            if (VineObject.IsWaterOn())
                AudioManager.Unmute("WaterStream");
        }

        if (v != 4) 
        {
            AudioManager.Mute("WaterStream");        
        }
    }

    private void SwapCameraPosition(int v)
    {
        CameraController.SetActive(v == VIEW_VINEYARD_TOPDOWN);
        Camera.main.transform.position = viewObjs[v].position + (Vector3.back * 10);
    }
}
