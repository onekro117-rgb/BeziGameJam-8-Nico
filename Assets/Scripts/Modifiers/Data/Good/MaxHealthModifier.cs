using UnityEngine;

[CreateAssetMenu(menuName = "Modifiers/Good/Increase Max Health")]
public class MaxHealthModifier : ModifierData
{
    [Header("Health Increase")]
    [Range(1f, 200f)]
    [Tooltip("Porcentaje de aumento de vida máxima (ej: 25 = +25% vida)")]
    public float percent = 25f;

    public override IRevertibleEffect Apply(GameManager gameManager)
    {
        if (gameManager == null) return null;

        // Obtener el PlayerHealth del GameManager
        PlayerHealth playerHealth = gameManager.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("MaxHealthModifier: No se encuentra PlayerHealth en GameManager.");
            return null;
        }

        // Calcular multiplicador (25% = 1.25x)
        float mult = 1f + percent / 100f;

        // Aplicar el modificador
        playerHealth.ModifyMaxHealth(mult);

        Debug.Log($"MaxHealthModifier: Aplicado x{mult:F2} ({percent}%)");

        // Retornar el efecto para poder revertirlo después
        return new MaxHealthEffect(mult);
    }

    private class MaxHealthEffect : IRevertibleEffect
    {
        private readonly float _mult;

        public string DebugName => $"MaxHealth x{_mult:F2}";

        public MaxHealthEffect(float mult) => _mult = mult;

        public void Revert(GameManager gameManager)
        {
            if (gameManager == null) return;

            PlayerHealth playerHealth = gameManager.GetComponent<PlayerHealth>();
            if (playerHealth == null) return;

            // Revertir aplicando el inverso del multiplicador
            playerHealth.ModifyMaxHealth(1f / _mult);

            Debug.Log($"MaxHealthModifier: Revertido x{_mult:F2}");
        }
    }
}