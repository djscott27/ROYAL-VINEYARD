using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TieredBar : MonoBehaviour
{
    [SerializeField] private Image indicator;
    [SerializeField] private int tiers;

    private Slider bar;
    private float rate;

    void OnEnable()
    {
        bar = GetComponent<Slider>();
        bar.minValue = 0;
        bar.maxValue = tiers;
    }

    void Update()
    {
        indicator.color = isAgedCorrectly() ? Color.yellow : Color.gray;
        bar.value += rate * Time.deltaTime;
    }

    public bool isAgedCorrectly()
    {
        return bar.value > 1 && bar.value < 2;
    }

    public bool isUnderdone()
    {
        return bar.value < 1;
    }

    public bool isOverdone()
    {
        return bar.value > 2;
    }

    public void StartProgress(float at, float rate)
    {
        bar.value = at;
        this.rate = rate;
    }
}
