using UnityEngine;

/// <summary>
/// Estado de patrulla - El enemigo patrulla entre dos puntos.
/// Transiciona a PrepareAttackState cuando detecta al jugador.
/// VERSIÓN FASE 2: Con transición completa a PrepareAttackState
/// </summary>
public class PatrolState : IEnemyState
{
    private EnemyContext context;
    private EnemyBrain brain;
    private Vector2 patrolPointA;
    private Vector2 patrolPointB;
    private float patrolSpeed;

    // Variables para feedback visual
    private bool wasPlayerInRange = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public PatrolState(EnemyContext context, EnemyBrain brain)
    {
        this.context = context;
        this.brain = brain;
    }

    public void Enter()
    {
        Debug.Log($"<color=green>[{context.Enemy.name}]</color> 🚶 Entrando en <b>PATROL</b>");

        // Verificar que el config existe
        if (context.Config == null)
        {
            Debug.LogError($"PatrolState: Config es NULL en {context.Enemy.name}!");
            return;
        }

        // Obtener sprite renderer para feedback visual
        spriteRenderer = context.Enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Configurar puntos de patrulla según tipo de enemigo
        SetupPatrolPoints();

        // Obtener velocidad de patrulla
        patrolSpeed = context.Config.patrolSpeed;
    }

    public void Tick(float deltaTime)
    {
        // Verificar que tenemos todo lo necesario
        if (context.Config == null || context.Movement == null) return;

        // Patrullar entre puntos
        context.Movement.PatrolBetweenPoints(patrolPointA, patrolPointB);

        // Detectar si el jugador está en rango
        bool playerInRange = context.Sensors != null &&
                            context.Sensors.PlayerInRange(context.Config.detectionRange);

        // Feedback visual: Cambiar color según detección
        if (spriteRenderer != null)
        {
            spriteRenderer.color = playerInRange ? Color.red : originalColor;
        }

        // Log cuando entra/sale del rango
        if (playerInRange && !wasPlayerInRange)
        {
            Debug.Log($"<color=yellow>[{context.Enemy.name}]</color> ⚠️ ¡JUGADOR DETECTADO EN RANGO!");
        }
        else if (!playerInRange && wasPlayerInRange)
        {
            Debug.Log($"<color=cyan>[{context.Enemy.name}]</color> ℹ️ Jugador salió del rango");
        }

        wasPlayerInRange = playerInRange;

        // Verificar si puede transicionar a ataque
        if (ShouldTransitionToAttack())
        {
            // FASE 2: Transicionar a PrepareAttackState
            brain.ChangeState(new PrepareAttackState(context, brain));
        }
    }

    public void Exit()
    {
        // Restaurar color original
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        if (context.Movement != null)
        {
            context.Movement.Stop();
        }
    }

    /// <summary>
    /// Configura los puntos de patrulla según el tipo de enemigo
    /// </summary>
    private void SetupPatrolPoints()
    {
        if (context.Config == null) return;

        float patrolDistance = 3f; // Valor por defecto

        // Obtener distancia de patrulla según tipo de config
        if (context.Config is ChargeEnemyConfig charge)
        {
            patrolDistance = charge.patrolDistance;
        }
        else if (context.Config is ShooterEnemyConfig shooter)
        {
            patrolDistance = shooter.patrolDistance;
        }
        else if (context.Config is FlyerEnemyConfig flyer)
        {
            patrolDistance = flyer.patrolAreaSize.x;
        }

        // Calcular puntos de patrulla desde spawn position
        patrolPointA = context.SpawnPosition + Vector2.left * patrolDistance;
        patrolPointB = context.SpawnPosition + Vector2.right * patrolDistance;
    }

    /// <summary>
    /// Verifica si debe transicionar al estado de ataque
    /// </summary>
    private bool ShouldTransitionToAttack()
    {
        // Verificar que tenemos todo lo necesario
        if (context.Config == null || context.Sensors == null) return false;

        // 1. Debe estar en rango
        if (!context.Sensors.PlayerInRange(context.Config.detectionRange))
            return false;

        // 2. El cooldown debe estar listo
        if (context.AttackCooldown == null || !context.AttackCooldown.IsReady)
            return false;

        // 3. El ataque debe poder ejecutarse
        if (context.Attack == null || !context.Attack.CanStartAttack())
            return false;

        return true;
    }
}