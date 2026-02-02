using UnityEngine;

[CreateAssetMenu(menuName = "Modifiers/Good/Heal Health")]
public class AddLifeModifier : ModifierData
{
    [Header("Heal Settings")]
    [Tooltip("Amount of HP to heal")]
    [SerializeField] private int healAmount = 1;

    public override IRevertibleEffect Apply(GameManager gameManager)
    {
        if (gameManager == null || gameManager.Player == null)
        {
            Debug.LogError("HealHealthModifier: GameManager or Player is null.");
            return null;
        }

        // Get PlayerHealth component
        HealthComponent health = gameManager.Player.GetComponent<HealthComponent>();
        if (health == null)
        {
            Debug.LogError("HealHealthModifier: No HealthComponent found on Player.");
            return null;
        }

        // Heal the player
        health.Heal(healAmount);

        Debug.Log($"<color=green>[HealHealth]</color> Healed {healAmount} HP. Current: {health.CurrentHealth}/{health.MaxHealth}");

        return null; // Not reversible (healing is instant)
    }
}



