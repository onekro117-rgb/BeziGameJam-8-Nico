using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private void Awake()
    {
        // Obtener referencia si no está asignada
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        // Validar que tenemos la referencia necesaria
        if (playerHealth == null)
        {
            Debug.LogError("PlayerDeathHandler: No se encontró PlayerHealth en el Player!");
            return;
        }

        // Suscribirse SOLO al evento de muerte del HealthComponent
        playerHealth.OnDeath += HandlePlayerDeath;
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar memory leaks
        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    /// <summary>
    /// Se llama cuando el jugador muere (CurrentHealth = 0)
    /// Ahora activa Game Over directo, sin respawn
    /// </summary>
    private void HandlePlayerDeath()
    {
        Debug.Log("=== GAME OVER - PLAYER MURIÓ ===");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverPanel();
        }
        else
        {
            Debug.LogError("UIManager.Instance es null! No se puede mostrar Game Over Panel");
        }
    }
}