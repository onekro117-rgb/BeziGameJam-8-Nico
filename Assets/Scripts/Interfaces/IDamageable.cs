using System;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// Take damage with knockback information
    /// </summary>
    /// <param name="damage">Amount of damage</param>
    /// <param name="attackerPosition">Position of the attacker (for knockback direction)</param>
    void TakeDamage(int damage, Vector2 attackerPosition);
    void TakeDamage(int damage);
    void Heal(int amount);
    
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }
    bool IsInvulnerable { get; }
    
    event Action OnDeath;
    event Action<int, int> OnHealthChanged;
}
