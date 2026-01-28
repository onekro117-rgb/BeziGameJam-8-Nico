using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Magic Spell", menuName = "Magic System/Magic Spell Data")]
public class MagicSpellData : ScriptableObject
{
    [Header("Spell Info")]
    public string spellName = "Magia 1";
    
    [Header("QTE Configuration")]
    [Tooltip("Lista de botones y sus posiciones en la línea del QTE")]
    public List<QTEButtonData> buttons = new List<QTEButtonData>();
    
    [Header("Damage Area")]
    [Tooltip("Define el área de daño y daño base de esta magia")]
    public MagicDamageArea damageArea;
    
    [Header("Spell Effects")]
    [Tooltip("Multiplicador del cooldown de la magia")]
    public float cooldownMultiplier = 1f;
}

[System.Serializable]
public class QTEButtonData
{
    [Tooltip("Tecla a presionar (Q, W, E, R)")]
    public KeyCode key = KeyCode.Q;
    
    [Tooltip("Posición en la línea del QTE (0.0 = inicio, 1.0 = final)")]
    [Range(0f, 1f)]
    public float position = 0.5f;
}
