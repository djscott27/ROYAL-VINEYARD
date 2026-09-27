using System.ComponentModel.Design;
using UnityEngine;

public class ShopButtonUtility : MonoBehaviour
{
    public int soilCost;
    public int seedsCost;
    public int trellisCost;
    public int barellsCost;
    public TMPro.TextMeshProUGUI soldOutText;
    public TMPro.TextMeshProUGUI brokeText;
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void OnBuySoil()
    {
        if (Player.AddCoins(-Mathf.Abs(soilCost)))
        {
            AudioManager.Play("SoilPurchased", 1.0f, 1.0f, false);
            Player.AddSoil(1);
        }
        else
        {
            TriggerBrokeText();
        }
    }
    public void OnBuySeeds()
    {
        if (Player.AddCoins(-Mathf.Abs(seedsCost)))
        {
            AudioManager.Play("SeedsPurchased", 1.0f, 1.0f, false);
            Player.AddSeeds(1);
        }
        else
        {
            TriggerBrokeText();
        }
    }
    public void OnBuyTrellis()
    {
        if (Player.AddCoins(-Mathf.Abs(trellisCost)))
        {
            AudioManager.Play("TrellisPurchased", 1.0f, 1.0f, false);
            Player.AddTrellis(1);
        }
        else
        {
            TriggerBrokeText();
        }
    }
    public void OnBuyBarells()
    {
        if (Player.AddCoins(-Mathf.Abs(barellsCost)))
        {
            if (!Player.AddBarrels(1))
            {
                TriggerSoldOutText();
                Player.AddCoins(Mathf.Abs(barellsCost));
            }
            else
            {
                BarrelManager.Instance.InstanceBarell();
                //AudioManager.Play("TrellisPurchased", 1.0f, 1.0f, false);
            }
        }
        else
        {
            TriggerBrokeText();
        }
    }
    public void TriggerBrokeText()
    {
        AudioManager.Play("InsufficientFunds", 1.0f, 1.0f, false);
        LeanTween.value(gameObject, Color.white, Color.clear, 0.5f)
            .setEaseInOutSine()
            .setOnUpdate((Color color) =>
            {
                brokeText.color = color;
            });
    }
    public void TriggerSoldOutText()
    {
        AudioManager.Play("SoldOut", 1.0f, 1.0f, false);
        LeanTween.value(gameObject, Color.white, Color.clear, 0.5f)
            .setEaseInOutSine()
            .setOnUpdate((Color color) =>
            {
                soldOutText.color = color;
            });
    }
}
