using UnityEngine;

/// <summary>
/// Contenedor de referencias y estado compartido para el sistema de enemigos.
/// Se pasa entre los estados del FSM para que todos accedan a la misma información.
/// </summary>
public class EnemyContext
{
    // ===== REFERENCIAS CORE =====
    /// <summary>Transform del enemigo</summary>
    public Transform Enemy { get; private set; }

    /// <summary>Transform del jugador</summary>
    public Transform Player { get; private set; }

    /// <summary>Rigidbody2D del enemigo para movimiento</summary>
    public Rigidbody2D Rigidbody { get; private set; }

    // ===== CONFIGURACIÓN =====
    /// <summary>Configuración del enemigo (SO)</summary>
    public EnemyConfigBase Config { get; private set; }

    // ===== MÓDULOS =====
    /// <summary>Sistema de movimiento del enemigo</summary>
    public IEnemyMovement Movement { get; set; }

    /// <summary>Sistema de sensores/detección del enemigo</summary>
    public EnemySensors Sensors { get; set; }

    /// <summary>Sistema de ataque del enemigo</summary>
    public IAttack Attack { get; set; }

    // ===== ESTADO =====
    /// <summary>Cooldown del ataque</summary>
    public CooldownTimer AttackCooldown { get; private set; }

    /// <summary>Última posición conocida del jugador (snapshot)</summary>
    public Vector2 LastKnownPlayerPosition { get; set; }

    /// <summary>Posición inicial del enemigo al spawnearse</summary>
    public Vector2 SpawnPosition { get; private set; }

    /// <summary>
    /// Constructor del contexto
    /// </summary>
    public EnemyContext(
        Transform enemy,
        Transform player,
        Rigidbody2D rb,
        EnemyConfigBase config)
    {
        Enemy = enemy;
        Player = player;
        Rigidbody = rb;
        Config = config;
        SpawnPosition = enemy.position;

        // Inicializar cooldown según tipo de config
        float cooldownDuration = GetCooldownDuration(config);
        AttackCooldown = new CooldownTimer(cooldownDuration);
    }

    /// <summary>
    /// Obtiene la duración del cooldown según el tipo de enemigo
    /// </summary>
    private float GetCooldownDuration(EnemyConfigBase config)
    {
        if (config is ChargeEnemyConfig charge)
            return charge.chargeCooldown;

        if (config is ShooterEnemyConfig shooter)
            return shooter.shootCooldown;

        if (config is FlyerEnemyConfig flyer)
            return flyer.diveCooldown;

        return 3f; // Valor por defecto
    }
}
