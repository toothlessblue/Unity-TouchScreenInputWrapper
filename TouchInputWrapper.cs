using UnityEngine;

namespace Unity_TouchScreenInputWrapper
{
    public class TouchInputWrapper : MonoBehaviour
    {
        private static TouchInputWrapper instance;

        [SerializeField]
        private float holdThreshold = 0.5f;

        [SerializeField]
        private float dragDistanceThreshold = 0.5f;
        
        [SerializeField]
        private float pinchThreshold = 0.5f;

        [Space(20)]
        [Header("Below fields are only for displaying the current state")]
        [SerializeField]
        private TouchType holding;

        [SerializeField]
        private TouchType heldThisFrame;

        [SerializeField]
        private bool droppedThisFrame;

        [SerializeField]
        private TouchType touching;

        [SerializeField]
        private TouchType touchedThisFrame;

        [SerializeField]
        private TouchType dragging;

        [SerializeField]
        private bool draggedThisFrame;

        [SerializeField]
        private bool pinching;
        
        [SerializeField]
        private float pinchDelta;
        
        [SerializeField]
        private float lastPinchDistance;

        private float timeOfLastTouchDown;

        private TouchType touchDown {
            get {
                if (Input.GetMouseButtonDown(1) || (Input.GetMouseButtonDown(0) && Input.touchCount == 2)) {
                    return TouchType.Double;
                } else if (Input.GetMouseButtonDown(0)) {
                    return TouchType.Single;
                }

                return TouchType.None;
            }
        }

        private bool touchUp => Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);

        private Vector2 touchStartPos;
        private Vector2 touchEndPos;
        private Vector2 touchCurrentPos;

        private Vector2 lastTouchPos;
        private Vector2 touchPosDelta;

        private void Awake() {
            if (TouchInputWrapper.instance) {
                Debug.LogWarning(
                    "Multiple instances of InputWrapper exist, this doesn't affect usage, but will affect performance and is un-needed."
                );
            }

            TouchInputWrapper.instance = this;
        }

        private void Update() {
            this.touchCurrentPos = Input.mousePosition;
            
            this.touchPosDelta = this.touchCurrentPos - this.lastTouchPos;
            this.touchedThisFrame = TouchType.None;
            this.droppedThisFrame = false;
            this.draggedThisFrame = false;

            if (this.holding != TouchType.None) {
                this.heldThisFrame = TouchType.None;
            }

            TouchType touchDownType = this.touchDown;
            if (touchDownType != TouchType.None) {
                this.touching = touchDownType;
                this.timeOfLastTouchDown = Time.time;
                this.touchStartPos = this.touchCurrentPos;
            }

            // Reset on touch up
            if (this.touchUp) {
                if (this.holding == TouchType.None && this.dragging == TouchType.None) {
                    this.touchedThisFrame = this.touching;
                } else if (this.dragging != TouchType.None) {
                    this.droppedThisFrame = true;
                }

                this.dragging = TouchType.None;
                this.touching = TouchType.None;
                this.holding = TouchType.None;
                this.touchEndPos = this.touchCurrentPos;
                
                this.pinching = false;
                this.pinchDelta = 0;
                this.lastPinchDistance = 0;
            }

            // Holding
            if (this.touching != TouchType.None && Time.time - this.timeOfLastTouchDown > this.holdThreshold &&
                this.holding == TouchType.None && this.dragging == TouchType.None) {
                this.holding = this.touching;
                this.heldThisFrame = this.touching;
            }

            // Pinching
            if (this.touching == TouchType.Double && SystemInfo.deviceType != DeviceType.Desktop) {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);
                float distance = Vector2.Distance(touch0.position, touch1.position);

                if (distance > this.pinchThreshold) {
                    this.pinching = true;
                    this.lastPinchDistance = distance;
                }
            }

            if (this.pinching) {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);
                float distance = Vector2.Distance(touch0.position, touch1.position);

