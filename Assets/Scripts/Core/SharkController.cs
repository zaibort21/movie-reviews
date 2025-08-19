using UnityEngine;

namespace HungryShark.Core
{
    /// <summary>
    /// Controls shark movement with touch input optimized for mobile devices
    /// </summary>
    public class SharkController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float maxSpeed = 15f;
        [SerializeField] private float acceleration = 2f;
        [SerializeField] private float deceleration = 1f;
        
        [Header("Mobile Touch Controls")]
        [SerializeField] private float touchSensitivity = 2f;
        [SerializeField] private bool invertYTouch = false;
        
        [Header("Physics")]
        [SerializeField] private float waterDrag = 1f;
        [SerializeField] private float buoyancy = 0.5f;
        
        private Rigidbody rb;
        private Vector3 targetDirection;
        private float currentSpeed;
        private Vector2 touchStartPosition;
        private bool isTouching;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            
            // Configure rigidbody for underwater movement
            rb.useGravity = false;
            rb.drag = waterDrag;
        }
        
        private void Update()
        {
            HandleTouchInput();
            UpdateMovement();
            ApplyBuoyancy();
        }
        
        private void HandleTouchInput()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
            #elif UNITY_ANDROID || UNITY_IOS
            HandleMobileTouch();
            #endif
        }
        
        private void HandleMouseInput()
        {
            // Desktop/Editor input for testing
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPosition = Input.mousePosition;
                isTouching = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isTouching = false;
            }
            
            if (isTouching && Input.GetMouseButton(0))
            {
                Vector2 currentTouchPosition = Input.mousePosition;
                Vector2 touchDelta = (currentTouchPosition - touchStartPosition) * touchSensitivity * 0.01f;
                
                UpdateTargetDirection(touchDelta);
                touchStartPosition = currentTouchPosition;
            }
        }
        
        private void HandleMobileTouch()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        touchStartPosition = touch.position;
                        isTouching = true;
                        break;
                        
                    case TouchPhase.Moved:
                        Vector2 touchDelta = (touch.position - touchStartPosition) * touchSensitivity * 0.01f;
                        UpdateTargetDirection(touchDelta);
                        touchStartPosition = touch.position;
                        break;
                        
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        isTouching = false;
                        break;
                }
            }
        }
        
        private void UpdateTargetDirection(Vector2 touchDelta)
        {
            // Convert touch input to 3D movement direction
            Vector3 cameraRight = Camera.main.transform.right;
            Vector3 cameraUp = Camera.main.transform.up;
            
            // Apply touch delta to movement direction
            Vector3 inputDirection = cameraRight * touchDelta.x + cameraUp * (invertYTouch ? -touchDelta.y : touchDelta.y);
            
            if (inputDirection.magnitude > 0.1f)
            {
                targetDirection = inputDirection.normalized;
                currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
            }
            else
            {
                currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0f);
            }
        }
        
        private void UpdateMovement()
        {
            if (targetDirection.magnitude > 0.1f && currentSpeed > 0.1f)
            {
                // Move the shark
                Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
                rb.AddForce(movement, ForceMode.VelocityChange);
                
                // Rotate shark to face movement direction
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void ApplyBuoyancy()
        {
            // Simple buoyancy effect
            if (transform.position.y < 0) // Below water level
            {
                rb.AddForce(Vector3.up * buoyancy, ForceMode.Force);
            }
        }
        
        /// <summary>
        /// Get current shark speed for feeding system calculations
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }
        
        /// <summary>
        /// Apply speed boost effect (for power-ups)
        /// </summary>
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
        }
        
        private System.Collections.IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
        {
            float originalMaxSpeed = maxSpeed;
            maxSpeed *= multiplier;
            
            yield return new WaitForSeconds(duration);
            
            maxSpeed = originalMaxSpeed;
        }
    }
}