using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset (player lower = more view above)")]
    [SerializeField] private Vector2 offset = new Vector2(0f, -2f);

    [Header("Dead Zone (in world units)")]
    [Tooltip("Width/Height of the dead zone rectangle in WORLD units.")]
    [SerializeField] private Vector2 deadZoneSize = new Vector2(4f, 2f);

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.15f;
    private Vector3 _velocity;
    
    [Header("Arena Bounds")]
    [Tooltip("BoxCollider2D that defines the arena boundaries. Camera will not move outside this area.")]
    [SerializeField] private BoxCollider2D arenaBounds;
    
    private Camera _camera;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 focus = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        Vector3 camPos = transform.position;

        float halfW = deadZoneSize.x * 0.5f;
        float halfH = deadZoneSize.y * 0.5f;

        float minX = camPos.x - halfW;
        float maxX = camPos.x + halfW;
        float minY = camPos.y - halfH;
        float maxY = camPos.y + halfH;

        float newX = camPos.x;
        float newY = camPos.y;

        if (focus.x < minX) newX += focus.x - minX;
        else if (focus.x > maxX) newX += focus.x - maxX;

        if (focus.y < minY) newY += focus.y - minY;
        else if (focus.y > maxY) newY += focus.y - maxY;

        Vector3 desired = new Vector3(newX, newY, camPos.z);

        if (arenaBounds != null && _camera != null)
        {
            desired = ClampToArenaBounds(desired);
        }

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
    }
    
    private Vector3 ClampToArenaBounds(Vector3 desiredPosition)
    {
        float cameraHalfHeight = _camera.orthographicSize;
        float cameraHalfWidth = cameraHalfHeight * _camera.aspect;
        
        Vector2 arenaCenter = (Vector2)arenaBounds.transform.position + arenaBounds.offset;
        Vector2 arenaSize = arenaBounds.size;
        
        float arenaMinX = arenaCenter.x - arenaSize.x * 0.5f;
        float arenaMaxX = arenaCenter.x + arenaSize.x * 0.5f;
        float arenaMinY = arenaCenter.y - arenaSize.y * 0.5f;
        float arenaMaxY = arenaCenter.y + arenaSize.y * 0.5f;
        
        float clampedX = Mathf.Clamp(
            desiredPosition.x,
            arenaMinX + cameraHalfWidth,
            arenaMaxX - cameraHalfWidth
        );
        
        float clampedY = Mathf.Clamp(
            desiredPosition.y,
            arenaMinY + cameraHalfHeight,
            arenaMaxY - cameraHalfHeight
        );
        
        return new Vector3(clampedX, clampedY, desiredPosition.z);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 camPos = Application.isPlaying ? transform.position : transform.position;
        Vector3 size = new Vector3(deadZoneSize.x, deadZoneSize.y, 0f);
        Gizmos.DrawWireCube(camPos, size);
        
        if (arenaBounds != null)
        {
            Gizmos.color = Color.red;
            Vector2 arenaCenter = (Vector2)arenaBounds.transform.position + arenaBounds.offset;
            Vector3 arenaSize3D = new Vector3(arenaBounds.size.x, arenaBounds.size.y, 0f);
            Gizmos.DrawWireCube(arenaCenter, arenaSize3D);
            
            Camera cam = GetComponent<Camera>();
            if (cam != null && cam.orthographic)
            {
                Gizmos.color = Color.cyan;
                float cameraHalfHeight = cam.orthographicSize;
                float cameraHalfWidth = cameraHalfHeight * cam.aspect;
                
                float arenaMinX = arenaCenter.x - arenaBounds.size.x * 0.5f;
                float arenaMaxX = arenaCenter.x + arenaBounds.size.x * 0.5f;
                float arenaMinY = arenaCenter.y - arenaBounds.size.y * 0.5f;
                float arenaMaxY = arenaCenter.y + arenaBounds.size.y * 0.5f;
                
                float clampedMinX = arenaMinX + cameraHalfWidth;
                float clampedMaxX = arenaMaxX - cameraHalfWidth;
                float clampedMinY = arenaMinY + cameraHalfHeight;
                float clampedMaxY = arenaMaxY - cameraHalfHeight;
                
                Vector3 clampedCenter = new Vector3(
                    (clampedMinX + clampedMaxX) * 0.5f,
                    (clampedMinY + clampedMaxY) * 0.5f,
                    camPos.z
                );
                Vector3 clampedSize = new Vector3(
                    clampedMaxX - clampedMinX,
                    clampedMaxY - clampedMinY,
                    0f
                );
                
                Gizmos.DrawWireCube(clampedCenter, clampedSize);
            }
        }
    }
#endif
}

