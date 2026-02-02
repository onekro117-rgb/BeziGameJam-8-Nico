using UnityEngine;

/// <summary>
/// Health component for enemies
/// Inherits from HealthComponent
/// </summary>
public class EnemyHealth : HealthComponent
{
    protected override void Die()
    {
        if (IsDead)
            return;

        base.Die(); // Invoca OnDeath y marca IsDead = true

        Debug.Log($"<color=yellow>[EnemyHealth]</color> {gameObject.name} died!");

        // Destroy enemy after short delay (para que se vea la animación de muerte)
        Destroy(gameObject, 0.1f);
    }
}
