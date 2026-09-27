using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class Draggable : MonoBehaviour
{
    [Header("Spring")]
    [Tooltip("How fast the grab point chases the cursor, in Hz. Higher = stiffer.")]
    [SerializeField, Range(0.5f, 20f)] private float frequency = 6f;

    [Tooltip("1 = no overshoot. Below 1 = bouncy/springy. Above 1 = sluggish.")]
    [SerializeField, Range(0f, 2f)] private float dampingRatio = 1f;

    [Tooltip("Caps how hard you can pull (m/s^2). Scaled by mass, so heavy objects still feel heavy against collisions.")]
    [SerializeField] private float maxAcceleration = 150f;

    [Header("Feel")]
    [Tooltip("Angular damping while held, so objects swing and settle instead of spinning forever.")]
    [SerializeField] private float angularDampingWhileHeld = 3f;

    [Tooltip("Speed cap when thrown.")]
    [SerializeField] private float maxReleaseSpeed = 25f;

    private Rigidbody2D rb;
    private TargetJoint2D joint;
    private Camera cam;

    private bool isDragging;
    private float originalAngularDamping;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // Smooth visuals between physics steps, and avoid tunnelling when thrown.
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // A spring solved inside the physics engine (Box2D's "mouse joint").
        // Because it's solved implicitly, it stays stable no matter where
        // you grab or how stiff you make it — no jitter.
        joint = gameObject.AddComponent<TargetJoint2D>();
        joint.autoConfigureTarget = false;
        joint.enabled = false;
    }

    private Vector2 GetMouseWorld() => cam.ScreenToWorldPoint(Input.mousePosition);
    
    public virtual void StartDrag()
    {
        Vector2 mouse = GetMouseWorld();
        isDragging = true;

        // Pull from the exact point that was clicked, so objects hang,
        // swing and tilt naturally.
        joint.anchor = transform.InverseTransformPoint(mouse);
        joint.target = mouse;
        joint.frequency = frequency;
        joint.dampingRatio = dampingRatio;
        joint.maxForce = maxAcceleration * rb.mass;
        joint.enabled = true;

        originalAngularDamping = rb.angularDamping;
        rb.angularDamping = angularDampingWhileHeld;

        OnGrab();
    }

    void OnMouseDown() => StartDrag();

    void Update()
    {
        if (isDragging)
            joint.target = GetMouseWorld();
        if (isDragging && Input.GetMouseButtonUp(0))
            Release();
    }

    void OnMouseUp() => Release();

    protected virtual void OnDisable() => Release();

    private void Release()
    {
        if (!isDragging)
            return;

        isDragging = false;
        joint.enabled = false;
        rb.angularDamping = originalAngularDamping;

        // Keep momentum so it can be thrown, but within reason.
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxReleaseSpeed);

        OnRelease();
    }

    protected abstract void OnGrab();
    protected abstract void OnRelease();
}