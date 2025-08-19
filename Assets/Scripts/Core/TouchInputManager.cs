using UnityEngine;

namespace HungryShark.Core
{
    /// <summary>
    /// Handles all touch input for mobile devices with gesture recognition
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        [Header("Touch Settings")]
        [SerializeField] private float touchSensitivity = 2f;
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxSwipeTime = 1f;
        [SerializeField] private bool useGyroscope = false;
        
        [Header("Gesture Recognition")]
        [SerializeField] private float tapMaxDuration = 0.3f;
        [SerializeField] private float doubleTapMaxDelay = 0.5f;
        
        private Vector2 touchStartPosition;
        private Vector2 touchEndPosition;
        private float touchStartTime;
        private float lastTapTime;
        private bool isFirstTouch = true;
        
        // Events for different touch gestures
        public System.Action<Vector2> OnTouchStart;
        public System.Action<Vector2> OnTouchMove;
        public System.Action<Vector2> OnTouchEnd;
        public System.Action<Vector2> OnSwipe;
        public System.Action<Vector2> OnTap;
        public System.Action<Vector2> OnDoubleTap;
        public System.Action<float> OnPinch;
        
        // For gyroscope input
        private bool gyroEnabled;
        private Quaternion gyroInitialRotation;
        
        private void Start()
        {
            InitializeGyroscope();
        }
        
        private void Update()
        {
            #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
            #elif UNITY_ANDROID || UNITY_IOS
            HandleTouchInput();
            #endif
            
            if (useGyroscope && gyroEnabled)
            {
                HandleGyroscopeInput();
            }
        }
        
        private void InitializeGyroscope()
        {
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                gyroEnabled = true;
                gyroInitialRotation = Input.gyro.attitude;
            }
            else
            {
                gyroEnabled = false;
                useGyroscope = false;
            }
        }
        
        private void HandleMouseInput()
        {
            // Simulate touch input with mouse for testing in editor
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                touchStartPosition = mousePosition;
                touchStartTime = Time.time;
                OnTouchStart?.Invoke(mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                OnTouchMove?.Invoke(mousePosition - touchStartPosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 mousePosition = Input.mousePosition;
                touchEndPosition = mousePosition;
                HandleTouchEnd();
            }
        }
        
        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                HandleSingleTouch();
            }
            else if (Input.touchCount == 2)
            {
                HandlePinchGesture();
            }
        }
        
        private void HandleSingleTouch()
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    touchStartTime = Time.time;
                    OnTouchStart?.Invoke(touch.position);
                    break;
                    
                case TouchPhase.Moved:
                    Vector2 deltaPosition = touch.position - touchStartPosition;
                    OnTouchMove?.Invoke(deltaPosition * touchSensitivity);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    touchEndPosition = touch.position;
                    HandleTouchEnd();
                    break;
            }
        }
        
        private void HandleTouchEnd()
        {
            float touchDuration = Time.time - touchStartTime;
            Vector2 swipeVector = touchEndPosition - touchStartPosition;
            float swipeDistance = swipeVector.magnitude;
            
            OnTouchEnd?.Invoke(touchEndPosition);
            
            // Check for swipe
            if (swipeDistance >= minSwipeDistance && touchDuration <= maxSwipeTime)
            {
                Vector2 swipeDirection = swipeVector.normalized;
                OnSwipe?.Invoke(swipeDirection);
            }
            // Check for tap
            else if (touchDuration <= tapMaxDuration && swipeDistance < minSwipeDistance)
            {
                // Check for double tap
                if (!isFirstTouch && (Time.time - lastTapTime) <= doubleTapMaxDelay)
                {
                    OnDoubleTap?.Invoke(touchEndPosition);
                    isFirstTouch = true; // Reset to prevent triple tap
                }
                else
                {
                    OnTap?.Invoke(touchEndPosition);
                    lastTapTime = Time.time;
                    isFirstTouch = false;
                }
            }
        }
        
        private void HandlePinchGesture()
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            // Get current distance between fingers
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);
            
            // Get previous distance between fingers
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;
            float prevDistance = Vector2.Distance(touch1PrevPos, touch2PrevPos);
            
            // Calculate pinch delta
            float deltaDistance = currentDistance - prevDistance;
            OnPinch?.Invoke(deltaDistance);
        }
        
        private void HandleGyroscopeInput()
        {
            Quaternion gyroRotation = Input.gyro.attitude;
            Quaternion deltaRotation = gyroRotation * Quaternion.Inverse(gyroInitialRotation);
            
            // Convert gyroscope rotation to movement input
            Vector3 eulerAngles = deltaRotation.eulerAngles;
            Vector2 gyroInput = new Vector2(eulerAngles.y, eulerAngles.x);
            
            // Normalize to -1 to 1 range
            gyroInput.x = Mathf.DeltaAngle(0, gyroInput.x) / 90f;
            gyroInput.y = Mathf.DeltaAngle(0, gyroInput.y) / 90f;
            
            OnTouchMove?.Invoke(gyroInput * touchSensitivity);
        }
        
        #region Public Methods
        
        public void SetTouchSensitivity(float sensitivity)
        {
            touchSensitivity = sensitivity;
        }
        
        public void ToggleGyroscope(bool enable)
        {
            useGyroscope = enable && gyroEnabled;
        }
        
        public bool IsGyroscopeAvailable()
        {
            return gyroEnabled;
        }
        
        public void CalibrateGyroscope()
        {
            if (gyroEnabled)
            {
                gyroInitialRotation = Input.gyro.attitude;
            }
        }
        
        #endregion
        
        #region Gesture Utility Methods
        
        public static Vector2 GetSwipeDirection(Vector2 swipeVector)
        {
            Vector2 normalizedSwipe = swipeVector.normalized;
            
            // Determine primary direction
            if (Mathf.Abs(normalizedSwipe.x) > Mathf.Abs(normalizedSwipe.y))
            {
                return new Vector2(Mathf.Sign(normalizedSwipe.x), 0);
            }
            else
            {
                return new Vector2(0, Mathf.Sign(normalizedSwipe.y));
            }
        }
        
        public static bool IsVerticalSwipe(Vector2 swipeVector)
        {
            return Mathf.Abs(swipeVector.y) > Mathf.Abs(swipeVector.x);
        }
        
        public static bool IsHorizontalSwipe(Vector2 swipeVector)
        {
            return Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y);
        }
        
        #endregion
    }
}