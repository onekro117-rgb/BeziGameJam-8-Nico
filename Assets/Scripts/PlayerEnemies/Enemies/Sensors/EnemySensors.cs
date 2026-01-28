using UnityEngine;

/// <summary>
/// Sistema de sensores para detectar al jugador.
/// Maneja detección por rango, line of sight y detección de idle (para Flyer).
/// </summary>
public class EnemySensors : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyConfigBase config;

    private Transform player;
    private Vector2 lastPlayerPosition;
    private float idleTimer;

    /// <summary>Posición actual del jugador</summary>
    public Vector2 PlayerPosition => player != null ? player.position : Vector2.zero;

    /// <summary>Última posición registrada del jugador</summary>
    public Vector2 LastPlayerPosition => lastPlayerPosition;

    private void Awake()
    {
        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"EnemySensors on {gameObject.name}: No se encontró jugador con tag 'Player'");
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
                if (context != null)
                {
                    config = context.Config;
                }
            }
        }

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Actualizar detección de idle
        UpdateIdleDetection();
    }

    /// <summary>
    /// Verifica si el jugador está dentro del rango de detección
    /// </summary>
    public bool PlayerInRange(float range)
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= range;
    }

    /// <summary>
    /// Verifica si hay línea de visión directa al jugador (sin obstáculos)
    /// </summary>
    public bool PlayerInLineOfSight()
    {
        if (player == null || config == null) return false;

        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Raycast hacia el jugador
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            distance,
            config.playerLayer
        );

        // Si el raycast golpea al jugador, hay línea de visión
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    /// <summary>
    /// Actualiza el temporizador de idle del jugador
    /// </summary>
    private void UpdateIdleDetection()
    {
        Vector2 currentPos = player.position;
        float movement = Vector2.Distance(currentPos, lastPlayerPosition);

        // Solo para FlyerEnemyConfig
        if (config is FlyerEnemyConfig flyerConfig)
        {
            // Si el movimiento es menor al umbral, incrementar timer
            if (movement < flyerConfig.idlePositionThreshold)
            {
                idleTimer += Time.deltaTime;
            }
            else
            {
                // Si se movió, resetear timer
                idleTimer = 0f;
                lastPlayerPosition = currentPos;
            }
        }
        else
        {
            // Para otros enemigos, solo actualizar posición
            lastPlayerPosition = currentPos;
        }
    }

    /// <summary>
    /// Verifica si el jugador está idle (quieto)
    /// </summary>
    public bool PlayerIsIdle()
    {
        if (config is FlyerEnemyConfig flyerConfig)
        {
            return idleTimer >= flyerConfig.idleDetectionTime;
        }
        return false;
    }

    /// <summary>
    /// Obtiene el tiempo que el jugador ha estado idle
    /// </summary>
    public float GetIdleTime()
    {
        return idleTimer;
    }

    /// <summary>
    /// Resetea el temporizador de idle
    /// </summary>
    public void ResetIdleTimer()
    {
        idleTimer = 0f;
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        // Dibujar rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.detectionRange);

        // Dibujar línea al jugador si está en rango
        if (player != null && PlayerInRange(config.detectionRange))
        {
            Gizmos.color = PlayerInLineOfSight() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // Para Flyer, mostrar info de idle
        if (config is FlyerEnemyConfig && player != null)
        {
            if (PlayerIsIdle())
            {
                // Dibujar círculo rojo si el jugador está idle
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(player.position, 0.5f);
            }
        }
    }
}
