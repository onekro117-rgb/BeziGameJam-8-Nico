using UnityEngine;

/// <summary>
/// Configuración base para todos los enemigos.
/// Todos los enemigos heredan de esta clase y añaden sus parámetros específicos.
/// </summary>
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemies/Base Config")]
public class EnemyConfigBase : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Velocidad de patrulla")]
    public float patrolSpeed = 2f;

    [Tooltip("Velocidad al perseguir al jugador")]
    public float chaseSpeed = 4f;

    [Header("Detection")]
    [Tooltip("Rango de detección del jugador")]
    public float detectionRange = 5f;

    [Tooltip("Layer del jugador para raycast")]
    public LayerMask playerLayer = 1 << 6; // Asume que Player está en layer 6

    [Header("Combat")]
    [Tooltip("Daño que hace el enemigo al tocar al jugador")]
    public int contactDamage = 1;

    [Tooltip("Cooldown entre golpes de contacto para evitar spam")]
    public float contactDamageCooldown = 1f;

    [Header("Telegraph & Recovery")]
    [Tooltip("Tiempo de preparación antes de atacar (telegrafía visible)")]
    [Range(0.5f, 3f)]
    public float prepareTime = 0.8f;

    [Tooltip("Tiempo quieto tras fallar ataque (ventana de castigo)")]
    [Range(0.5f, 5f)]
    public float recoverTime = 1.5f;

    [Header("Visual Feedback")]
    [Tooltip("Color del sprite durante preparación")]
    public Color telegraphColor = Color.yellow;

    [Tooltip("Color del sprite durante recuperación")]
    public Color recoverColor = Color.gray;
}
