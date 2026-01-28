using UnityEngine;

public class KillZone : MonoBehaviour
{
    private bool _alreadyTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_alreadyTriggered) return;

        Transform root = other.transform.root;
        if (!root.CompareTag("Player")) return;

        _alreadyTriggered = true;

        // Activar Game Over directo
        Debug.Log("=== PLAYER CAYÓ EN KILLZONE - GAME OVER ===");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOverPanel();
        }
        else
        {
            Debug.LogError("UIManager.Instance es null! No se puede mostrar Game Over Panel");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.root.CompareTag("Player"))
            _alreadyTriggered = false;
    }
}

