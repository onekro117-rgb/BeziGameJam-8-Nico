using System.Collections.Generic;
using UnityEngine;

public class MagicSystem : MonoBehaviour
{
    [Header("Magic Settings")]
    [SerializeField] private float magicCooldown = 3f;
    
    [Header("Available Spells")]
    [SerializeField] private List<MagicSpellData> availableSpells = new List<MagicSpellData>();
    [SerializeField] private int currentSpellIndex = 0;
    
    private float cooldownTimer;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        if (QTEManager.Instance != null)
        {
            QTEManager.Instance.OnQTEComplete += OnQTECompleted;
        }
        
        if (availableSpells.Count == 0)
        {
            Debug.LogWarning("MagicSystem: No hay magias configuradas! Añade MagicSpellData assets en el Inspector.");
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (cooldownTimer <= 0 && !QTEManager.Instance.IsQTEActive())
        {
            if (InputManager.Magic1WasPressed && availableSpells.Count > 0)
            {
                SetSpell(0);
                CastMagic();
            }
            else if (InputManager.Magic2WasPressed && availableSpells.Count > 1)
            {
                SetSpell(1);
                CastMagic();
            }
            else if (InputManager.Magic3WasPressed && availableSpells.Count > 2)
            {
                SetSpell(2);
                CastMagic();
            }
        }
    }

    private void CastMagic()
    {
        if (availableSpells.Count == 0)
        {
            Debug.LogWarning("No hay magias disponibles para lanzar!");
            return;
        }
        
        MagicSpellData spell = availableSpells[currentSpellIndex];
        
        if (spell.buttons.Count == 0)
        {
            Debug.LogWarning($"La magia '{spell.spellName}' no tiene botones configurados!");
            return;
        }
        
        Debug.Log($"Lanzando: {spell.spellName}");
        
        List<QTEButton> buttons = new List<QTEButton>();
        foreach (var buttonData in spell.buttons)
        {
            buttons.Add(new QTEButton(buttonData.key, buttonData.position));
        }
        
        QTEManager.Instance.StartQTE(buttons);
        
        cooldownTimer = magicCooldown * spell.cooldownMultiplier;
    }

    private void OnQTECompleted(int score)
    {
        if (availableSpells.Count == 0) return;
        
        MagicSpellData spell = availableSpells[currentSpellIndex];
        int maxScore = spell.buttons.Count * 3;
        
        float damageMultiplier = 0f;
        
        if (score >= maxScore)
        {
            damageMultiplier = 1f;
            Debug.Log($"=== {spell.spellName.ToUpper()} PERFECTA === ({score}/{maxScore} puntos)");
        }
        else if (score >= maxScore / 2)
        {
            damageMultiplier = 0.5f;
            Debug.Log($"=== {spell.spellName.ToUpper()} PARCIAL === ({score}/{maxScore} puntos, 50% efectividad)");
        }
        else
        {
            damageMultiplier = 0f;
            Debug.Log($"=== {spell.spellName.ToUpper()} FALLIDA === ({score}/{maxScore} puntos)");
        }
        
        ApplyMagicDamage(spell, damageMultiplier);
    }
    
    private void ApplyMagicDamage(MagicSpellData spell, float damageMultiplier)
    {
        if (spell.damageArea == null)
        {
            Debug.LogWarning($"La magia '{spell.spellName}' no tiene MagicDamageArea asignado!");
            return;
        }
        
        if (damageMultiplier <= 0f)
        {
            Debug.Log($"QTE fallido, no se aplica daño.");
            return;
        }
        
        int finalDamage = Mathf.RoundToInt(spell.damageArea.baseDamage * damageMultiplier);
        Vector2 playerPosition = transform.position;
        
        List<IDamageable> enemiesHit = new List<IDamageable>();
        
        switch (spell.damageArea.areaType)
        {
            case DamageAreaType.Circle:
                enemiesHit = GetEnemiesInCircle(playerPosition, spell.damageArea.circleRadius);
                break;
                
            case DamageAreaType.Cone:
                bool facingRight = playerMovement != null && playerMovement.IsFacingRight;
                enemiesHit = GetEnemiesInCone(playerPosition, spell.damageArea.coneDistance, 
                                               spell.damageArea.coneAngle, facingRight);
                break;
                
            case DamageAreaType.Rectangle:
                enemiesHit = GetEnemiesInRectangle(playerPosition, spell.damageArea.rectangleSize, 
                                                    spell.damageArea.rectangleOffset);
                break;
        }
        
        foreach (var enemy in enemiesHit)
        {
            enemy.TakeDamage(finalDamage);
        }
        
        Debug.Log($"[{spell.spellName}] Daño aplicado: {finalDamage} a {enemiesHit.Count} enemigo(s)");
    }
    
    private List<IDamageable> GetEnemiesInCircle(Vector2 center, float radius)
    {
        List<IDamageable> enemies = new List<IDamageable>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        
        foreach (var hit in hits)
        {
            IDamageable health = hit.GetComponent<IDamageable>();
            if (health != null && !enemies.Contains(health))
            {
                enemies.Add(health);
            }
        }
        
        return enemies;
    }
    
    private List<IDamageable> GetEnemiesInCone(Vector2 origin, float distance, float angle, bool facingRight)
    {
        List<IDamageable> enemies = new List<IDamageable>();
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, distance);
        
