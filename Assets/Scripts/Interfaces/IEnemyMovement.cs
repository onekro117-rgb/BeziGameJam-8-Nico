using UnityEngine;

/// Interfaz para el sistema de movimiento de enemigos.
/// Permite que el Brain controle el movimiento sin depender de implementación específica.
public interface IEnemyMovement
{
    /// Mueve el enemigo hacia un objetivo específico
    void MoveTowards(Vector2 target, float speed);

    /// Detiene completamente el movimiento del enemigo
    void Stop();

    /// Hace que el enemigo patrulle entre dos puntos
    void PatrolBetweenPoints(Vector2 pointA, Vector2 pointB);

    /// Voltea el sprite del enemigo hacia un objetivo (flip horizontal)
    void FlipTowards(Vector2 target);
}
