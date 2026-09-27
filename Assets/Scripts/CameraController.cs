using System;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Singleton
    static CameraController instance;
    private bool isActive = true;

    [SerializeField] private float moveSpeed = 3;

    // bounds
    [Header("Bounds")]
    [SerializeField] private float boundsWidth;
    [SerializeField] private float boundsHeight;

    private float maxX; private float minX;
    private float maxY; private float minY;
    private Vector2 boundsCenter;

    void Awake()
    {
        instance = this;
        CalculateBounds();
    }

    void CalculateBounds()
    {
        boundsCenter = transform.position;
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;

        float centerX = boundsCenter.x;
        float centerY = boundsCenter.y;

        minX = centerX - boundsWidth * 0.5f + halfWidth;
        maxX = centerX + boundsWidth * 0.5f - halfWidth;
        minY = centerY - boundsHeight * 0.5f + halfHeight;
        maxY = centerY + boundsHeight * 0.5f - halfHeight;
    }

    void Update()
    {
        if (!isActive)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 movementVect = new Vector2(x,y).normalized * moveSpeed * Time.deltaTime;
        Vector3 targetPos = transform.position + (Vector3) movementVect;
        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        transform.position = targetPos;
    }

    public static void SetActive(bool v)
    {
        instance.isActive = v;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = boundsCenter;
        Vector3 size = new Vector3(boundsWidth, boundsHeight, 0f);

        Gizmos.DrawWireCube(center, size);
    }
}
