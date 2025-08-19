using UnityEngine;

namespace HungryShark.Gameplay
{
    /// <summary>
    /// Controls prey behavior including movement, AI, and interaction with the shark
    /// </summary>
    public class PreyController : MonoBehaviour
    {
        [Header("Prey Settings")]
        [SerializeField] private float size = 0.5f;
        [SerializeField] private float nutritionalValue = 1f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float fleeSpeed = 6f;
        
        [Header("AI Behavior")]
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private float fleeDistance = 12f;
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private float changeDirectionTime = 3f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem deathEffect;
        [SerializeField] private AudioClip deathSound;
        
        private Transform sharkTransform;
        private Vector3 wanderCenter;
        private Vector3 moveDirection;
        private float lastDirectionChange;
        private bool isFleeing;
        private Rigidbody rb;
        private AudioSource audioSource;
        
        public enum PreyState
        {
            Wandering,
            Fleeing,
            Schooling
        }
        
        private PreyState currentState = PreyState.Wandering;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure rigidbody for fish movement
            rb.useGravity = false;
            rb.drag = 2f;
            
            wanderCenter = transform.position;
            ChooseNewDirection();
        }
        
        private void Start()
        {
            // Find the shark (assumes there's only one shark with the tag "Player")
            GameObject shark = GameObject.FindGameObjectWithTag("Player");
            if (shark != null)
            {
                sharkTransform = shark.transform;
            }
            
            // Scale the fish based on its size
            transform.localScale = Vector3.one * size;
        }
        
        private void Update()
        {
            UpdateAI();
            Move();
            
            // Occasionally change direction when wandering
            if (currentState == PreyState.Wandering && Time.time - lastDirectionChange > changeDirectionTime)
            {
                ChooseNewDirection();
            }
        }
        
        private void UpdateAI()
        {
            if (sharkTransform == null) return;
            
            float distanceToShark = Vector3.Distance(transform.position, sharkTransform.position);
            
            // Check if shark is within detection range
            if (distanceToShark <= detectionRange)
            {
                // Check if the shark can consume this prey
                FeedingSystem sharkFeeding = sharkTransform.GetComponent<FeedingSystem>();
                if (sharkFeeding != null && sharkFeeding.CanConsume(size))
                {
                    // Flee from the shark
                    currentState = PreyState.Fleeing;
                    FleeFromShark();
                }
                else if (distanceToShark > fleeDistance)
                {
                    // Return to wandering if far enough
                    currentState = PreyState.Wandering;
                    isFleeing = false;
                }
            }
            else if (currentState == PreyState.Fleeing)
            {
                // Return to wandering if shark is far away
                currentState = PreyState.Wandering;
                isFleeing = false;
            }
        }
        
        private void FleeFromShark()
        {
            if (sharkTransform == null) return;
            
            // Calculate flee direction (away from shark)
            Vector3 fleeDirection = (transform.position - sharkTransform.position).normalized;
            
            // Add some randomness to avoid predictable movement
            fleeDirection += Random.insideUnitSphere * 0.3f;
            fleeDirection.Normalize();
            
            moveDirection = fleeDirection;
            isFleeing = true;
        }
        
        private void ChooseNewDirection()
        {
            // Random direction for wandering
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y *= 0.5f; // Reduce vertical movement
            
            // Stay within wander area
            Vector3 toCenter = wanderCenter - transform.position;
            if (toCenter.magnitude > wanderRadius)
            {
                randomDirection += toCenter.normalized * 0.5f;
            }
            
            moveDirection = randomDirection.normalized;
            lastDirectionChange = Time.time;
        }
        
        private void Move()
        {
            float currentSpeed = isFleeing ? fleeSpeed : moveSpeed;
            
            if (moveDirection.magnitude > 0.1f)
            {
                // Apply movement
                Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
                rb.AddForce(movement, ForceMode.VelocityChange);
                
                // Rotate to face movement direction
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 3f * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Called when this prey is consumed by the shark
        /// </summary>
        public void OnConsumed()
        {
            PlayDeathEffects();
            
            // Notify any systems that might be tracking this prey
            OnPreyConsumed?.Invoke(this);
            
            // Destroy the prey object
            Destroy(gameObject);
        }
        
        private void PlayDeathEffects()
        {
            // Play particle effect
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            
            // Play sound effect
            if (deathSound != null && audioSource != null)
            {
                // Create a temporary audio source since this object will be destroyed
                GameObject tempAudio = new GameObject("TempAudio");
                AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
                tempSource.clip = deathSound;
                tempSource.Play();
                
                Destroy(tempAudio, deathSound.length);
            }
        }
        
        #region Public Getters
        
        public float GetSize()
        {
            return size;
        }
        
        public float GetNutritionalValue()
        {
            return nutritionalValue;
        }
        
        public PreyState GetCurrentState()
        {
            return currentState;
        }
        
        #endregion
        
        #region Events
        
        public static System.Action<PreyController> OnPreyConsumed;
        
        #endregion
        
        #region Gizmos (for debugging)
        
        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw flee distance
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, fleeDistance);
            
            // Draw wander area
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(wanderCenter, wanderRadius);
        }
        
        #endregion
    }
}