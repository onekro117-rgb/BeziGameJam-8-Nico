using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State { Playing, ChoosingModifier, GameOver }
    public State CurrentState { get; private set; } = State.Playing;

    [Header("References")]
    // Ya no necesitamos HealthSystem en el sistema sin vidas
    // [SerializeField] private HealthSystem healthSystem;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private MagicSystem magicSystem;
    [SerializeField] private ModifierManager modifierManager;

    private ModifierData[] _currentOptions;
    private ModifierOffer[] _currentOffers;

    // Ya no exponemos HealthSystem
    // public HealthSystem HealthSystem => healthSystem;
    public PlayerMovement PlayerMovement => playerMovement;

    public void OnLevelCompleted()
    {
        CurrentState = State.ChoosingModifier;
        Time.timeScale = 0f;

        _currentOffers = modifierManager.GenerateOffers(3);

        UIManager.Instance.ShowModifierChoices(_currentOffers, OnModifierChosen);
    }

    private void OnModifierChosen(int index)
    {
        Time.timeScale = 1f;

        _currentOffers[index].Apply(this);

        ResetLevelKeepLives();
        CurrentState = State.Playing;
    }

    private void ResetLevelKeepLives()
    {
        // IMPORTANTE: NO resetear vidas aquí

        // Respawn player (resetea movimiento / posición)
        playerMovement.Respawn();

        // Resetear enemigos/obstáculos si tienes spawners
        // EnemySpawner.ResetAll();
        // ProjectileManager.Clear();
        // etc.
    }

    /// <summary>
    /// Resetea completamente el juego al estado inicial.
    /// Se llama cuando el jugador presiona Retry en el Game Over.
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("=== INICIANDO RESETEO COMPLETO DEL JUEGO ===");

        // 1. Resetear estado del GameManager
        CurrentState = State.Playing;

        // 2. Resetear sistemas del jugador
        // 2.1 - Salud del jugador
        if (playerHealth != null)
        {
            playerHealth.ResetForNewGame();
            Debug.Log("✓ Salud del jugador reseteada");
        }
        else
        {
            Debug.LogWarning("⚠ PlayerHealth no asignado en GameManager");
        }

        // 2.2 - Ya no reseteamos vidas porque no usamos HealthSystem
        // El sistema ahora es: Salud = 0 → Game Over directo

        // 2.3 - Posición y movimiento
        if (playerMovement != null)
        {
            playerMovement.Respawn();
            Debug.Log("✓ Posición del jugador reseteada");
        }
        else
        {
            Debug.LogWarning("⚠ PlayerMovement no asignado en GameManager");
        }

        // 2.4 - Combate
        if (playerCombat != null)
        {
            playerCombat.ResetCombat();
            Debug.Log("✓ Sistema de combate reseteado");
        }
        else
        {
            Debug.LogWarning("⚠ PlayerCombat no asignado en GameManager");
        }

        // 2.5 - Sistema de magia
        if (magicSystem != null)
        {
            magicSystem.ResetMagic();
            Debug.Log("✓ Sistema de magia reseteado");
        }
        else
        {
            Debug.LogWarning("⚠ MagicSystem no asignado en GameManager");
        }

        // 3. Resetear modificadores (revertir todos los efectos)
        if (modifierManager != null)
        {
            modifierManager.RevertAll(this);
            Debug.Log("✓ Modificadores revertidos");
        }
        else
        {
            Debug.LogWarning("⚠ ModifierManager no asignado en GameManager");
        }

        // 4. Resetear multiplicadores de movimiento
        if (playerMovement != null)
        {
            playerMovement.ResetMultipliers();
            Debug.Log("✓ Multiplicadores de movimiento reseteados");
        }

        // 5. Resetear sistema de oleadas
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RestartWaves();
            Debug.Log("✓ Sistema de oleadas reseteado");
        }
        else
        {
            Debug.LogWarning("⚠ WaveManager.Instance es null");
        }

        Debug.Log("=== JUEGO RESETEADO COMPLETAMENTE ===");
    }
}