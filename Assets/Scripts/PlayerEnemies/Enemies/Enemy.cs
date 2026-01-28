using UnityEngine;

[RequireComponent(typeof(HealthEnemies))]
public class Enemy : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;

    private IDamageable health;
    private Rigidbody2D rb;

    private void Awake()
    {
        health = GetComponent<IDamageable>();
        rb = GetComponent<Rigidbody2D>();

        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
            ApplyKnockback(knockbackDirection);
        }
    }

    private void ApplyKnockback(Vector2 direction)
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction.x * knockbackForce, knockbackForce);
        }
    }

    private void HandleDeath()
    {
        Debug.Log($"{gameObject.name} died!");
        Destroy(gameObject);
    }
}
