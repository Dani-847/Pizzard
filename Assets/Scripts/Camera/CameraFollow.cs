using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Tracking")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 0f, -10f);
    public float smoothSpeed = 5f;

    [Header("Deadzone (Free movement area)")]
    public Vector2 deadzoneSize = new Vector2(0f, 0f);

    [Header("Zoom")]
    public bool enableDynamicZoom = true;
    public float defaultZoom = 5f;
    public float bossArenaZoom = 8f;
    public float zoomSpeed = 2f;

    [Header("Room Bounds")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;
    private Vector3 currentVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
        
        // Auto-assign player if missing 
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
            else return;
        }

        // 1. Calculate deadzone center
        Vector3 targetPos = target.position + offset;
        Vector3 desiredPosition = transform.position;

        // X axis deadzone
        float distanceX = targetPos.x - transform.position.x;
        if (Mathf.Abs(distanceX) > deadzoneSize.x)
        {
            desiredPosition.x += distanceX - (Mathf.Sign(distanceX) * deadzoneSize.x);
        }

        // Y axis deadzone
        float distanceY = targetPos.y - transform.position.y;
        if (Mathf.Abs(distanceY) > deadzoneSize.y)
        {
            desiredPosition.y += distanceY - (Mathf.Sign(distanceY) * deadzoneSize.y);
        }

        // 2. Smooth movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. Dynamic Zoom
        if (enableDynamicZoom && cam != null)
        {
            // If we have very broad bounds set, or specific flags, we widen the view.
            float targetZ = useBounds ? bossArenaZoom : defaultZoom;
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZ, zoomSpeed * Time.deltaTime);
        }

        // 4. Clamping (Room Bounds)
        if (useBounds && cam != null)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = cam.orthographicSize * cam.aspect;
            
            float clampedX = Mathf.Clamp(transform.position.x, minBounds.x + camWidth, maxBounds.x - camWidth);
            float clampedY = Mathf.Clamp(transform.position.y, minBounds.y + camHeight, maxBounds.y - camHeight);
            
            // Si el cuarto es más pequeño que la cámara, centramos
            if (minBounds.x + camWidth > maxBounds.x - camWidth) clampedX = (minBounds.x + maxBounds.x) / 2f;
            if (minBounds.y + camHeight > maxBounds.y - camHeight) clampedY = (minBounds.y + maxBounds.y) / 2f;

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }

        transform.rotation = Quaternion.identity;
    }

    private void OnDrawGizmos()
    {
        // Draw Deadzone
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(deadzoneSize.x * 2, deadzoneSize.y * 2, 0));

        // Draw Room Bounds
        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
