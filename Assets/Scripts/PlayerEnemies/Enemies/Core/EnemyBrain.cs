using UnityEngine;

/// <summary>
/// Cerebro del enemigo - Gestiona el FSM (Finite State Machine).
/// Controla las transiciones entre estados y coordina todos los componentes.
/// </summary>
public class EnemyBrain : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private EnemyConfigBase config;

    [Header("References")]
    [SerializeField] private Transform player;

    private EnemyContext context;
    private IEnemyState currentState;
    private Rigidbody2D rb;

    // Componentes modulares
    private IEnemyMovement movement;
    private EnemySensors sensors;
    private IAttack attack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Encontrar player si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError($"EnemyBrain on {gameObject.name}: No se encontró el jugador con tag 'Player'!");
            }
        }

        // Inicializar componentes
        InitializeComponents();

        // Crear contexto compartido
        context = new EnemyContext(transform, player, rb, config);
        context.Movement = movement;
        context.Sensors = sensors;
        context.Attack = attack;

        // Estado inicial: Patrol
        ChangeState(new PatrolState(context, this));
    }

    private void InitializeComponents()
    {
        // Obtener o añadir componentes según lo que tenga el enemigo
        movement = GetComponent<IEnemyMovement>();
        if (movement == null)
        {
            // Si no tiene IEnemyMovement, añadir EnemyMovement por defecto
            movement = gameObject.AddComponent<EnemyMovement>();
            Debug.Log($"EnemyBrain: Añadido EnemyMovement automáticamente a {gameObject.name}");
        }

        sensors = GetComponent<EnemySensors>();
        if (sensors == null)
        {
            sensors = gameObject.AddComponent<EnemySensors>();
            Debug.Log($"EnemyBrain: Añadido EnemySensors automáticamente a {gameObject.name}");
        }

        attack = GetComponent<IAttack>();
        if (attack == null)
        {
            Debug.LogError($"EnemyBrain on {gameObject.name}: No se encontró ningún componente IAttack! " +
                          "Añade ChargeAttack, ShootAttack o DiveAttack.");
        }
    }

    private void Update()
    {
        if (currentState == null || context == null)
            return;

        // Tick del estado actual
        currentState.Tick(Time.deltaTime);

        // Tick del cooldown de ataque
        context.AttackCooldown.Tick(Time.deltaTime);
    }

    /// <summary>
    /// Cambia el estado actual del FSM
    /// </summary>
    public void ChangeState(IEnemyState newState)
    {
        if (newState == null)
        {
            Debug.LogError($"EnemyBrain: Intentando cambiar a un estado NULL!");
            return;
        }

        // Salir del estado actual
        currentState?.Exit();

        // Cambiar al nuevo estado
        currentState = newState;

        // Entrar al nuevo estado
        currentState.Enter();

        Debug.Log($"<color=cyan>[{gameObject.name}]</color> Estado: <b>{newState.GetType().Name}</b>");
    }

    /// <summary>
    /// Obtiene el contexto (útil para debugging)
    /// </summary>
    public EnemyContext GetContext()
    {
        return context;
    }

    private void OnDrawGizmosSelected()
    {
        if (config == null) return;

        // Dibujar rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.detectionRange);

        // Dibujar spawn position
        if (Application.isPlaying && context != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(context.SpawnPosition, 0.3f);
            Gizmos.DrawLine(transform.position, context.SpawnPosition);
        }
    }
}
