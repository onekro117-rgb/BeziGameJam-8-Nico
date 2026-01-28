using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private bool _alreadyTriggered;

    public void ResetTrigger()
    {
        _alreadyTriggered = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_alreadyTriggered) return;

        Transform root = other.transform.root;
        if (!root.CompareTag("Player")) return;

        _alreadyTriggered = true;

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnPlayerReachedGoal();
        }
    }
}
