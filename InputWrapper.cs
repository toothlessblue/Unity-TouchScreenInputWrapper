using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity_TouchScreenInput {
    public class InputWrapper : MonoBehaviour
    {
        private static InputWrapper instance;

        [SerializeField]
        private float holdThreshold = .5f;

        [SerializeField]
        private float dragDistanceThreshold = .5f;

        // For displaying in unity editor
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

        private bool touchUp {
            get => Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
        }

        private Vector2 touchStartPos;
        private Vector2 touchEndPos;
        private Vector2 touchCurrentPos;

        private Vector2 lastTouchPos;
        private Vector2 touchPosDelta;

    private void Awake() {
        if (InputWrapper.instance) {
            Debug.LogWarning(
                "Multiple instances of InputWrapper exist, this doesn't affect usage, but will affect performance and is un-needed."
            );
        }

        InputWrapper.instance = this;
    }

    void Update() {
        this.touchCurrentPos = Input.mousePosition;
        this.touchPosDelta = this.touchCurrentPos - this.lastTouchPos;
        this.touchedThisFrame = TouchType.None;
        this.droppedThisFrame = false;

        if (this.holding != TouchType.None) {
            this.heldThisFrame = TouchType.None;
        }

        TouchType touchDownType = this.touchDown;
        if (touchDownType != TouchType.None) {
            this.touching = touchDownType;
            this.timeOfLastTouchDown = Time.time;
            this.touchStartPos = this.touchCurrentPos;
        }

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
        }

        if (this.touching != TouchType.None && Time.time - this.timeOfLastTouchDown > this.holdThreshold &&
            this.holding == TouchType.None && this.dragging == TouchType.None) {
            this.holding = this.touching;
            this.heldThisFrame = this.touching;
        }

        if ((this.touchCurrentPos - this.touchStartPos).magnitude > this.dragDistanceThreshold &&
            this.touching != TouchType.None && this.holding == TouchType.None) {
            this.dragging = this.touching;
        }

        this.lastTouchPos = this.touchCurrentPos;
    }

    /// <summary>
    /// Returns true on the frame the user lifts their finger from the screen, if they do not drag or hold longer than the threshold.
    /// </summary>
    public static bool getTouch() => InputWrapper.instance.touchedThisFrame == TouchType.Single;

    /// <summary>
    /// Returns true on the frame the user lifts their finger from the screen, if they do not drag or hold longer than the threshold, and they were using two fingers
    /// </summary>
    public static bool getDoubleTouch() => InputWrapper.instance.touchedThisFrame == TouchType.Double;

    /// <summary>
    /// Returns true on the frame the user puts their finger on the screen
    /// </summary>
    public static bool getDown() => InputWrapper.instance.touchDown == TouchType.Single;

    /// <summary>
    /// Returns true on the frame the user puts their finger on the screen
    /// </summary>
    public static bool getDoubleDown() => InputWrapper.instance.touchDown == TouchType.Double;

    /// <summary>
    /// Returns true if the user is currently touching the screen, regardless of other factors
    /// </summary>
    public static bool getTouching() => InputWrapper.instance.touching == TouchType.Single;

    /// <summary>
    /// Returns true on the frame the user has been touching up the screen for longer than the specified threshold time.
    /// </summary>
    public static bool getHeld() => InputWrapper.instance.heldThisFrame == TouchType.Single;

    /// <summary>
    /// Returns true if the user did not hold, and did not touch, and DID move position further than the threshold.
    /// </summary>
    public static bool isDragging() => InputWrapper.instance.dragging == TouchType.Single;

    /// <summary>
    /// Returns true if the user did not hold, and did not touch, and DID move position further than the threshold, and they were using two fingers
    /// </summary>
    public static bool isDoubleDragging() => InputWrapper.instance.dragging == TouchType.Double;

    /// <summary>
    /// Returns true on the frame the user lets got when dragging. I.E. returns true when isDragging begins returning false again
    /// </summary>
    public static bool getDropped() => InputWrapper.instance.droppedThisFrame;

    /// <summary>
    /// Returns the current touch position
    /// </summary>
    public static Vector2 touchPosition() => InputWrapper.instance.touchCurrentPos;

    /// <summary>
    /// Returns the change in touch position this frame
    /// </summary>
    public static Vector2 touchDelta() => InputWrapper.instance.touchPosDelta;

    /// <summary>
    /// Returns true if the user is currently dragging within the specified rect transform
    /// </summary>
    /// <param name="rectTransform">Transform to check with</param>
    public static bool isDraggingWithinRect(RectTransform rectTransform) {
        Vector2 localMouse = rectTransform.InverseTransformPoint(InputWrapper.instance.touchStartPos);
        return rectTransform.rect.Contains(localMouse);
    }

    /// <summary>
    /// Returns true if the user is dragging, touched this frame, or held this frame
    /// </summary>
    public static bool interactedThisFrame() => InputWrapper.instance.dragging != TouchType.None ||
                                                InputWrapper.instance.touchedThisFrame != TouchType.None ||
                                                InputWrapper.instance.heldThisFrame != TouchType.None;

    private enum TouchType
    {
        None,
        Single,
        Double
    }
}
}