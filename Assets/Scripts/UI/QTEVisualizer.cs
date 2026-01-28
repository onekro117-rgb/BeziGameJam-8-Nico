using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTEVisualizer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject qtePanel;
    [SerializeField] private RectTransform indicatorBar;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Colors")]
    [SerializeField] private Color perfectColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private Color goodColor = new Color(1f, 1f, 0f, 0.5f);
    [SerializeField] private Color missColor = new Color(1f, 0f, 0f, 0.5f);

    private List<GameObject> buttonVisuals = new List<GameObject>();
    private QTEManager qteManager;
    private bool hasSetupButtons = false;

    private void Awake()
    {
        qteManager = FindFirstObjectByType<QTEManager>();
        
        if (qtePanel == null)
        {
            GameObject panel = transform.Find("QTE Panel")?.gameObject;
            if (panel != null) qtePanel = panel;
        }
        
        if (indicatorBar == null)
        {
            Transform indicator = transform.Find("QTE Panel/Indicator");
            if (indicator != null) indicatorBar = indicator.GetComponent<RectTransform>();
        }
        
        if (buttonsContainer == null)
        {
            Transform container = transform.Find("QTE Panel/Buttons Container");
            if (container != null) buttonsContainer = container;
        }
        
        if (buttonPrefab == null)
        {
            Transform prefab = transform.Find("Button Prefab");
            if (prefab != null) buttonPrefab = prefab.gameObject;
        }
        
        if (qtePanel != null)
            qtePanel.SetActive(false);
    }

    private void Update()
    {
        if (qteManager == null || !qteManager.IsQTEActive())
        {
            if (qtePanel != null && qtePanel.activeSelf)
            {
                HideQTE();
            }
            hasSetupButtons = false;
            return;
        }

        if (qtePanel != null && !qtePanel.activeSelf)
        {
            ShowQTE();
        }

        UpdateProgressBar();
    }

    private void ShowQTE()
    {
        if (qtePanel != null)
            qtePanel.SetActive(true);
    }

    private void HideQTE()
    {
        if (qtePanel != null)
            qtePanel.SetActive(false);
        ClearButtonVisuals();
    }

    private void UpdateProgressBar()
    {
        float progress = qteManager.GetCurrentProgress();
        
        if (indicatorBar != null)
        {
            Vector2 anchoredPos = indicatorBar.anchoredPosition;
            anchoredPos.x = Mathf.Lerp(-400f, 400f, progress);
            indicatorBar.anchoredPosition = anchoredPos;
        }
    }

    public void SetupButtons(List<QTEButton> buttons)
    {
        if (hasSetupButtons) return;
        
        ClearButtonVisuals();

        if (buttonsContainer == null || buttonPrefab == null) return;

        foreach (QTEButton button in buttons)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonsContainer);
            buttonObj.SetActive(true);
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            
            float xPos = Mathf.Lerp(-400f, 400f, button.targetPosition);
            rectTransform.anchoredPosition = new Vector2(xPos, 0f);
            
            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = button.keyCode.ToString();
            }
            
            buttonVisuals.Add(buttonObj);
        }
        
        hasSetupButtons = true;
    }

    private void ClearButtonVisuals()
    {
        foreach (GameObject buttonObj in buttonVisuals)
        {
            if (buttonObj != null)
                Destroy(buttonObj);
        }
        buttonVisuals.Clear();
        hasSetupButtons = false;
    }
}
