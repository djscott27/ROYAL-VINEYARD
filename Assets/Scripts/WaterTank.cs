using TMPro;
using UnityEngine;

public class WaterTank : MonoBehaviour
{
    private Animation anim;
    private bool waterOn = false;

    [SerializeField] private TextMeshPro statusTxt;

    void Start()
    {
        anim = GetComponent<Animation>();
        statusTxt.text = "Water is off";
    }

    void OnMouseDown()
    {
        if (anim.isPlaying)
            return;

        anim.Play();
        waterOn = !waterOn;

        if (waterOn)
        {
            statusTxt.text = "Water is flowing";
            VineObject.ToggleWaterOn();
        }
        else
        {
            statusTxt.text = "Water is off";
            VineObject.ToggleWaterOff();
        }
    }
}
