using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives;
    public int MaxLives => maxLives;
    public int CurrentLives => currentLives;

    [Header("Events")]
    public UnityEvent<int, int> OnLivesChanged; // CurrentLifes, maxLives
    public UnityEvent OnLifeLost;
    public UnityEvent OnGameOver;

    private void Awake()
    {
        currentLives = maxLives;
        OnLivesChanged?.Invoke(CurrentLives, maxLives);
    }

    public void ResetLives()
    {
        currentLives = maxLives;
        OnLivesChanged?.Invoke(CurrentLives, maxLives);
    }

    public void LoseLife()
    {
        if (CurrentLives <= 0) return;

        currentLives--;
        // OnLifeLost?.Invoke();
        OnLivesChanged?.Invoke(CurrentLives, maxLives);

        if (CurrentLives <= 0)
            OnGameOver?.Invoke();
    }

    public void AddLifes(int amount)
    {
        currentLives = Mathf.Clamp(CurrentLives + amount, 0, maxLives);
        OnLivesChanged?.Invoke(CurrentLives, maxLives);
    }
}

