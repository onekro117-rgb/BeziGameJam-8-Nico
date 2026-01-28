using UnityEngine;

public enum DamageAreaType
{
    Circle,
    Cone,
    Rectangle
}

[CreateAssetMenu(fileName = "New Magic Damage Area", menuName = "Magic System/Damage Area")]
public class MagicDamageArea : ScriptableObject
{
    [Header("Area Type")]
    [Tooltip("Tipo de área de daño: Circle (Fuego), Cone (Hielo), Rectangle (Rayo)")]
    public DamageAreaType areaType = DamageAreaType.Circle;
    
    [Header("Damage Settings")]
    [Tooltip("Daño base cuando el QTE es perfecto")]
    public int baseDamage = 50;
    
    [Header("Circle Area (Fuego)")]
    [Tooltip("Radio del círculo alrededor del player")]
    public float circleRadius = 3f;
    
    [Header("Cone Area (Hielo)")]
    [Tooltip("Ángulo del cono en grados (90 = cuarto de círculo)")]
    [Range(0f, 180f)]
    public float coneAngle = 60f;
    
    [Tooltip("Distancia máxima del cono desde el player")]
    public float coneDistance = 5f;
    
    [Header("Rectangle Area (Rayo)")]
    [Tooltip("Ancho y alto del rectángulo (X = ancho, Y = alto)")]
    public Vector2 rectangleSize = new Vector2(2f, 8f);
    
    [Tooltip("Desplazamiento desde el centro del player (Y positivo = hacia arriba)")]
    public Vector2 rectangleOffset = new Vector2(0f, 4f);
}
