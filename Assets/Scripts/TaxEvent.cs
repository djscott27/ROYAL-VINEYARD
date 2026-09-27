using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TaxEvent : MonoBehaviour
{
    TextMeshProUGUI text;
    public static TaxEvent Instance;
    void Start()
    {
        Instance = this;
        text = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        
    }
    public static void UpdateTaxEvent()
    {
        var tax = GameManager.ComputeTax();
        if (Player.AddCoins(-tax))
        {
            UIManager.SetNextListener(() => GameManager.UnpauseTime());
            UIManager.SetButtonText("Start a new day...");
            Instance.text.text = $"Dear Royal Sommelier, \n\nMy royal court and various patrons appreciate your craft. The fruits of your labor quell the pain of life. Your reputation proceeds you throughout the kingdom. However, that doesn't put you above my law, thus you must pay your fair share in taxes, <color=yellow>${tax}</color> coin.";
        }
        else
        {
            Instance.text.text = $"Dear Royal Sommelier, \n\nAlthough your craft is highly esteemed throughout the land, your inability to pay your fair share and follow my law shall permanently tarnish your name. I hereby refuse to purchase your wine and your patrons shall flock to other vineyards in response.";
            UIManager.SetButtonText("Main Menu");
            UIManager.SetNextListener(() => TaxEvent.MainMenu());
        }
        
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
