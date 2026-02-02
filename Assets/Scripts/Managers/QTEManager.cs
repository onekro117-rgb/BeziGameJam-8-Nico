using System;
using System.Collections.Generic;
using UnityEngine;

public class QTEManager : MonoBehaviour
{
    public static QTEManager Instance { get; private set; }

    [Header("QTE Settings")]
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float barDuration = 2f;
    
    [Header("Zone Thresholds")]
    [SerializeField] private float perfectZoneSize = 0.05f;
    [SerializeField] private float goodZoneSize = 0.10f;

    public event Action<int> OnQTEComplete;
    
    private List<QTEButton> qteButtons;
    private QTEVisualizer visualizer;

    private int currentButtonIndex;
    private int totalScore;
    private float currentProgress;
    private float qteTimer;
    private float perfectZoneMultiplier = 1f;  // Modified by modifiers
    private float goodZoneMultiplier = 1f;     // Modified by modifiers
    private bool isQTEActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        visualizer = FindFirstObjectByType<QTEVisualizer>();
    }

    private void Update()
    {
        if (!isQTEActive) return;

        qteTimer += Time.unscaledDeltaTime;

        if (qteTimer < startDelay)
        {
            currentProgress = 0f;
            return;
        }

        float effectiveTime = qteTimer - startDelay;
        currentProgress = effectiveTime / barDuration;

        if (currentProgress >= 1f)
        {
            EndQTE();
            return;
        }

        CheckInput();
    }

    public void StartQTE(List<QTEButton> buttons)
    {
        qteButtons = buttons;
        currentButtonIndex = 0;
        totalScore = 0;
        qteTimer = 0f;
        currentProgress = 0f;
        isQTEActive = true;

        Time.timeScale = 0f;

        PauseGame();

        if (visualizer != null)
        {
            visualizer.SetupButtons(buttons);
        }

        Debug.Log($"QTE Started with {buttons.Count} buttons!");
    }

    private void CheckInput()
    {
        if (currentButtonIndex >= qteButtons.Count) return;

        QTEButton currentButton = qteButtons[currentButtonIndex];
        
        bool q_pressed = InputManager.QTEButton1WasPressed;
        bool w_pressed = InputManager.QTEButton2WasPressed;
        bool e_pressed = InputManager.QTEButton3WasPressed;
        bool r_pressed = InputManager.QTEButton4WasPressed;
        
        bool anyButtonPressed = q_pressed || w_pressed || e_pressed || r_pressed;
        
        if (!anyButtonPressed) return;
        
        bool correctButtonPressed = 
            (q_pressed && currentButton.keyCode == KeyCode.Q) ||
            (w_pressed && currentButton.keyCode == KeyCode.W) ||
            (e_pressed && currentButton.keyCode == KeyCode.E) ||
            (r_pressed && currentButton.keyCode == KeyCode.R);
        
        if (correctButtonPressed)
        {
            int score = EvaluateHit(currentButton.targetPosition);
            totalScore += score;
            
            string result = score == 3 ? "PERFECTO" : score == 1 ? "PARCIAL" : "MISS";
            Debug.Log($"Button {currentButtonIndex + 1}: {result} (+{score} puntos)");
            
            currentButtonIndex++;
        }
        else
        {
            string wrongKey = q_pressed ? "Q" : w_pressed ? "W" : e_pressed ? "E" : "R";
            string expectedKey = currentButton.keyCode.ToString();
            
            Debug.Log($"Button {currentButtonIndex + 1}: FALLO - Presionaste {wrongKey} (esperaba {expectedKey}) (+0 puntos)");
            
            currentButtonIndex++;
        }
    }

    private int EvaluateHit(float targetPosition)
    {
        float distance = Mathf.Abs(currentProgress - targetPosition);

        // Apply multipliers to zone sizes
        float effectivePerfectZone = perfectZoneSize * perfectZoneMultiplier;
        float effectiveGoodZone = goodZoneSize * goodZoneMultiplier;

        if (distance <= effectivePerfectZone)
        {
            return 3; // Perfect
        }
        else if (distance <= effectiveGoodZone)
        {
            return 1; // Good
        }
        else
        {
            return 0; // Miss
        }
    }

    private void EndQTE()
    {
        isQTEActive = false;

        Time.timeScale = 1f;

        ResumeGame();
        
        Debug.Log($"=== MAGIA FINALIZADA === Puntos obtenidos: {totalScore}/{qteButtons.Count * 3}");
        
        OnQTEComplete?.Invoke(totalScore);
    }

    private void PauseGame()
    {
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }
    }

    private void ResumeGame()
    {
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        PlayerCombat playerCombat = FindFirstObjectByType<PlayerCombat>();
        if (playerCombat != null)
        {
            playerCombat.enabled = true;
        }
    }

    public bool IsQTEActive()
    {
        return isQTEActive;
    }

    public float GetCurrentProgress()
    {
        return currentProgress;
    }

    public int GetCurrentButtonIndex()
    {
        return currentButtonIndex;
    }

    /// <summary>
    /// Get current perfect zone size multiplier (for modifiers)
    /// </summary>
    public float GetPerfectZoneMultiplier()
    {
        return perfectZoneMultiplier;
    }

    /// <summary>
    /// Set perfect zone size multiplier (for modifiers)
    /// </summary>
    public void SetPerfectZoneMultiplier(float multiplier)
    {
        perfectZoneMultiplier = Mathf.Max(0.1f, multiplier); // Min 0.1x (10% of normal)
        Debug.Log($"<color=magenta>[QTEManager]</color> Perfect zone multiplier: {perfectZoneMultiplier:F2}x");
    }

    /// <summary>
    /// Get current good zone size multiplier (for modifiers - optional)
    /// </summary>
    public float GetGoodZoneMultiplier()
    {
        return goodZoneMultiplier;
    }

    /// <summary>
    /// Set good zone size multiplier (for modifiers - optional)
    /// </summary>
    public void SetGoodZoneMultiplier(float multiplier)
    {
        goodZoneMultiplier = Mathf.Max(0.1f, multiplier);
        Debug.Log($"<color=magenta>[QTEManager]</color> Good zone multiplier: {goodZoneMultiplier:F2}x");
    }
}

[Serializable]
public class QTEButton
{
    public KeyCode keyCode;
    public float targetPosition;

    public QTEButton(KeyCode key, float position)
    {
        keyCode = key;
        targetPosition = position;
    }
}
