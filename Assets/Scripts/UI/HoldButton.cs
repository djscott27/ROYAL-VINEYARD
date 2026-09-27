using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// A world-space (SpriteRenderer) button that must be held for a set duration before it fires.
/// Hovering fades in a circle; holding sweeps it 360 degrees like a pie.
///
/// The circle sprite and its material are generated at runtime. You don't need any assets
/// except the Custom/RadialFillSprite shader in your project.
///
/// Requirements:
///  - A Collider2D on this GameObject (to receive hover/clicks/touches).
///  - An EventSystem in the scene.
///  - A Physics2DRaycaster on the camera (added automatically to Camera.main if missing).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class HoldButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Hold Settings")]
    [Tooltip("Seconds the button must be held to trigger.")]
    [SerializeField] private float holdDuration = 3f;

    [Tooltip("If true, progress drains back smoothly when released early. If false, it snaps to zero.")]
    [SerializeField] private bool drainOnRelease = true;

    [Tooltip("How fast progress drains back (multiplier of fill speed).")]
    [SerializeField] private float drainSpeedMultiplier = 3f;

    [Tooltip("Cancel the hold if the pointer slides off the button.")]
    [SerializeField] private bool cancelOnPointerExit = true;

    [Header("Hover Fade")]
    [Tooltip("Seconds to fade the circle in on hover (and out when the pointer leaves).")]
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Circle (generated)")]
    [Tooltip("Shader used for the sweep. Filled in automatically in the editor; keep it assigned so it's included in builds.")]
    [SerializeField] private Shader radialFillShader;

    [Tooltip("Size the circle to this object's SpriteRenderer (if it has one).")]
    [SerializeField] private bool autoSizeToButton = true;

    [Tooltip("Circle diameter relative to the button sprite when auto-sizing.")]
    [SerializeField] private float sizeMultiplier = 1f;

    [Tooltip("Circle diameter in local units, used when not auto-sizing (or there's no button sprite).")]
    [SerializeField] private float circleDiameter = 1f;

    [SerializeField] private Color fillColor = new Color(1f, 1f, 1f, 0.8f);

    [Tooltip("Show a faint full circle behind the sweep.")]
    [SerializeField] private bool showTrack = true;

    [SerializeField] private Color trackColor = new Color(1f, 1f, 1f, 0.2f);

    [Tooltip("Texture size of the generated circle. Higher = smoother edges on large circles.")]
    [SerializeField] private int textureResolution = 256;

    [Header("Events")]
    public UnityEvent onHoldStarted;
    public UnityEvent onHoldCanceled;
    public UnityEvent onHoldCompleted;

    public float Progress { get; private set; }   // 0..1
    public bool Interactable = true;

    private const string ShaderName = "Custom/RadialFillSprite";
    private static readonly int FillId = Shader.PropertyToID("_Fill");
    private static readonly int UVRectId = Shader.PropertyToID("_UVRect");

    private SpriteRenderer fillRenderer;
    private SpriteRenderer trackRenderer;
    private MaterialPropertyBlock mpb;

    // Generated assets, cleaned up in OnDestroy.
    private Texture2D circleTexture;
    private Sprite circleSprite;
    private Material fillMaterial;

    private bool isHovered;
    private bool isHolding;
    private bool hasFired;
    private float visibility; // 0 = hidden, 1 = fully shown

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();

        // World-space objects need a 2D physics raycaster on the camera to get pointer events.
        var cam = Camera.main;
        if (cam != null && cam.GetComponent<Physics2DRaycaster>() == null)
            cam.gameObject.AddComponent<Physics2DRaycaster>();

        BuildCircle();
        UpdateVisual();
    }

    private void Start()
    {
        if (EventSystem.current == null)
            Debug.LogWarning("HoldButton: no EventSystem in the scene. Add one via GameObject > UI > Event System.", this);
    }

    // ---------- Circle generation ----------

    private void BuildCircle()
    {
        var buttonRenderer = GetComponent<SpriteRenderer>();

        float diameter = circleDiameter;
        if (autoSizeToButton && buttonRenderer != null && buttonRenderer.sprite != null)
        {
            Vector3 size = buttonRenderer.sprite.bounds.size;
            diameter = Mathf.Max(size.x, size.y) * sizeMultiplier;
        }
        diameter = Mathf.Max(0.01f, diameter);

        int res = Mathf.Clamp(textureResolution, 32, 2048);
        circleTexture = GenerateCircleTexture(res);
        circleSprite = Sprite.Create(
            circleTexture,
            new Rect(0, 0, res, res),
            new Vector2(0.5f, 0.5f),
            res / diameter,          // pixels per unit -> sprite is exactly 'diameter' wide
            0,
            SpriteMeshType.FullRect);
        circleSprite.name = "Generated Circle";

        int sortingLayer = buttonRenderer != null ? buttonRenderer.sortingLayerID : 0;
        int baseOrder = buttonRenderer != null ? buttonRenderer.sortingOrder : 0;

        if (showTrack)
        {
            trackRenderer = CreateChildRenderer("Circle Track", sortingLayer, baseOrder + 1);
            trackRenderer.sprite = circleSprite;
        }

        fillRenderer = CreateChildRenderer("Circle Fill", sortingLayer, baseOrder + 2);
        fillRenderer.sprite = circleSprite;

        if (radialFillShader == null)
            radialFillShader = Shader.Find(ShaderName);

        if (radialFillShader == null)
        {
            Debug.LogError($"HoldButton: shader '{ShaderName}' not found. Add RadialFillSprite.shader to the project.", this);
            return;
        }

        fillMaterial = new Material(radialFillShader) { name = "Generated Radial Fill" };
        fillRenderer.sharedMaterial = fillMaterial;
    }

    private SpriteRenderer CreateChildRenderer(string objectName, int sortingLayer, int sortingOrder)
    {
        var go = new GameObject(objectName);
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingLayerID = sortingLayer;
        sr.sortingOrder = sortingOrder;
        return sr;
    }

    private static Texture2D GenerateCircleTexture(int res)
    {
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false)
        {
            name = "Generated Circle Texture",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var pixels = new Color32[res * res];
        float center = (res - 1) * 0.5f;
        float radius = res * 0.5f - 1f; // leave a 1px border for clean edges

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                // Anti-aliased edge: fade out over 1px.
                float a = Mathf.Clamp01(radius - d + 0.5f);
                pixels[y * res + x] = new Color32(255, 255, 255, (byte)(a * 255f));
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true); // upload and free the CPU copy
        return tex;
    }

    // ---------- Hold + hover logic ----------

    private void Update()
    {
        // Fade in while hovered (or mid-hold), out otherwise.
        float targetVisibility = (isHovered || isHolding) && Interactable ? 1f : 0f;
        float fadeStep = fadeDuration > 0f ? Time.unscaledDeltaTime / fadeDuration : 1f;
        visibility = Mathf.MoveTowards(visibility, targetVisibility, fadeStep);

        if (isHolding && !hasFired)
        {
            Progress += Time.unscaledDeltaTime / holdDuration;

            if (Progress >= 1f)
            {
                Progress = 1f;
                hasFired = true;
                isHolding = false;
                UpdateVisual();
                onHoldCompleted?.Invoke();
                return;
            }
        }
        else if (!isHolding && Progress > 0f)
        {
            if (drainOnRelease)
                Progress = Mathf.Max(0f, Progress - Time.unscaledDeltaTime / holdDuration * drainSpeedMultiplier);
            else
                Progress = 0f;
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (trackRenderer != null)
        {
            var c = trackColor;
            c.a *= visibility;
            trackRenderer.color = c;
        }

        if (fillRenderer != null)
        {
            var c = fillColor;
            c.a *= visibility;
            fillRenderer.color = c;

            fillRenderer.GetPropertyBlock(mpb);   // keep the sprite texture SpriteRenderer sets
            mpb.SetFloat(FillId, Progress);
            mpb.SetVector(UVRectId, new Vector4(0f, 0f, 1f, 1f)); // generated sprite uses the whole texture
            fillRenderer.SetPropertyBlock(mpb);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (cancelOnPointerExit) Release();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!Interactable || eventData.button != PointerEventData.InputButton.Left) return;

        isHolding = true;
        hasFired = false;
        Progress = 0f;
        onHoldStarted?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    private void Release()
    {
        if (isHolding && !hasFired)
            onHoldCanceled?.Invoke();

        isHolding = false;
    }

    private void OnDisable()
    {
        isHovered = false;
        isHolding = false;
        hasFired = false;
        Progress = 0f;
        visibility = 0f;
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (fillMaterial != null) Destroy(fillMaterial);
        if (circleSprite != null) Destroy(circleSprite);
        if (circleTexture != null) Destroy(circleTexture);
    }

#if UNITY_EDITOR
    // Grab the shader reference in the editor so it's serialized and included in builds.
    private void Reset() => AssignShader();
    private void OnValidate() => AssignShader();

    private void AssignShader()
    {
        if (radialFillShader == null)
            radialFillShader = Shader.Find(ShaderName);
    }
#endif
}