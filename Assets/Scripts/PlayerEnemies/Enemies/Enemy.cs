using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class Enemy : MonoBehaviour
{
    private HealthComponent health;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();

        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"{gameObject.name} died!");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }
}
