using UnityEngine;

/// <summary>
/// Configuración específica para el enemigo que dispara proyectiles en arco.
/// Hereda todos los parámetros base y añade configuración de disparo.
/// </summary>
[CreateAssetMenu(fileName = "ShooterEnemyConfig", menuName = "Enemies/Shooter Config")]
public class ShooterEnemyConfig : EnemyConfigBase
{
    [Header("Shooting Settings")]
    [Tooltip("Prefab del proyectil a disparar")]
    public GameObject projectilePrefab;

    [Tooltip("Velocidad del proyectil")]
    [Range(3f, 12f)]
    public float projectileSpeed = 6f;

    [Tooltip("Altura del arco del mortero (más alto = más visible)")]
    [Range(1f, 6f)]
    public float arcHeight = 3f;

    [Tooltip("Tiempo apuntando antes de disparar (telegrafía)")]
    [Range(0.8f, 3f)]
    public float aimTime = 1.2f;

    [Tooltip("Cooldown entre disparos")]
    [Range(3f, 10f)]
    public float shootCooldown = 4f;

    [Header("Projectile")]
    [Tooltip("Daño del proyectil al impactar")]
    public int projectileDamage = 2;

    [Tooltip("Tiempo de vida del proyectil antes de autodestruirse")]
    public float projectileLifetime = 5f;

    [Header("Patrol")]
    [Tooltip("Distancia de patrulla desde el punto de spawn")]
    public float patrolDistance = 4f;

    [Header("Balance Recommendations")]
    [Tooltip("Valores recomendados:\n" +
             "- Fácil: aimTime=2.0s, arcHeight=4, shootCooldown=5s\n" +
             "- Normal: aimTime=1.5s, arcHeight=3, shootCooldown=4.5s\n" +
             "- Difícil: aimTime=1.0s, arcHeight=2, shootCooldown=3.5s")]
    [SerializeField] private string balanceNotes = "";
}
