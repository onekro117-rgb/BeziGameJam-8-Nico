/// Interfaz base para todos los estados del FSM de enemigos.
/// Cada estado (Patrol, Prepare, Attack, Recover) implementa esto.
public interface IEnemyState
{
    /// Se llama cuando el estado se activa por primera vez
    void Enter();

    /// Se llama cada frame mientras el estado está activo
    /// <param name="deltaTime">Time.deltaTime</param>
    void Tick(float deltaTime);

    /// Se llama cuando el estado termina y cambia a otro
    void Exit();
}