        Vector2 coneDirection = facingRight ? Vector2.right : Vector2.left;
        float halfAngle = angle * 0.5f;
        
        foreach (var hit in hits)
        {
            IDamageable health = hit.GetComponent<IDamageable>();
            if (health != null)
            {
                Vector2 directionToEnemy = ((Vector2)hit.transform.position - origin).normalized;
                float angleToEnemy = Vector2.Angle(coneDirection, directionToEnemy);
                
                if (angleToEnemy <= halfAngle && !enemies.Contains(health))
                {
                    enemies.Add(health);
                }
            }
        }
        
        return enemies;
    }
    
    private List<IDamageable> GetEnemiesInRectangle(Vector2 playerPosition, Vector2 size, Vector2 offset)
    {
        List<IDamageable> enemies = new List<IDamageable>();
        Vector2 boxCenter = playerPosition + offset;
        
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, size, 0f);
        
        foreach (var hit in hits)
        {
            IDamageable health = hit.GetComponent<IDamageable>();
            if (health != null && !enemies.Contains(health))
            {
                enemies.Add(health);
            }
        }
        
        return enemies;
    }

    private void OnDestroy()
    {
        if (QTEManager.Instance != null)
        {
            QTEManager.Instance.OnQTEComplete -= OnQTECompleted;
        }
    }

    public float GetCooldownProgress()
    {
        return Mathf.Clamp01(1f - (cooldownTimer / magicCooldown));
    }
    
    public void SetSpell(int index)
    {
        if (index >= 0 && index < availableSpells.Count)
        {
            currentSpellIndex = index;
            Debug.Log($"Magia seleccionada: {availableSpells[currentSpellIndex].spellName}");
        }
        else
        {
            Debug.LogWarning($"Índice de magia inválido: {index}");
        }
    }
    
    public void NextSpell()
    {
        if (availableSpells.Count == 0) return;
        
        currentSpellIndex = (currentSpellIndex + 1) % availableSpells.Count;
        Debug.Log($"Magia seleccionada: {availableSpells[currentSpellIndex].spellName}");
    }
    
    public void PreviousSpell()
    {
        if (availableSpells.Count == 0) return;
        
        currentSpellIndex--;
        if (currentSpellIndex < 0)
            currentSpellIndex = availableSpells.Count - 1;
        
        Debug.Log($"Magia seleccionada: {availableSpells[currentSpellIndex].spellName}");
    }
    
    public string GetCurrentSpellName()
    {
        if (availableSpells.Count == 0 || currentSpellIndex >= availableSpells.Count)
            return "Sin magia";
        
        return availableSpells[currentSpellIndex].spellName;
    }

    /// Resetea el sistema de magia a su estado inicial
    public void ResetMagic()
    {
        cooldownTimer = 0f;
        currentSpellIndex = 0;

        Debug.Log("MagicSystem: Sistema de magia reseteado");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (availableSpells.Count == 0 || currentSpellIndex >= availableSpells.Count)
            return;
        
        MagicSpellData spell = availableSpells[currentSpellIndex];
        if (spell == null || spell.damageArea == null)
            return;
        
        Vector2 playerPosition = transform.position;
        
        switch (spell.damageArea.areaType)
        {
            case DamageAreaType.Circle:
                DrawCircleGizmo(playerPosition, spell.damageArea.circleRadius);
                break;
                
            case DamageAreaType.Cone:
                bool facingRight = playerMovement != null && playerMovement.IsFacingRight;
                DrawConeGizmo(playerPosition, spell.damageArea.coneDistance, 
                              spell.damageArea.coneAngle, facingRight);
                break;
                
            case DamageAreaType.Rectangle:
                DrawRectangleGizmo(playerPosition, spell.damageArea.rectangleSize, 
                                   spell.damageArea.rectangleOffset);
                break;
        }
    }
    
    private void DrawCircleGizmo(Vector2 center, float radius)
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 0.8f);
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, radius);
        
        Gizmos.DrawWireSphere(center, radius);
    }

    private void DrawConeGizmo(Vector2 origin, float distance, float angle, bool facingRight)
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);

        Vector2 coneDirection = facingRight ? Vector2.right : Vector2.left;
        float halfAngle = angle * 0.5f;

        Vector3 direction3D = new Vector3(coneDirection.x, coneDirection.y, 0f);
        Vector3 origin3D = new Vector3(origin.x, origin.y, 0f);

        Vector3 topEdge = Quaternion.Euler(0, 0, halfAngle) * direction3D * distance;
        Vector3 bottomEdge = Quaternion.Euler(0, 0, -halfAngle) * direction3D * distance;

        Gizmos.DrawLine(origin3D, origin3D + topEdge);
        Gizmos.DrawLine(origin3D, origin3D + bottomEdge);

        int segments = 20;
        Vector3 previousPoint = origin3D + bottomEdge;
        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = -halfAngle + (angle * i / segments);
            Vector3 currentPoint = origin3D + Quaternion.Euler(0, 0, currentAngle) * direction3D * distance;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }

    private void DrawRectangleGizmo(Vector2 playerPosition, Vector2 size, Vector2 offset)
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 boxCenter = new Vector3(playerPosition.x + offset.x, playerPosition.y + offset.y, 0f);
        Vector3 boxSize = new Vector3(size.x, size.y, 0.1f);
        
        Gizmos.DrawWireCube(boxCenter, boxSize);
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawCube(boxCenter, boxSize);
    }
#endif
}
