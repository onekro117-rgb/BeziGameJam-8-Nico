using UnityEngine;

public class HealthEnemies : HealthComponent
{
    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, 0.1f);
    }
}
