using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint Instance { get; private set; }

    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSpawnPosition()
    {
        return spawnPoint != null ? spawnPoint.position : transform.position;
    }
}