                this.pinchDelta = distance - this.lastPinchDistance;
                this.lastPinchDistance = distance;
            }

            if (SystemInfo.deviceType == DeviceType.Desktop) {
                this.pinchDelta = Input.mouseScrollDelta.y;
            }
            
            // Dragging
            if (!this.pinching && (this.touchCurrentPos - this.touchStartPos).magnitude > this.dragDistanceThreshold && this.touching != TouchType.None && this.holding == TouchType.None) {
                this.dragging = this.touching;
                this.draggedThisFrame = true;
            }

            this.lastTouchPos = this.touchCurrentPos;
        }

        /// <summary>
        /// Returns true on the frame the user lifts their finger from the screen, if they do not drag or hold longer than the threshold.
        /// </summary>
        public static bool getTouch() => TouchInputWrapper.instance.touchedThisFrame == TouchType.Single;

        /// <summary>
        /// Returns true on the frame the user lifts their finger from the screen, if they do not drag or hold longer than the threshold, and they were using two fingers
        /// </summary>
        public static bool getDoubleTouch() => TouchInputWrapper.instance.touchedThisFrame == TouchType.Double;

        /// <summary>
        /// Returns true on the frame the user puts their finger on the screen
        /// </summary>
        public static bool getDown() => TouchInputWrapper.instance.touchDown == TouchType.Single;

        /// <summary>
        /// Returns true on the frame the user puts their finger on the screen
        /// </summary>
        public static bool getDoubleDown() => TouchInputWrapper.instance.touchDown == TouchType.Double;

        /// <summary>
        /// Returns true if the user is currently touching the screen, regardless of other factors
        /// </summary>
        public static bool getTouching() => TouchInputWrapper.instance.touching == TouchType.Single;

        /// <summary>
        /// Returns true on the frame the user has been touching up the screen for longer than the specified threshold time.
        /// </summary>
        public static bool getHeld() => TouchInputWrapper.instance.heldThisFrame == TouchType.Single;

        /// <summary>
        /// Returns true if the user did not hold, and did not touch, and DID move position further than the threshold.
        /// </summary>
        public static bool isDragging() => TouchInputWrapper.instance.dragging == TouchType.Single;

        /// <summary>
        /// Returns true if the user did not hold, and did not touch, and DID move position further than the threshold, and they were using two fingers
        /// </summary>
        public static bool isDoubleDragging() => TouchInputWrapper.instance.dragging == TouchType.Double;

        /// <summary>
        /// Returns true on the frame the user lets got when dragging. I.E. returns true when isDragging begins returning false again
        /// </summary>
        public static bool getDropped() => TouchInputWrapper.instance.droppedThisFrame;

        /// <summary>
        /// Returns true on the frame a drag begins
        /// </summary>
        public static bool getDragged() => TouchInputWrapper.instance.draggedThisFrame;

        /// <summary>
        /// Returns the position the last touch began at
        /// </summary>
        public static Vector2 touchStartPosition() => TouchInputWrapper.instance.touchStartPos;

        /// <summary>
        /// Returns the current touch position
        /// </summary>
        public static Vector2 touchPosition() => TouchInputWrapper.instance.touchCurrentPos;

        /// <summary>
        /// Returns the change in touch position this frame
        /// </summary>
        public static Vector2 touchDelta() => TouchInputWrapper.instance.touchPosDelta;

        /// <summary>
        /// Returns true if the user is currently dragging within the specified rect transform
        /// </summary>
        /// <param name="rectTransform">Transform to check with</param>
        public static bool isDraggingWithinRect(RectTransform rectTransform) {
            if (!TouchInputWrapper.isDragging()) return false;
            
            Vector2 localMouse = rectTransform.InverseTransformPoint(TouchInputWrapper.instance.touchStartPos);
            return rectTransform.rect.Contains(localMouse);
        }

        public static bool touchIsWithinRect(RectTransform rectTransform) {
            Vector2 localMouse = rectTransform.InverseTransformPoint(TouchInputWrapper.instance.touchCurrentPos);
            return rectTransform.rect.Contains(localMouse);
        }

        public static bool isPinching() {
            return TouchInputWrapper.instance.pinching || SystemInfo.deviceType == DeviceType.Desktop;
        }

        public static float getPinchDelta() {
            return TouchInputWrapper.instance.pinchDelta;
        }

        /// <summary>
        /// Returns true if the user is dragging, touched this frame, held this frame, or pinching
        /// </summary>
        public static bool interactedThisFrame() => TouchInputWrapper.instance.dragging != TouchType.None ||
                                                    TouchInputWrapper.instance.touchedThisFrame != TouchType.None ||
                                                    TouchInputWrapper.instance.heldThisFrame != TouchType.None ||
                                                    TouchInputWrapper.instance.pinching;

        private enum TouchType
        {
            None,
            Single,
            Double
        }
    }
}