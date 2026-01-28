using UnityEngine;

/// <summary>
/// Proyectil que viaja en arco (mortero) hacia un objetivo fijo.
/// Se usa para el Enemy Shooter.
/// </summary>
public class MortarProjectile : MonoBehaviour
{
    private Vector2 targetPosition;
    private float speed;
    private float arcHeight;
    private int damage;
    private float lifetime;

    private Vector2 startPosition;
    private float journeyLength;
    private float distanceTravelled;
    private bool initialized;

    /// <summary>
    /// Inicializa el proyectil con sus parámetros
    /// </summary>
    public void Initialize(Vector2 target, float projectileSpeed, float arc, int dmg, float life)
    {
        targetPosition = target;
        speed = projectileSpeed;
        arcHeight = arc;
        damage = dmg;
        lifetime = life;

        startPosition = transform.position;
        journeyLength = Vector2.Distance(startPosition, targetPosition);
        distanceTravelled = 0f;
        initialized = true;

        // Autodestruirse después del lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!initialized || journeyLength <= 0f) return;

        // Moverse hacia el objetivo
        float step = speed * Time.deltaTime;
        distanceTravelled += step;

        // Calcular progreso (0 a 1)
        float progress = Mathf.Clamp01(distanceTravelled / journeyLength);

        // Posición lineal (interpolación)
        Vector2 currentPos = Vector2.Lerp(startPosition, targetPosition, progress);

        // Añadir arco (parábola)
        float heightOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
        currentPos.y += heightOffset;

        transform.position = currentPos;

        // Rotar según la dirección del movimiento
        if (progress < 1f)
        {
            Vector2 direction = currentPos - (Vector2)transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Destruir si alcanzamos el objetivo
        if (progress >= 1f)
        {
            OnReachTarget();
        }
    }

    private void OnReachTarget()
    {
        // TODO: Añadir efecto visual de impacto
        Debug.Log($"Proyectil alcanzó objetivo en {targetPosition}");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform root = collision.transform.root;

        // Dañar al jugador
        if (root.CompareTag("Player"))
        {
            IDamageable playerHealth = root.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Proyectil golpeó al jugador ({damage} daño)");
            }

            Destroy(gameObject);
            return;
        }

        // Destruir si choca con paredes/suelo
        if (collision.CompareTag("Ground") || collision.CompareTag("Wall"))
        {
            Debug.Log($"Proyectil chocó con {collision.gameObject.name}");
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (!initialized || journeyLength <= 0f) return;

        // Dibujar trayectoria predicha
        Gizmos.color = Color.cyan;

        Vector2 lastPos = startPosition;
        int segments = 20;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector2 pos = Vector2.Lerp(startPosition, targetPosition, t);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            if (i > 0)
            {
                Gizmos.DrawLine(lastPos, pos);
            }
            lastPos = pos;
        }

        // Dibujar objetivo
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
    }
}
