using UnityEngine;

/// <summary>
/// Ataque de picada para el Enemy Flyer.
/// Se lanza en picada hacia donde ESTABA el jugador idle.
/// </summary>
public class DiveAttack : MonoBehaviour, IAttack
{
    [Header("Configuration")]
    [SerializeField] private FlyerEnemyConfig config;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private Rigidbody2D rb;
    private bool isDiving;
    private Vector2 diveTarget;
    private Vector2 diveStartPosition;

    public bool IsAttacking => isDiving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"DiveAttack on {gameObject.name}: No se encontró Rigidbody2D!");
        }
    }

    private void Start()
    {
        // Obtener config si no está asignado
        if (config == null)
        {
            EnemyBrain brain = GetComponent<EnemyBrain>();
            if (brain != null)
            {
                var context = brain.GetContext();
                if (context != null && context.Config is FlyerEnemyConfig flyerConfig)
                {
                    config = flyerConfig;
                }
            }
        }

        if (config == null)
        {
            Debug.LogError($"DiveAttack on {gameObject.name}: No se encontró FlyerEnemyConfig!");
        }
    }

    public bool CanStartAttack()
    {
        // Puede hacer dive si no está ya haciendo dive
        return !isDiving;
    }

    public void StartAttack()
    {
        if (isDiving || config == null) return;

        // Encontrar al jugador
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning($"DiveAttack: No se encontró el jugador!");
            return;
        }

        // SNAPSHOT: Fijar posición hacia donde ESTÁ el jugador AHORA
        diveTarget = player.position;
        diveStartPosition = transform.position;
        isDiving = true;

        if (showDebugLogs)
        {
            Debug.Log($"<color=magenta>[{gameObject.name}]</color> 🦅 Iniciando DIVE hacia {diveTarget}");
        }
    }

    private void FixedUpdate()
    {
        if (!isDiving || config == null) return;

        // Moverse hacia el objetivo fijado
        Vector2 direction = (diveTarget - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * config.diveSpeed;

        // Terminar si llegamos cerca del objetivo
        float distance = Vector2.Distance(transform.position, diveTarget);
        if (distance < 0.5f)
        {
            StopDive();
        }
    }

    private void StopDive()
    {
        if (!isDiving) return;

        isDiving = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[{gameObject.name}]</color> 🛑 Dive finalizado");
        }
    }

    public void CancelAttack()
    {
        StopDive();
    }

    private void OnDrawGizmosSelected()
    {
        if (!isDiving) return;

        // Dibujar objetivo de dive
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(diveTarget, 0.5f);
        Gizmos.DrawLine(transform.position, diveTarget);

        // Dibujar punto de inicio
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(diveStartPosition, 0.3f);
    }
}
