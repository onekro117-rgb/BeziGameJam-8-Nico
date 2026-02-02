using UnityEngine;

/// <summary>
/// Componente genérico que aplica daño al jugador por contacto.
/// Incluye cooldown para evitar spam de daño.
/// </summary>
public class EnemyDamageOnTouch : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyConfigBase config;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private CooldownTimer damageCooldown;

    private void Awake()
    {
        // Inicializar cooldown
        if (config != null)
        {
            damageCooldown = new CooldownTimer(config.contactDamageCooldown);
        }
        else
        {
            // Valor por defecto si no hay config
            damageCooldown = new CooldownTimer(1f);
            Debug.LogWarning($"EnemyDamageOnTouch on {gameObject.name}: No hay config asignado. Usando cooldown por defecto de 1s.");
        }
    }

    private void Start()
    {
        // Intentar obtener config del EnemyBrain si no está asignado
        if (config == null)
        {
            EnemyBrain brain = GetComponent<EnemyBrain>();
            if (brain != null)
            {
                var context = brain.GetContext();
                if (context != null)
                {
                    config = context.Config;
                    damageCooldown.SetDuration(config.contactDamageCooldown);
                }
            }
        }
    }

    private void Update()
    {
        // Actualizar cooldown
        damageCooldown.Tick(Time.deltaTime);
    }

    /// <summary>
    /// Detecta colisión con el jugador y aplica daño
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Verificar si el cooldown está listo
        if (!damageCooldown.IsReady) return;

        // Obtener el root del objeto (en caso de que sea un hijo)
        Transform root = collision.transform.root;

        // Verificar si es el jugador
        if (!root.CompareTag("Player")) return;

        // Buscar el componente IDamageable
        IDamageable playerHealth = root.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            // Determinar el daño
            int damage = config != null ? config.contactDamage : 1;

            // Aplicar daño
            Vector2 enemyPosition = transform.position;
            playerHealth.TakeDamage(damage, enemyPosition);

            // Iniciar cooldown
            damageCooldown.StartCooldown();

            if (showDebugLogs)
            {
                Debug.Log($"<color=red>[{gameObject.name}]</color> Daño por contacto: <b>{damage}</b> al jugador");
            }
        }
        else
        {
            Debug.LogWarning($"EnemyDamageOnTouch: El jugador no tiene componente IDamageable!");
        }
    }

    /// <summary>
    /// Alternativa usando Trigger en vez de Collision
    /// Descomenta si tus enemigos usan Trigger Colliders
    /// </summary>
    /*
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!damageCooldown.IsReady) return;
        
        Transform root = collision.transform.root;
        if (!root.CompareTag("Player")) return;
        
        IDamageable playerHealth = root.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            int damage = config != null ? config.contactDamage : 1;
            playerHealth.TakeDamage(damage);
            damageCooldown.StartCooldown();
            
            if (showDebugLogs)
            {
                Debug.Log($"<color=red>[{gameObject.name}]</color> Daño por contacto (Trigger): <b>{damage}</b>");
            }
        }
    }
    */
}
