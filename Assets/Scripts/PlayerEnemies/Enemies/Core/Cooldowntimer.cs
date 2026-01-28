using UnityEngine;

/// Sistema de cooldown reutilizable para ataques y habilidades.
/// Maneja el temporizador y proporciona información sobre el progreso.
[System.Serializable]
public class CooldownTimer
{
    [SerializeField] private float cooldownDuration;
    private float cooldownTimer;

    /// Constructor del cooldown
    public CooldownTimer(float duration)
    {
        cooldownDuration = duration;
        cooldownTimer = 0f;
    }

    /// Indica si el cooldown está completo y listo para usar
    public bool IsReady => cooldownTimer <= 0f;

    /// Progreso del cooldown (0 = iniciado, 1 = listo)
    public float Progress => Mathf.Clamp01(1f - (cooldownTimer / cooldownDuration));

    /// Tiempo restante del cooldown en segundos
    public float TimeRemaining => Mathf.Max(0f, cooldownTimer);

    /// Inicia el cooldown
    public void StartCooldown()
    {
        cooldownTimer = cooldownDuration;
    }

    /// Actualiza el temporizador. Llamar en Update o Tick
    public void Tick(float deltaTime)
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= deltaTime;
    }

    /// Resetea el cooldown a listo inmediatamente
    public void Reset()
    {
        cooldownTimer = 0f;
    }

    /// Cambia la duración del cooldown
    public void SetDuration(float duration)
    {
        cooldownDuration = duration;
    }

    /// Obtiene la duración configurada del cooldown
    public float GetDuration()
    {
        return cooldownDuration;
    }
}
