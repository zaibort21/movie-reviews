using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HungryShark.UI
{
    /// <summary>
    /// Manages the game's user interface including HUD, menus, and mobile UI elements
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private Slider hungerBar;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI fishCountText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image sharkSizeIndicator;
        
        [Header("Game Menus")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverMenu;
        [SerializeField] private GameObject settingsMenu;
        
        [Header("Game Over Screen")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI finalTimeText;
        [SerializeField] private TextMeshProUGUI finalFishCountText;
        
        [Header("Mobile Controls")]
        [SerializeField] private GameObject mobileControlsPanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Joystick movementJoystick; // Optional virtual joystick
        
        [Header("Settings")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Toggle gyroscopeToggle;
        [SerializeField] private Slider volumeSlider;
        
        private Core.GameManager gameManager;
        private Gameplay.FeedingSystem feedingSystem;
        private Core.TouchInputManager touchInput;
        
        private void Awake()
        {
            // Find required components
            gameManager = FindObjectOfType<Core.GameManager>();
            feedingSystem = FindObjectOfType<Gameplay.FeedingSystem>();
            touchInput = FindObjectOfType<Core.TouchInputManager>();
        }
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            SetupMobileUI();
        }
        
        private void InitializeUI()
        {
            // Initialize HUD
            if (hungerBar != null)
            {
                hungerBar.minValue = 0f;
                hungerBar.maxValue = 1f;
                hungerBar.value = 1f;
            }
            
            UpdateScore(0);
            UpdateFishCount(0);
            UpdateTime(0f);
            
            // Show main menu initially
            ShowMainMenu();
        }
        
        private void SubscribeToEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnScoreChanged += UpdateScore;
                gameManager.OnGameTimeChanged += UpdateTime;
                gameManager.OnGameStarted += OnGameStarted;
                gameManager.OnGamePaused += OnGamePaused;
                gameManager.OnGameResumed += OnGameResumed;
                gameManager.OnGameOver += OnGameOver;
            }
            
            if (feedingSystem != null)
            {
                feedingSystem.OnHungerChanged += UpdateHunger;
                feedingSystem.OnSizeChanged += UpdateSharkSize;
                feedingSystem.OnFishEaten += UpdateFishCount;
            }
        }
        
        private void SetupMobileUI()
        {
            bool isMobile = Application.isMobilePlatform;
            
            if (mobileControlsPanel != null)
            {
                mobileControlsPanel.SetActive(isMobile);
            }
            
            // Setup pause button
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(TogglePause);
            }
            
            // Setup settings sliders
            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }
            
            if (gyroscopeToggle != null)
            {
                gyroscopeToggle.onValueChanged.AddListener(OnGyroscopeToggled);
                gyroscopeToggle.gameObject.SetActive(touchInput != null && touchInput.IsGyroscopeAvailable());
            }
            
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }
        
        #region HUD Updates
        
        private void UpdateScore(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {newScore:N0}";
            }
        }
        
        private void UpdateFishCount(int fishCount)
        {
            if (fishCountText != null)
            {
                fishCountText.text = $"Fish: {fishCount}";
            }
        }
        
        private void UpdateTime(float gameTime)
        {
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                timeText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }
        
        private void UpdateHunger(float hungerValue)
        {
            if (hungerBar != null && feedingSystem != null)
            {
                hungerBar.value = feedingSystem.GetHungerPercentage();
                
                // Change color based on hunger level
                Image fillImage = hungerBar.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    float hungerPercent = feedingSystem.GetHungerPercentage();
                    if (hungerPercent > 0.6f)
                        fillImage.color = Color.green;
                    else if (hungerPercent > 0.3f)
                        fillImage.color = Color.yellow;
                    else
                        fillImage.color = Color.red;
                }
            }
        }
        
        private void UpdateSharkSize(float newSize)
        {
            if (sharkSizeIndicator != null)
            {
                // Scale the size indicator based on shark size
                float sizeRatio = newSize / 5f; // Assuming max size is 5
                sharkSizeIndicator.transform.localScale = Vector3.one * (0.5f + sizeRatio * 0.5f);
            }
        }
        
        #endregion
        
        #region Menu Management
        
        public void ShowMainMenu()
        {
            SetActiveMenu(mainMenu);
        }
        
        public void ShowPauseMenu()
        {
            SetActiveMenu(pauseMenu);
        }
        
        public void ShowGameOverMenu()
        {
            SetActiveMenu(gameOverMenu);
            UpdateGameOverStats();
        }
        
        public void ShowSettingsMenu()
        {
            SetActiveMenu(settingsMenu);
        }
        
        public void HideAllMenus()
        {
            SetActiveMenu(null);
        }
        
        private void SetActiveMenu(GameObject menuToShow)
        {
            if (mainMenu != null) mainMenu.SetActive(menuToShow == mainMenu);
            if (pauseMenu != null) pauseMenu.SetActive(menuToShow == pauseMenu);
            if (gameOverMenu != null) gameOverMenu.SetActive(menuToShow == gameOverMenu);
            if (settingsMenu != null) settingsMenu.SetActive(menuToShow == settingsMenu);
        }
        
        private void UpdateGameOverStats()
        {
            if (gameManager == null) return;
            
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {gameManager.GetCurrentScore():N0}";
                
            if (finalTimeText != null)
            {
                float time = gameManager.GetGameTime();
                int minutes = Mathf.FloorToInt(time / 60f);
                int seconds = Mathf.FloorToInt(time % 60f);
                finalTimeText.text = $"Time Survived: {minutes:00}:{seconds:00}";
            }
            
            if (finalFishCountText != null && feedingSystem != null)
                finalFishCountText.text = $"Fish Eaten: {feedingSystem.GetFishEaten()}";
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnGameStarted()
        {
            HideAllMenus();
        }
        
        private void OnGamePaused()
        {
            ShowPauseMenu();
        }
        
        private void OnGameResumed()
        {
            HideAllMenus();
        }
        
        private void OnGameOver()
        {
            ShowGameOverMenu();
        }
        
        #endregion
        
        #region Button Handlers
        
        public void StartGame()
        {
            if (gameManager != null)
            {
                HideAllMenus();
                // Game manager will handle starting the game
            }
        }
        
        public void TogglePause()
        {
            if (gameManager == null) return;
            
            if (gameManager.IsGamePaused())
            {
                gameManager.ResumeGame();
            }
            else
            {
                gameManager.PauseGame();
            }
        }
        
        public void RestartGame()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
        
        public void QuitGame()
        {
            if (gameManager != null)
            {
                gameManager.QuitGame();
            }
        }
        
        #endregion
        
        #region Settings Handlers
        
        private void OnSensitivityChanged(float value)
        {
            if (touchInput != null)
            {
                touchInput.SetTouchSensitivity(value);
            }
        }
        
        private void OnGyroscopeToggled(bool enabled)
        {
            if (touchInput != null)
            {
                touchInput.ToggleGyroscope(enabled);
            }
        }
        
        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                gameManager.OnScoreChanged -= UpdateScore;
                gameManager.OnGameTimeChanged -= UpdateTime;
                gameManager.OnGameStarted -= OnGameStarted;
                gameManager.OnGamePaused -= OnGamePaused;
                gameManager.OnGameResumed -= OnGameResumed;
                gameManager.OnGameOver -= OnGameOver;
            }
            
            if (feedingSystem != null)
            {
                feedingSystem.OnHungerChanged -= UpdateHunger;
                feedingSystem.OnSizeChanged -= UpdateSharkSize;
                feedingSystem.OnFishEaten -= UpdateFishCount;
            }
        }
    }
    
    // Simple joystick class for mobile controls (placeholder)
    [System.Serializable]
    public class Joystick : MonoBehaviour
    {
        public Vector2 Direction { get; private set; }
        
        // Implementation would go here for virtual joystick
        // This is a placeholder for the UI system
    }
}