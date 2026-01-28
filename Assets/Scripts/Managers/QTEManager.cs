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
    private int currentButtonIndex;
    private float currentProgress;
    private bool isQTEActive;
    private int totalScore;
    private float qteTimer;
    private QTEVisualizer visualizer;

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

        qteTimer += Time.deltaTime;

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
        
        if (distance <= perfectZoneSize)
        {
            return 3;
        }
        else if (distance <= goodZoneSize)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    private void EndQTE()
    {
        isQTEActive = false;
        
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
