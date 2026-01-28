using UnityEngine;

/// <summary>
/// Estado de ataque - Ejecuta el ataque específico del enemigo.
/// Espera a que el ataque termine antes de pasar a Recovery.
/// </summary>
public class AttackState : IEnemyState
{
    private EnemyContext context;
    private EnemyBrain brain;
    private bool shotExecuted; // Para ShootAttack

    public AttackState(EnemyContext context, EnemyBrain brain)
    {
        this.context = context;
        this.brain = brain;
    }

    public void Enter()
    {
        Debug.Log($"<color=red>[{context.Enemy.name}]</color> 💥 <b>ATACANDO</b>");

        if (context.Attack == null)
        {
            Debug.LogError($"AttackState: No hay componente IAttack en {context.Enemy.name}!");
            brain.ChangeState(new PatrolState(context, brain));
            return;
        }

        // Iniciar el ataque
        context.Attack.StartAttack();

        // Iniciar cooldown
        if (context.AttackCooldown != null)
        {
            context.AttackCooldown.StartCooldown();
        }

        shotExecuted = false;
    }

    public void Tick(float deltaTime)
    {
        if (context.Attack == null)
        {
            brain.ChangeState(new RecoverState(context, brain));
            return;
        }

        // Caso especial para ShootAttack: Ejecutar disparo en el momento correcto
        ShootAttack shootAttack = context.Attack as ShootAttack;
        if (shootAttack != null && !shotExecuted)
        {
            // El ShootAttack no dispara automáticamente, hay que llamar ExecuteShot()
            shootAttack.ExecuteShot();
            shotExecuted = true;
        }

        // Esperar a que termine el ataque
        if (!context.Attack.IsAttacking)
        {
            brain.ChangeState(new RecoverState(context, brain));
        }
    }

    public void Exit()
    {
        // El ataque se limpia solo
    }
}
