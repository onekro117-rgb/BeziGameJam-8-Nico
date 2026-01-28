using UnityEngine;

public class PlayerHealth : HealthComponent
{
    [Header("Player References")]
    [SerializeField] private PlayerMovement playerMovement;

    protected override void Awake()
    {
        base.Awake();

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }

    protected override void Die()
    {
        if (IsDead)
            return;

        base.Die(); // Invoca OnDeath y marca IsDead = true

        // Game Over directo, sin sistema de vidas
        Debug.Log("=== PLAYER MURIÓ - GAME OVER ===");
    }

    /// <summary>
    /// Resetea completamente la salud del jugador para un nuevo juego
    /// </summary>
    public void ResetForNewGame()
    {
        // Resetear la salud al máximo
        ResetHealth();

        Debug.Log("PlayerHealth: Salud reseteada para nuevo juego");
    }
}