using System;
using UnityEngine;

public class Crusher : MonoBehaviour
{
    static Crusher instance;
    private Animator anim;

    void Start()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }

    public static void Crush()
    {
        instance.anim.SetTrigger("Crush");
    }

    public void crushSound()
    {
        AudioManager.Play("CrushingGrape", 1, 1, false);
    }

    public void pourSound()
    {
        AudioManager.Play("GrapesIntoCrusher", 1, 1, false);
    }
}
