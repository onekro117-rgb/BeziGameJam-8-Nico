using UnityEngine;

/// <summary>
/// Component on Player that spawns the ring magic.
/// Integrates with MagicSystem to receive QTE results.
/// </summary>
public class RingMagicSpawner : MonoBehaviour
{
    [Header("Ring Prefab")]
    [SerializeField] private GameObject ringMagicPrefab;

    [Header("Perfect QTE Values")]
    [SerializeField] private float perfectRadius = 2.5f;
    [SerializeField] private int perfectDamage = 2;
    [SerializeField] private float perfectDuration = 5f;
    [SerializeField] private float perfectRotationSpeed = 180f;
    [SerializeField] private float perfectHitInterval = 0.5f;
    [SerializeField] private Color perfectColor = Color.cyan;

    [Header("Partial QTE Values")]
    [SerializeField] private float partialRadius = 2.0f;
    [SerializeField] private int partialDamage = 1;
    [SerializeField] private float partialDuration = 3f;
    [SerializeField] private float partialRotationSpeed = 180f;
    [SerializeField] private float partialHitInterval = 0.75f;
    [SerializeField] private Color partialColor = Color.yellow;

    private GameObject currentRing;

    /// <summary>
    /// Spawns ring with perfect QTE values
    /// </summary>
    public void SpawnPerfectRing()
    {
        SpawnRing(perfectRadius, perfectDamage, perfectRotationSpeed,
                  perfectDuration, perfectHitInterval, perfectColor);
    }

    /// <summary>
    /// Spawns ring with partial QTE values
    /// </summary>
    public void SpawnPartialRing()
    {
        SpawnRing(partialRadius, partialDamage, partialRotationSpeed,
                  partialDuration, partialHitInterval, partialColor);
    }

    private void SpawnRing(float radius, int damage, float rotSpeed,
                           float duration, float hitInterval, Color color)
    {
        // Destroy existing ring if any
        if (currentRing != null)
        {
            Destroy(currentRing);
        }

        // Spawn ring at player position
        currentRing = Instantiate(ringMagicPrefab, transform.position, Quaternion.identity);

        // Get RingMagicBehavior and initialize
        RingMagicBehavior behavior = currentRing.GetComponent<RingMagicBehavior>();
        if (behavior != null)
        {
            behavior.Initialize(transform, radius, damage, rotSpeed, duration, hitInterval);
        }
        else
        {
            Debug.LogError("RingMagicSpawner: RingMagicPrefab doesn't have RingMagicBehavior component!");
        }

        Debug.Log($"<color=cyan>[RingMagicSpawner]</color> Ring spawned - Radius: {radius}, Damage: {damage}");
    }
}
