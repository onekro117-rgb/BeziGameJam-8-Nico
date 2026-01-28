using UnityEngine;

/// <summary>
/// Estado de preparación de ataque - TELEGRAFÍA.
/// El enemigo se queda quieto y muestra un aviso visual antes de atacar.
/// </summary>
public class PrepareAttackState : IEnemyState
{
    private EnemyContext context;
    private EnemyBrain brain;
    private float prepareTimer;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public PrepareAttackState(EnemyContext context, EnemyBrain brain)
    {
        this.context = context;
        this.brain = brain;
    }

    public void Enter()
    {
        Debug.Log($"<color=yellow>[{context.Enemy.name}]</color> ⚠️ <b>PREPARANDO ATAQUE</b> (TELEGRAFÍA)");

        prepareTimer = 0f;

        // Detener movimiento
        context.Movement.Stop();

        // Guardar última posición conocida del jugador (snapshot)
        if (context.Player != null)
        {
            context.LastKnownPlayerPosition = context.Player.position;
        }

        // Mostrar telegrafía visual
        ShowTelegraph();
    }

    public void Tick(float deltaTime)
    {
        if (context.Config == null) return;

        prepareTimer += deltaTime;

        // Actualizar telegrafía visual
        UpdateTelegraph(prepareTimer / context.Config.prepareTime);

        // Transición: Tras tiempo de preparación → Atacar
        if (prepareTimer >= context.Config.prepareTime)
        {
            brain.ChangeState(new AttackState(context, brain));
        }
    }

    public void Exit()
    {
        HideTelegraph();
    }

    /// <summary>
    /// Muestra el feedback visual de telegrafía
    /// </summary>
    private void ShowTelegraph()
    {
        spriteRenderer = context.Enemy.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;

            // Color de telegrafía (amarillo por defecto)
            Color telegraphColor = context.Config != null ?
                context.Config.telegraphColor : Color.yellow;

            spriteRenderer.color = telegraphColor;
        }
    }

    /// <summary>
    /// Actualiza la telegrafía visual (opcional: parpadeo)
    /// </summary>
    private void UpdateTelegraph(float progress)
    {
        if (spriteRenderer == null || context.Config == null) return;

        // Parpadeo más rápido mientras se acerca el ataque
        float blinkSpeed = 5f + (progress * 10f);
        float blink = Mathf.PingPong(Time.time * blinkSpeed, 1f);

        Color telegraphColor = context.Config.telegraphColor;
        spriteRenderer.color = Color.Lerp(originalColor, telegraphColor, blink);
    }

    /// <summary>
    /// Oculta la telegrafía
    /// </summary>
    private void HideTelegraph()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
