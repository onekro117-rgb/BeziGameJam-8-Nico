using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Game Manager")]
    [SerializeField] private GameObject gameManager;

    [Header("Modifier Choice Panel")]
    [SerializeField] private GameObject modifierPanel;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] optionTitleTexts;
    [SerializeField] private TMP_Text[] optionDescTexts;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private Action<int> _onOptionSelected;

    private void Awake()
    {
        Instance = this;

        if (modifierPanel != null)
            modifierPanel.SetActive(false);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            SetupGameOverButtons();
        }
    }

    private void SetupGameOverButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void ShowModifierChoices(ModifierOffer[] offers, System.Action<int> onOptionSelected)
    {
        _onOptionSelected = onOptionSelected;
        modifierPanel.SetActive(true);

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;

            optionTitleTexts[i].text = offers[i].Title;
            optionDescTexts[i].text = offers[i].Description;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                modifierPanel.SetActive(false);
                _onOptionSelected?.Invoke(index);
            });
        }
    }

    public void ShowModifierPanel(System.Action onAnyButtonClicked)
    {
        modifierPanel.SetActive(true);
        Time.timeScale = 0f;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() =>
            {
                modifierPanel.SetActive(false);
                Time.timeScale = 1f;
                onAnyButtonClicked?.Invoke();
            });
        }
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("✓ Game Over Panel mostrado");
        }
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log("✓ Game Over Panel ocultado");
        }
    }

    private void OnRetryClicked()
    {
        Debug.Log("=== RETRY BUTTON CLICKED ===");

        // 1. Ocultar panel y restaurar tiempo
        HideGameOverPanel();

        // 2. Buscar el GameManager y resetear todo el juego
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ResetGame();
        }
        else
        {
            // Fallback si no hay GameManager
            Debug.LogWarning("No se encontró GameManager. Haciendo reseteo manual...");
            ManualReset();
        }
    }

    /// <summary>
    /// Reseteo manual si no existe GameManager (fallback de seguridad)
    /// </summary>
    private void ManualReset()
    {
        Debug.Log("=== INICIANDO RESETEO MANUAL ===");

        // Resetear WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RestartWaves();
            Debug.Log("✓ WaveManager reseteado");
        }

        // Buscar player y resetear sus componentes
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Resetear vidas
            HealthSystem healthSystem = player.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.ResetLives();
                Debug.Log("✓ Vidas reseteadas");
            }

            // Resetear salud
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetForNewGame();
                Debug.Log("✓ Salud reseteada");
            }

            // Resetear posición
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.Respawn();
                playerMovement.ResetMultipliers();
                Debug.Log("✓ Posición y multiplicadores reseteados");
            }

            // Resetear combate
            PlayerCombat playerCombat = player.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.ResetCombat();
                Debug.Log("✓ Combate reseteado");
            }

            // Resetear magia
            MagicSystem magicSystem = player.GetComponent<MagicSystem>();
            if (magicSystem != null)
            {
                magicSystem.ResetMagic();
                Debug.Log("✓ Magia reseteada");
            }

            // Resetear modificadores
            ModifierManager modifierManager = FindFirstObjectByType<ModifierManager>();
            if (modifierManager != null)
            {
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    modifierManager.RevertAll(gm);
                    Debug.Log("✓ Modificadores revertidos");
                }
            }
        }
        else
        {
            Debug.LogError("⚠ No se encontró el Player con tag 'Player'!");
        }

        Debug.Log("=== RESETEO MANUAL COMPLETADO ===");
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        Debug.Log("Main Menu button clicked - Scene not implemented yet");
        // TODO: Implementar carga de escena del menú principal
        // SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
