using UnityEngine;

/// <summary>
/// Configuración específica para el enemigo que hace carga lineal.
/// Hereda todos los parámetros base y añade configuración de carga.
/// </summary>
[CreateAssetMenu(fileName = "ChargeEnemyConfig", menuName = "Enemies/Charge Config")]
public class ChargeEnemyConfig : EnemyConfigBase
{
    [Header("Charge Settings")]
    [Tooltip("Velocidad durante la carga (debe ser rápida pero esquivable)")]
    [Range(5f, 15f)]
    public float chargeSpeed = 8f;

    [Tooltip("Distancia máxima que puede recorrer en una carga")]
    public float chargeMaxDistance = 10f;

    [Tooltip("Duración máxima de la carga (seguridad si choca con pared)")]
    public float chargeMaxDuration = 2f;

    [Tooltip("Cooldown entre cargas")]
    [Range(2f, 10f)]
    public float chargeCooldown = 3f;

    [Header("Patrol")]
    [Tooltip("Distancia de patrulla desde el punto de spawn")]
    public float patrolDistance = 3f;

    [Header("Balance Recommendations")]
    [Tooltip("Valores recomendados:\n" +
             "- Fácil: prepareTime=1.2s, chargeSpeed=7, recoverTime=2.5s\n" +
             "- Normal: prepareTime=1.0s, chargeSpeed=8, recoverTime=2.0s\n" +
             "- Difícil: prepareTime=0.8s, chargeSpeed=10, recoverTime=1.5s")]
    [SerializeField] private string balanceNotes = "";
}
