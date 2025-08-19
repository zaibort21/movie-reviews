using UnityEngine;
using UnityEngine.SceneManagement;

namespace HungryShark.Core
{
    /// <summary>
    /// Main game manager that coordinates all game systems
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game Settings")]
        [SerializeField] private float gameTimeScale = 1f;
        [SerializeField] private bool pauseOnFocusLoss = true;
        
        [Header("Spawning")]
        [SerializeField] private GameObject[] preyPrefabs;
        [SerializeField] private int maxPreyCount = 20;
        [SerializeField] private float spawnRadius = 50f;
        [SerializeField] private float spawnInterval = 2f;
        
        [Header("Ocean Boundaries")]
        [SerializeField] private Vector3 oceanSize = new Vector3(100f, 50f, 100f);
        [SerializeField] private Vector3 oceanCenter = Vector3.zero;
        
        private int currentScore = 0;
        private float gameTime = 0f;
        private bool isGameActive = true;
        private bool isGamePaused = false;
        
        private SharkController sharkController;
        private FeedingSystem feedingSystem;
        private int preyCount = 0;
        private float lastSpawnTime;
        
        // Game state events
        public System.Action<int> OnScoreChanged;
        public System.Action<float> OnGameTimeChanged;
        public System.Action OnGameStarted;
        public System.Action OnGamePaused;
        public System.Action OnGameResumed;
        public System.Action OnGameOver;
        
        // Singleton instance
        public static GameManager Instance { get; private set; }
        
        private void Awake()
        {
            // Implement singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // Find shark components
            GameObject shark = GameObject.FindGameObjectWithTag("Player");
            if (shark != null)
            {
                sharkController = shark.GetComponent<SharkController>();
                feedingSystem = shark.GetComponent<FeedingSystem>();
            }
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void Update()
        {
            if (!isGameActive || isGamePaused) return;
            
            UpdateGameTime();
            HandleSpawning();
            CheckBoundaries();
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (pauseOnFocusLoss && isGameActive)
            {
                if (hasFocus)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
        
        private void InitializeGame()
        {
            Time.timeScale = gameTimeScale;
            gameTime = 0f;
            currentScore = 0;
            preyCount = 0;
            isGameActive = true;
            isGamePaused = false;
            
            // Subscribe to feeding system events
            if (feedingSystem != null)
            {
                feedingSystem.OnFishEaten += OnFishEaten;
                feedingSystem.OnGameOver += HandleGameOver;
            }
            
            // Subscribe to prey events
            HungryShark.Gameplay.PreyController.OnPreyConsumed += OnPreyConsumed;
            
            OnGameStarted?.Invoke();
            
            // Initial prey spawn
            SpawnInitialPrey();
        }
        
        private void UpdateGameTime()
        {
            gameTime += Time.deltaTime;
            OnGameTimeChanged?.Invoke(gameTime);
        }
        
        private void HandleSpawning()
        {
            if (preyCount < maxPreyCount && Time.time - lastSpawnTime > spawnInterval)
            {
                SpawnPrey();
                lastSpawnTime = Time.time;
            }
        }
        
        private void SpawnInitialPrey()
        {
            int initialSpawnCount = Mathf.Min(maxPreyCount / 2, 10);
            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnPrey();
            }
        }
        
        private void SpawnPrey()
        {
            if (preyPrefabs.Length == 0) return;
            
            // Choose random prey prefab
            GameObject preyPrefab = preyPrefabs[Random.Range(0, preyPrefabs.Length)];
            
            // Find spawn position within ocean boundaries
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // Instantiate prey
            GameObject newPrey = Instantiate(preyPrefab, spawnPosition, Random.rotation);
            preyCount++;
            
            Debug.Log($"Spawned prey at {spawnPosition}. Total prey: {preyCount}");
        }
        
        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 randomPosition;
            int attempts = 0;
            
            do
            {
                // Generate random position within ocean bounds
                randomPosition = oceanCenter + new Vector3(
                    Random.Range(-oceanSize.x / 2, oceanSize.x / 2),
                    Random.Range(-oceanSize.y / 2, oceanSize.y / 2),
                    Random.Range(-oceanSize.z / 2, oceanSize.z / 2)
                );
                attempts++;
            }
            while (IsNearShark(randomPosition) && attempts < 10);
            
            return randomPosition;
        }
        
        private bool IsNearShark(Vector3 position)
        {
            if (sharkController == null) return false;
            
            float distance = Vector3.Distance(position, sharkController.transform.position);
            return distance < spawnRadius * 0.3f; // Don't spawn too close to shark
        }
        
        private void CheckBoundaries()
        {
            if (sharkController == null) return;
            
            Vector3 sharkPosition = sharkController.transform.position;
            Vector3 oceanMin = oceanCenter - oceanSize / 2;
            Vector3 oceanMax = oceanCenter + oceanSize / 2;
            
            // Check if shark is outside boundaries
            if (sharkPosition.x < oceanMin.x || sharkPosition.x > oceanMax.x ||
                sharkPosition.y < oceanMin.y || sharkPosition.y > oceanMax.y ||
                sharkPosition.z < oceanMin.z || sharkPosition.z > oceanMax.z)
            {
                // Push shark back towards center
                Vector3 directionToCenter = (oceanCenter - sharkPosition).normalized;
                sharkController.transform.position = Vector3.Lerp(sharkPosition, sharkPosition + directionToCenter * 5f, Time.deltaTime);
            }
        }
        
        #region Event Handlers
        
        private void OnFishEaten(int totalFishEaten)
        {
            currentScore += 10; // Base score per fish
            OnScoreChanged?.Invoke(currentScore);
        }
        
        private void OnPreyConsumed(HungryShark.Gameplay.PreyController prey)
        {
            preyCount--;
            
            // Add bonus score based on prey size
            float sizeBonus = prey.GetSize() * 5f;
            currentScore += Mathf.RoundToInt(sizeBonus);
            OnScoreChanged?.Invoke(currentScore);
        }
        
        private void HandleGameOver()
        {
            isGameActive = false;
            Time.timeScale = 0f;
            OnGameOver?.Invoke();
            
            Debug.Log($"Game Over! Final Score: {currentScore}, Time Survived: {gameTime:F1}s");
        }
        
        #endregion
        
        #region Public Methods
        
        public void PauseGame()
        {
            if (!isGameActive || isGamePaused) return;
            
            isGamePaused = true;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }
        
        public void ResumeGame()
        {
            if (!isGameActive || !isGamePaused) return;
            
            isGamePaused = false;
            Time.timeScale = gameTimeScale;
            OnGameResumed?.Invoke();
        }
        
        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
        
        public int GetCurrentScore()
        {
            return currentScore;
        }
        
        public float GetGameTime()
        {
            return gameTime;
        }
        
        public bool IsGameActive()
        {
            return isGameActive;
        }
        
        public bool IsGamePaused()
        {
            return isGamePaused;
        }
        
        #endregion
        
        #region Gizmos (for debugging)
        
        private void OnDrawGizmosSelected()
        {
            // Draw ocean boundaries
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(oceanCenter, oceanSize);
            
            // Draw spawn radius around shark
            if (sharkController != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(sharkController.transform.position, spawnRadius);
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (feedingSystem != null)
            {
                feedingSystem.OnFishEaten -= OnFishEaten;
                feedingSystem.OnGameOver -= HandleGameOver;
            }
            
            HungryShark.Gameplay.PreyController.OnPreyConsumed -= OnPreyConsumed;
        }
    }
}