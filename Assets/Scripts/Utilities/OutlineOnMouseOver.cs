using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class OutlineOnMouseOver : MonoBehaviour
{
    private Material outlineMaterial;
    bool bolded = false;

    void Awake()
    {
        Shader outlineShader = Shader.Find("Sprites/Outline");
        outlineMaterial = new Material(outlineShader);
        outlineMaterial.SetFloat("_OutlineEnabled",0);
        outlineMaterial.SetFloat("_OutlineMode",0);
        outlineMaterial.SetFloat("_OutlineShape",0);
        outlineMaterial.SetFloat("_OutlinePosition",2);
        outlineMaterial.SetFloat("_Thickness",0.5f);
        GetComponent<SpriteRenderer>().material = outlineMaterial;
    }

    void OnMouseOver()
    {
        if (!GameManager.IsPointerOverUI())
            outlineMaterial.SetFloat("_OutlineEnabled",1);
    }

    void OnMouseExit()
    {
        if (!bolded)
            outlineMaterial.SetFloat("_OutlineEnabled",0);
    }

    public void BoldOutline()
    {
        bolded = true;
        outlineMaterial.SetFloat("_Thickness",0.8f);
    }

    public void ResetOutline()
    {
        bolded = false;
        outlineMaterial.SetFloat("_Thickness",0.5f);
        outlineMaterial.SetFloat("_OutlineEnabled",0);
    }
}
