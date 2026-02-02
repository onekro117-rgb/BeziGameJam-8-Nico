using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] protected int maxHealth = 5;
    [SerializeField] protected float invulnerabilityDuration = 1f;

    public int CurrentHealth { get; protected set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; protected set; }
    public bool IsInvulnerable { get; protected set; }

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;

    protected float invulnerabilityTimer;
    protected int baseMaxHealth;
    protected KnockbackReceiver knockbackReceiver;

    protected virtual void Awake()
    {
        baseMaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        IsDead = false;
        knockbackReceiver = GetComponent<KnockbackReceiver>();
    }

    protected virtual void Update()
    {
        if (IsInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                IsInvulnerable = false;
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsInvulnerable || IsDead || CurrentHealth <= 0)
            return;

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        IsInvulnerable = true;
        invulnerabilityTimer = invulnerabilityDuration;

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Take damage with knockback
    /// </summary>
    public virtual void TakeDamage(int damage, Vector2 attackerPosition)
    {
        Debug.Log($"<color=yellow>[DEBUG]</color> HealthComponent.TakeDamage con knockback llamado");

        TakeDamage(damage);

        if (knockbackReceiver != null)
        {
            Debug.Log($"<color=green>[DEBUG]</color> KnockbackReceiver encontrado, aplicando knockback");
            Vector2 direction = ((Vector2)transform.position - attackerPosition).normalized;
            Debug.Log($"<color=green>[DEBUG]</color> Dirección calculada: {direction}");
            knockbackReceiver.ApplyKnockback(direction);
        }
        else
        {
            Debug.LogError($"<color=red>[DEBUG]</color> NO se encontró KnockbackReceiver!");
        }
    }

    public virtual void Heal(int amount)
    {
        if (IsDead)
            return;

        int previousHealth = CurrentHealth;
        CurrentHealth += amount;
        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        
        if (CurrentHealth != previousHealth)
        {
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Debug.Log($"{gameObject.name} healed {amount}. Health: {CurrentHealth}/{maxHealth}");
        }
    }

    public virtual void ModifyMaxHealth(float multiplier)
    {
        int oldMaxHealth = maxHealth;
        maxHealth = Mathf.RoundToInt(baseMaxHealth * multiplier);
        
        float healthPercentage = (float)CurrentHealth / oldMaxHealth;
        CurrentHealth = Mathf.RoundToInt(maxHealth * healthPercentage);
        CurrentHealth = Mathf.Clamp(CurrentHealth, 1, maxHealth);
        
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Debug.Log($"{gameObject.name} max health modified: {oldMaxHealth} -> {maxHealth}");
    }

    public virtual void ResetHealth()
    {
        CurrentHealth = maxHealth;
        IsDead = false;
        IsInvulnerable = false;
        invulnerabilityTimer = 0f;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    protected virtual void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        Debug.Log($"{gameObject.name} died!");
        OnDeath?.Invoke();
    }
}
