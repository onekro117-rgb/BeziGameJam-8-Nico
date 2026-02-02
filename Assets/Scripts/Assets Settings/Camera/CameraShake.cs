using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera shake effects
/// Attach to Main Camera
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeDuration = 0.2f;

    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        originalPosition = transform.localPosition;
    }

    /// <summary>
    /// Trigger camera shake
    /// </summary>
    /// <param name="intensityMultiplier">Optional multiplier for intensity</param>
    public void Shake(float intensityMultiplier = 1f)
    {
        // Stop existing shake
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        // Start new shake
        shakeCoroutine = StartCoroutine(ShakeCoroutine(intensityMultiplier));
    }

    private IEnumerator ShakeCoroutine(float multiplier)
    {
        float elapsed = 0f;
        float effectiveIntensity = shakeIntensity * multiplier;

        while (elapsed < shakeDuration)
        {
            // Calculate progress
            float t = elapsed / shakeDuration;

            // Reduce intensity over time (ease-out)
            float currentIntensity = Mathf.Lerp(effectiveIntensity, 0f, t);

            // Random offset
            float offsetX = Random.Range(-1f, 1f) * currentIntensity;
            float offsetY = Random.Range(-1f, 1f) * currentIntensity;

            // Apply shake
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset position
        transform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

    /// <summary>
    /// Stop shake immediately
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        transform.localPosition = originalPosition;
    }
}
