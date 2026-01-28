using System;

public interface IDamageable
{
    void TakeDamage(int damage);
    void Heal(int amount);
    
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }
    bool IsInvulnerable { get; }
    
    event Action OnDeath;
    event Action<int, int> OnHealthChanged;
}
