using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main behavior for the rotating ring magic.
/// Spawns multiple colliders in a circle pattern and rotates them.
/// </summary>
public class RingMagicBehavior : MonoBehaviour
{
    [Header("Ring Configuration")]
    [SerializeField] private int numberOfColliders = 8;
    [SerializeField] private float ringRadius = 2.5f;
    [SerializeField] private float rotationSpeed = 180f; // degrees per second
    [SerializeField] private float duration = 5f;
    [SerializeField] private int damagePerHit = 2;
    [SerializeField] private float hitInterval = 0.5f; // time between hits on same enemy

    [Header("Collider Settings")]
    [SerializeField] private float colliderSize = 0.4f;

    [Header("Visual")]
    [SerializeField] private Color ringColor = Color.cyan;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private Transform playerTransform;
    private List<GameObject> colliderObjects = new List<GameObject>();
    private Dictionary<IDamageable, float> lastHitTime = new Dictionary<IDamageable, float>();
    private float lifetimeTimer = 0f;

    /// <summary>
    /// Initializes the ring with specific parameters
    /// </summary>
    public void Initialize(Transform player, float radius, int damage, float rotSpeed, float dur, float hitInt)
    {
        playerTransform = player;
        ringRadius = radius;
        damagePerHit = damage;
        rotationSpeed = rotSpeed;
        duration = dur;
        hitInterval = hitInt;

        SetupRing();

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[RingMagic]</color> Initialized - Radius: {radius}, Damage: {damage}, Duration: {duration}s");
        }
    }

    private void SetupRing()
    {
        // Create colliders arranged in a circle
        float angleStep = 360f / numberOfColliders;

        for (int i = 0; i < numberOfColliders; i++)
        {
            float angle = i * angleStep;
            float angleRad = angle * Mathf.Deg2Rad;

            // Calculate position on circle
            Vector3 localPosition = new Vector3(
                Mathf.Cos(angleRad) * ringRadius,
                Mathf.Sin(angleRad) * ringRadius,
                0f
            );

            // Create collider GameObject
            GameObject colliderObj = new GameObject($"RingCollider_{i}");
            colliderObj.transform.SetParent(transform);
            colliderObj.transform.localPosition = localPosition;
            colliderObj.layer = gameObject.layer;

            // Add CircleCollider2D
            CircleCollider2D circleCollider = colliderObj.AddComponent<CircleCollider2D>();
            circleCollider.isTrigger = true;
            circleCollider.radius = colliderSize;

            // Add RingDamageCollider script
            RingDamageCollider damageScript = colliderObj.AddComponent<RingDamageCollider>();
            damageScript.Initialize(this);

            // Add visual sprite (optional)
            SpriteRenderer sprite = colliderObj.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateCircleSprite();
            sprite.color = ringColor;
            sprite.sortingOrder = 10;

            colliderObjects.Add(colliderObj);
        }

        // Setup line renderer for ring visual
        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = ringColor;
        lineRenderer.endColor = ringColor;
        lineRenderer.sortingOrder = 9;
        lineRenderer.useWorldSpace = false;

        // Create circle with line renderer
        int segments = 64;
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 360f / segments;
            float angleRad = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Cos(angleRad) * ringRadius,
                Mathf.Sin(angleRad) * ringRadius,
                0f
            );

            lineRenderer.SetPosition(i, pos);
        }
    }

    private Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? Color.white : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void Update()
    {
        // Follow player
        if (playerTransform != null)
        {
            transform.position = playerTransform.position;
        }

        // Rotate ring
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        // Update lifetime
        lifetimeTimer += Time.deltaTime;

        // Fade out near end
        if (lifetimeTimer >= duration - 1f)
        {
            float fadeAlpha = 1f - ((lifetimeTimer - (duration - 1f)) / 1f);
            FadeRing(fadeAlpha);
        }

        // Destroy when duration ends
        if (lifetimeTimer >= duration)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>[RingMagic]</color> Duration ended, destroying ring");
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called by RingDamageCollider when it hits an enemy
    /// </summary>
    public void OnColliderHitEnemy(Collider2D collision)
    {
        // Get enemy damageable component
        IDamageable enemy = collision.GetComponent<IDamageable>();
        if (enemy == null)
        {
            // Try root
            enemy = collision.transform.root.GetComponent<IDamageable>();
        }

        if (enemy == null) return;

        // Check cooldown
        float currentTime = Time.time;
        if (lastHitTime.ContainsKey(enemy))
        {
            float timeSinceLastHit = currentTime - lastHitTime[enemy];
            if (timeSinceLastHit < hitInterval)
            {
                return; // Still in cooldown
            }
        }

        // Apply damage
        enemy.TakeDamage(damagePerHit);
        lastHitTime[enemy] = currentTime;

        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>[RingMagic]</color> Hit enemy! Dealt {damagePerHit} damage");
        }
    }

    private void FadeRing(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        // Fade line renderer
        if (lineRenderer != null)
        {
            Color color = ringColor;
            color.a = alpha;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        // Fade collider sprites
        foreach (var obj in colliderObjects)
        {
            SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                Color color = ringColor;
                color.a = alpha;
                sprite.color = color;
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up colliders
        foreach (var obj in colliderObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        colliderObjects.Clear();
        lastHitTime.Clear();
    }
}