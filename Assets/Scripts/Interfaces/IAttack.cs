/// Interfaz para todos los tipos de ataque de enemigos.
/// ChargeAttack, ShootAttack y DiveAttack implementan esto.
public interface IAttack
{
    /// Verifica si el ataque puede iniciarse
    /// (ej: no está ya atacando, tiene objetivo válido, etc.)
    bool CanStartAttack();

    /// Inicia la ejecución del ataque
    void StartAttack();

    /// Indica si el ataque está actualmente en ejecución
    bool IsAttacking { get; }

    /// Cancela el ataque en progreso
    void CancelAttack();
}
