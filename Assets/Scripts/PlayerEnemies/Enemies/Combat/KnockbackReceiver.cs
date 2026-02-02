using System.Collections;
using UnityEngine;

/// <summary>
/// Component that handles knockback when entity receives damage.
/// Apply to any GameObject that should be knocked back (Player, Enemies).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Force of the knockback")]
    [SerializeField] private float knockbackForce = 5f;

    [Tooltip("Duration of the knockback in seconds")]
    [SerializeField] private float knockbackDuration = 0.2f;

    [Tooltip("Curve for knockback falloff (ease-out)")]
    [SerializeField] private AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    // Public state
    public bool IsKnockedBack { get; private set; }

    // Private references
    private Rigidbody2D rb;
    private Coroutine currentKnockbackCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"KnockbackReceiver on {gameObject.name}: No Rigidbody2D found!");
        }
    }

    /// <summary>
    /// Apply knockback in a direction
    /// </summary>
    /// <param name="direction">Normalized direction of knockback</param>
    /// <param name="forceMultiplier">Optional multiplier for force (default 1.0)</param>
    public void ApplyKnockback(Vector2 direction, float forceMultiplier = 1f)
    {
        if (rb == null)
        {
            Debug.LogWarning($"KnockbackReceiver on {gameObject.name}: Cannot apply knockback, no Rigidbody2D!");
            return;
        }

        // Stop any existing knockback
        if (currentKnockbackCoroutine != null)
        {
            StopCoroutine(currentKnockbackCoroutine);
        }

        // Start new knockback
        float effectiveForce = knockbackForce * forceMultiplier;
        currentKnockbackCoroutine = StartCoroutine(KnockbackCoroutine(direction.normalized, effectiveForce));

        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>[Knockback]</color> {gameObject.name} knocked back with force {effectiveForce}");
        }
    }

    /// <summary>
    /// Coroutine that applies knockback over time with ease-out
    /// </summary>
    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        IsKnockedBack = true;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            // Calculate progress (0 to 1)
            float t = elapsed / knockbackDuration;

            // Apply curve for ease-out effect
            float curveValue = knockbackCurve.Evaluate(t);

            // Calculate current velocity based on curve
            float currentForce = force * curveValue;

            // Apply velocity
            rb.linearVelocity = direction * currentForce;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // End knockback
        IsKnockedBack = false;
        currentKnockbackCoroutine = null;

        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[Knockback]</color> {gameObject.name} knockback ended");
        }
    }

    /// <summary>
    /// Stop knockback immediately
    /// </summary>
    public void CancelKnockback()
    {
        if (currentKnockbackCoroutine != null)
        {
            StopCoroutine(currentKnockbackCoroutine);
            currentKnockbackCoroutine = null;
        }

        IsKnockedBack = false;

        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>[Knockback]</color> {gameObject.name} knockback cancelled");
        }
    }

    private void OnDestroy()
    {
        // Clean up coroutine
        if (currentKnockbackCoroutine != null)
        {
            StopCoroutine(currentKnockbackCoroutine);
        }
    }
}
