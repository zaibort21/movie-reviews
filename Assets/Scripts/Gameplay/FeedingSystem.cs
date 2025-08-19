using UnityEngine;

namespace HungryShark.Gameplay
{
    /// <summary>
    /// Handles feeding mechanics including collision detection and size-based consumption
    /// </summary>
    public class FeedingSystem : MonoBehaviour
    {
        [Header("Feeding Settings")]
        [SerializeField] private float baseSize = 1f;
        [SerializeField] private float maxSize = 5f;
        [SerializeField] private float growthRate = 0.1f;
        [SerializeField] private float sizeConsumptionRatio = 0.8f; // Prey must be 80% smaller to consume
        
        [Header("Hunger System")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float hungerDecayRate = 5f; // per second
        [SerializeField] private float feedingAmount = 20f;
        
        [Header("Effects")]
        [SerializeField] private ParticleSystem feedingEffect;
        [SerializeField] private AudioClip feedingSound;
        
        private float currentSize;
        private float currentHunger;
        private int fishEaten = 0;
        private AudioSource audioSource;
        
        // Events
        public System.Action<float> OnSizeChanged;
        public System.Action<float> OnHungerChanged;
        public System.Action<int> OnFishEaten;
        public System.Action OnGameOver;
        
        private void Awake()
        {
            currentSize = baseSize;
            currentHunger = maxHunger;
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void Start()
        {
            UpdateSharkScale();
            OnSizeChanged?.Invoke(currentSize);
            OnHungerChanged?.Invoke(currentHunger);
        }
        
        private void Update()
        {
            UpdateHunger();
        }
        
        private void UpdateHunger()
        {
            currentHunger -= hungerDecayRate * Time.deltaTime;
            currentHunger = Mathf.Max(currentHunger, 0f);
            
            OnHungerChanged?.Invoke(currentHunger);
            
            if (currentHunger <= 0)
            {
                OnGameOver?.Invoke();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            PreyController prey = other.GetComponent<PreyController>();
            if (prey != null)
            {
                AttemptToConsumePrey(prey);
            }
        }
        
        private void AttemptToConsumePrey(PreyController prey)
        {
            float preySize = prey.GetSize();
            
            // Check if shark is large enough to consume the prey
            if (currentSize >= preySize * (1f + sizeConsumptionRatio))
            {
                ConsumePrey(prey);
            }
            else if (preySize >= currentSize * (1f + sizeConsumptionRatio))
            {
                // Prey is too large, damage the shark
                TakeDamage(preySize * 10f);
            }
        }
        
        private void ConsumePrey(PreyController prey)
        {
            // Increase size and hunger
            float sizeIncrease = prey.GetNutritionalValue() * growthRate;
            currentSize = Mathf.Min(currentSize + sizeIncrease, maxSize);
            
            float hungerRestored = feedingAmount * prey.GetNutritionalValue();
            currentHunger = Mathf.Min(currentHunger + hungerRestored, maxHunger);
            
            fishEaten++;
            
            // Update visual scale
            UpdateSharkScale();
            
            // Play effects
            PlayFeedingEffects(prey.transform.position);
            
            // Destroy prey
            prey.OnConsumed();
            
            // Trigger events
            OnSizeChanged?.Invoke(currentSize);
            OnHungerChanged?.Invoke(currentHunger);
            OnFishEaten?.Invoke(fishEaten);
            
            Debug.Log($"Consumed {prey.name}! Size: {currentSize:F2}, Hunger: {currentHunger:F1}, Fish Eaten: {fishEaten}");
        }
        
        private void TakeDamage(float damage)
        {
            currentHunger -= damage;
            currentHunger = Mathf.Max(currentHunger, 0f);
            
            OnHungerChanged?.Invoke(currentHunger);
            
            Debug.Log($"Took damage! Hunger: {currentHunger:F1}");
        }
        
        private void UpdateSharkScale()
        {
            // Scale the shark model based on current size
            float scaleMultiplier = currentSize / baseSize;
            transform.localScale = Vector3.one * scaleMultiplier;
        }
        
        private void PlayFeedingEffects(Vector3 position)
        {
            // Play particle effect
            if (feedingEffect != null)
            {
                feedingEffect.transform.position = position;
                feedingEffect.Play();
            }
            
            // Play sound effect
            if (feedingSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(feedingSound);
            }
        }
        
        #region Public Getters
        
        public float GetCurrentSize()
        {
            return currentSize;
        }
        
        public float GetCurrentHunger()
        {
            return currentHunger;
        }
        
        public float GetHungerPercentage()
        {
            return currentHunger / maxHunger;
        }
        
        public int GetFishEaten()
        {
            return fishEaten;
        }
        
        public bool CanConsume(float preySize)
        {
            return currentSize >= preySize * (1f + sizeConsumptionRatio);
        }
        
        #endregion
        
        #region Power-ups and Special Abilities
        
        public void ApplyGrowthBoost(float multiplier, float duration)
        {
            StartCoroutine(GrowthBoostCoroutine(multiplier, duration));
        }
        
        public void RestoreHunger(float amount)
        {
            currentHunger = Mathf.Min(currentHunger + amount, maxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }
        
        private System.Collections.IEnumerator GrowthBoostCoroutine(float multiplier, float duration)
        {
            float originalGrowthRate = growthRate;
            growthRate *= multiplier;
            
            yield return new WaitForSeconds(duration);
            
            growthRate = originalGrowthRate;
        }
        
        #endregion
    }
}