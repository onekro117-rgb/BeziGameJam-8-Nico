using UnityEngine;

/// <summary>
/// Estado de recuperación - VENTANA DE CASTIGO.
/// El enemigo se queda quieto tras fallar su ataque, vulnerable al jugador.
/// </summary>
public class RecoverState : IEnemyState
{
    private EnemyContext context;
    private EnemyBrain brain;
    private float recoverTimer;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public RecoverState(EnemyContext context, EnemyBrain brain)
    {
        this.context = context;
        this.brain = brain;
    }

    public void Enter()
    {
        Debug.Log($"<color=gray>[{context.Enemy.name}]</color> 😵 <b>RECUPERÁNDOSE</b> (Ventana de castigo)");

        recoverTimer = 0f;

        // Detener movimiento
        context.Movement.Stop();

        // Mostrar feedback visual de recuperación
        ShowRecovering();
    }

    public void Tick(float deltaTime)
    {
        if (context.Config == null) return;

        recoverTimer += deltaTime;

        // Quedarse quieto durante recuperación
        context.Movement.Stop();

        // Transición: Volver a patrulla tras recuperarse
        if (recoverTimer >= context.Config.recoverTime)
        {
            brain.ChangeState(new PatrolState(context, brain));
        }
    }

    public void Exit()
    {
        HideRecovering();
    }

    /// <summary>
    /// Muestra feedback visual de recuperación
    /// </summary>
    private void ShowRecovering()
    {
        spriteRenderer = context.Enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;

            // Color de recuperación (gris por defecto)
            Color recoverColor = context.Config != null ?
                context.Config.recoverColor : Color.gray;

            spriteRenderer.color = recoverColor;
        }
    }

    /// <summary>
    /// Oculta el feedback visual
    /// </summary>
    private void HideRecovering()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
