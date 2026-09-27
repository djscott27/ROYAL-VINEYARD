using UnityEngine;

public class Parchment : MonoBehaviour
{
    public void PlayParchmentSound()
    {
        AudioManager.Play("ParchmentOpen", 1,1,false);
    }
}
