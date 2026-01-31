using UnityEngine;

/// <summary>
/// Individual collider on the ring that deals damage to enemies.
/// Multiple of these are arranged in a circle and rotated.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RingDamageCollider : MonoBehaviour
{
    private RingMagicBehavior ringBehavior;

    public void Initialize(RingMagicBehavior behavior)
    {
        ringBehavior = behavior;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ringBehavior != null)
        {
            ringBehavior.OnColliderHitEnemy(collision);
        }
    }
}
