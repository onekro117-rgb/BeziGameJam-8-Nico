using UnityEngine;

/// <summary>
/// Configuración específica para el enemigo volador que hace dive attacks.
/// Hereda todos los parámetros base y añade configuración de vuelo y dive.
/// </summary>
[CreateAssetMenu(fileName = "FlyerEnemyConfig", menuName = "Enemies/Flyer Config")]
public class FlyerEnemyConfig : EnemyConfigBase
{
    [Header("Flight Settings")]
    [Tooltip("Altura a la que vuela sobre el suelo")]
    [Range(2f, 6f)]
    public float hoverHeight = 3f;

    [Tooltip("Distancia que mantiene del jugador mientras lo persigue")]
    [Range(2f, 8f)]
    public float followDistance = 4f;

    [Tooltip("Velocidad mientras vuela/patrulla")]
    [Range(1f, 5f)]
    public float hoverSpeed = 2f;

    [Header("Idle Detection")]
    [Tooltip("Tiempo que el jugador debe estar quieto para activar dive")]
    [Range(1f, 5f)]
    public float idleDetectionTime = 2f;

    [Tooltip("Umbral de movimiento para considerar al jugador quieto")]
    [Range(0.05f, 0.3f)]
    public float idlePositionThreshold = 0.1f;

    [Header("Dive Attack")]
    [Tooltip("Velocidad del dive (debe ser muy rápida)")]
    [Range(8f, 20f)]
    public float diveSpeed = 12f;

    [Tooltip("Tiempo de preparación antes del dive (telegrafía MUY clara)")]
    [Range(0.8f, 2f)]
    public float divePrepareTime = 1.0f;

    [Tooltip("Cooldown entre dives")]
    [Range(4f, 10f)]
    public float diveCooldown = 5f;

    [Tooltip("Tiempo de recuperación tras fallar dive")]
    [Range(1.5f, 4f)]
    public float diveRecoverTime = 2.5f;

    [Header("Patrol")]
    [Tooltip("Tamaño del área de patrulla aérea")]
    public Vector2 patrolAreaSize = new Vector2(5f, 2f);

    [Header("Balance Recommendations")]
    [Tooltip("Valores recomendados:\n" +
             "- Fácil: idleTime=1.5s, divePrepare=1.5s, diveSpeed=10\n" +
             "- Normal: idleTime=2.5s, divePrepare=1.2s, diveSpeed=12\n" +
             "- Difícil: idleTime=3.5s, divePrepare=1.0s, diveSpeed=15")]
    [SerializeField] private string balanceNotes = "";
}
