using System;
using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Look Controller")]
    public class LookController : PlayerComponent
    {
        public bool lockCursor;

        [Header("Smoothing")]
        public bool smoothLook;
        public float smoothTime = 5f;
        public float smoothMultiplier = 2f;

        [Header("Sensitivity")]
        public float sensitivityX = 2f;
        public float sensitivityY = 2f;

        [Header("Look Limits")]
        public MinMax horizontalLimits = new(-360, 360);
        public MinMax verticalLimits = new(-80, 90);

        [Header("Offset")]
        public Vector2 offset;

        [Header("Debug"), ReadOnly]
        public Vector2 rotation;

        private bool blockLook;
        private MinMax horizontalLimitsOrig;
        private MinMax verticalLimitsOrig;

        private Vector2 targetLook;
        private Vector2 startingLook;
        private bool customLerp;

        public Vector2 DeltaInput { get; set; }
        public Quaternion RotationX { get; private set; }
        public Quaternion RotationY { get; private set; }
        public Quaternion RotationFinal { get; private set; }

        public bool LookLocked
        {
            get => blockLook;
            set => blockLook = value;
        }

        void Start()
        {
            verticalLimitsOrig = verticalLimits;
            horizontalLimitsOrig = horizontalLimits;
            if (lockCursor) GameTools.ShowCursor(true, false);

            OptionsManager.ObserveOption("sensitivity", (obj) =>
            {
                sensitivityX = (float)obj;
                sensitivityY = (float)obj;
            });

            OptionsManager.ObserveOption("smoothing", (obj) => smoothLook = (bool)obj);
            OptionsManager.ObserveOption("smoothing_speed", (obj) => smoothTime = (float)obj);
        }

        void Update()
        {
            if (Cursor.lockState != CursorLockMode.None && !blockLook && isEnabled)
            {
                DeltaInput = InputManager.ReadInput<Vector2>(Controls.LOOK);
            }
            else
            {
                DeltaInput = Vector2.zero;
            }

            rotation.x += DeltaInput.x * sensitivityX / 30 * MainCamera.fieldOfView + offset.x;
            rotation.y += DeltaInput.y * sensitivityY / 30 * MainCamera.fieldOfView + offset.y;

            rotation.x = ClampAngle(rotation.x, horizontalLimits.RealMin, horizontalLimits.RealMax);
            rotation.y = ClampAngle(rotation.y, verticalLimits.RealMin, verticalLimits.RealMax);

            RotationX = Quaternion.AngleAxis(rotation.x, Vector3.up);
            RotationY = Quaternion.AngleAxis(rotation.y, Vector3.left);
            RotationFinal = RotationX * RotationY;

            transform.localRotation = smoothLook ? Quaternion.Slerp(transform.localRotation, RotationFinal, smoothTime * smoothMultiplier * Time.deltaTime) : RotationFinal;

            offset.y = 0F;
            offset.x = 0F;
        }

        /// <summary>
        /// Lerp look rotation to a specific target rotation.
        /// </summary>
        public void LerpRotation(Vector2 target, float duration = 0.5f)
        {
            target.x = ClampAngle(target.x);
            target.y = ClampAngle(target.y);

            float xDiff = FixDiff(target.x - rotation.x);
            float yDiff = FixDiff(target.y - rotation.y);

            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration));
        }

        /// <summary>
        /// Lerp look rotation to a specific target rotation.
        /// </summary>
        public void LerpRotation(Vector2 target, Action onLerpComplete, float duration = 0.5f)
        {
            target.x = ClampAngle(target.x);
            target.y = ClampAngle(target.y);

            float xDiff = FixDiff(target.x - rotation.x);
            float yDiff = FixDiff(target.y - rotation.y);

            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration));
        }

        /// <summary>
        /// Lerp look rotation to a specific target transform.
        /// </summary>
        public void LerpRotation(Transform target, float duration = 0.5f, bool keepLookLocked = false)
        {
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

            Vector3 eulerRotation = rotationToTarget.eulerAngles;
            Vector2 targetRotation = new Vector2(eulerRotation.y, eulerRotation.x);

            // Clamp the target rotation angles.
            targetRotation.x = ClampAngle(targetRotation.x);
            targetRotation.y = ClampAngle(-targetRotation.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(targetRotation.x - rotation.x);
            float yDiff = FixDiff(targetRotation.y - rotation.y);

            // Start the lerp process.
            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), null, duration, keepLookLocked));
        }

        /// <summary>
        /// Lerp look rotation to a specific target transform.
        /// </summary>
        public void LerpRotation(Transform target, Action onLerpComplete, float duration = 0.5f, bool keepLookLocked = false)
        {
            Vector3 directionToTarget = target.position - transform.position;
            Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget);

            Vector3 eulerRotation = rotationToTarget.eulerAngles;
            Vector2 targetRotation = new Vector2(eulerRotation.y, eulerRotation.x);

            // Clamp the target rotation angles.
            targetRotation.x = ClampAngle(targetRotation.x);
            targetRotation.y = ClampAngle(targetRotation.y);

            // Calculate the differences in each axis.
            float xDiff = FixDiff(targetRotation.x - rotation.x);
            float yDiff = FixDiff(targetRotation.y - rotation.y);

            // Start the lerp process.
            StartCoroutine(DoLerpRotation(new Vector2(xDiff, yDiff), onLerpComplete, duration, keepLookLocked));
        }

        /// <summary>
        /// Lerp the look rotation and clamp the look rotation within limits relative to the rotation.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void LerpClampRotation(Vector3 relative, MinMax vLimits, MinMax hLimits, float duration = 0.5f)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - rotation.x);

            float targetAngle = rotation.x + remainder;
            float min = targetAngle - Mathf.Abs(hLimits.RealMin);
            float max = targetAngle + Mathf.Abs(hLimits.RealMax);

            if (min < -360)
            {
                min += 360;
                max += 360;
            }
            else if (max > 360)
            {
                min -= 360;
                max -= 360;
            }

            if (Mathf.Abs(targetAngle - rotation.x) > 180)
            {
                if (rotation.x > 0) rotation.x -= 360;
                else if (rotation.x < 0) rotation.x += 360;
            }

            hLimits = new MinMax(min, max);
            StartCoroutine(DoLerpClampRotation(targetAngle, vLimits, hLimits, duration));
        }

        /// <summary>
        /// Lerp the look rotation manually. This function should only be used in the Update() function.
        /// </summary>
        public void CustomLerp(Vector2 target, float t)
        {
            if (!customLerp)
            {
                targetLook.x = ClampAngle(target.x);
                targetLook.y = ClampAngle(target.y);
                startingLook = rotation;
                customLerp = true;
                blockLook = true;
            }

            if ((t = Mathf.Clamp01(t)) < 1)
            {
                rotation.x = Mathf.LerpAngle(startingLook.x, targetLook.x, t);
                rotation.y = Mathf.LerpAngle(startingLook.y, targetLook.y, t);
            }
        }

        /// <summary>
        /// Get remainder to relative rotation.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <returns></returns>
        public float GetLookRemainder(Vector3 relative)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - rotation.x);
            return rotation.x + remainder;
        }

        /// <summary>
        /// Reset lerp parameters.
        /// </summary>
        public void ResetCustomLerp()
        {
            StopAllCoroutines();
            targetLook = Vector2.zero;
            startingLook = Vector2.zero;
            customLerp = false;
            blockLook = false;
        }

        /// <summary>
        /// Set look rotation limits.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void SetLookLimits(Vector3 relative, MinMax vLimits, MinMax hLimits)
        {
            if (hLimits.HasValue)
            {
                float toAngle = ClampAngle(relative.y);
                float remainder = FixDiff(toAngle - rotation.x);

                float targetAngle = rotation.x + remainder;
                float min = targetAngle - Mathf.Abs(hLimits.RealMin);
                float max = targetAngle + Mathf.Abs(hLimits.RealMax);

                if (min < -360)
                {
                    min += 360;
                    max += 360;
                }
                else if (max > 360)
                {
                    min -= 360;
                    max -= 360;
                }

                if (Mathf.Abs(targetAngle - rotation.x) > 180)
                {
                    if (rotation.x > 0) rotation.x -= 360;
                    else if (rotation.x < 0) rotation.x += 360;
                }

                hLimits = new MinMax(min, max);
                horizontalLimits = hLimits;
            }

            verticalLimits = vLimits;
        }

        /// <summary>
        /// Set vertical look rotation limits.
        /// </summary>
        /// <param name="vLimits">Vertical Limits [Up, Down]</param>
        public void SetVerticalLimits(MinMax vLimits)
        {
            verticalLimits = vLimits;
        }

        /// <summary>
        /// Set horizontal look rotation limits.
        /// </summary>
        /// <param name="relative">Relative target rotation.</param>
        /// <param name="hLimits">Horizontal Limits [Left, Right]</param>
        public void SetHorizontalLimits(Vector3 relative, MinMax hLimits)
        {
            float toAngle = ClampAngle(relative.y);
            float remainder = FixDiff(toAngle - rotation.x);

            float targetAngle = rotation.x + remainder;
            float min = targetAngle - Mathf.Abs(hLimits.RealMin);
            float max = targetAngle + Mathf.Abs(hLimits.RealMax);

            if (min < -360)
            {
                min += 360;
                max += 360;
            }
            else if (max > 360)
            {
                min -= 360;
                max -= 360;
            }

            if (Mathf.Abs(targetAngle - rotation.x) > 180)
            {
                if (rotation.x > 0) rotation.x -= 360;
                else if (rotation.x < 0) rotation.x += 360;
            }

            hLimits = new MinMax(min, max);
            horizontalLimits = hLimits;
        }

        /// <summary>
        /// Reset look rotation to default limits.
        /// </summary>
        public void ResetLookLimits()
        {
            StopAllCoroutines();
            horizontalLimits = horizontalLimitsOrig;
            verticalLimits = verticalLimitsOrig;
        }

        /// <summary>
        /// Get the current rotation from the transform and apply its rotation to the controller.
        /// </summary>
        /// <remarks>Good to use from the timeline to set the look rotation from animation uisng the SignalEmitter.</remarks>
        public void ApplyLookFromTransform()
        {
            Vector3 eulerAngles = transform.localEulerAngles;
            rotation.x = ClampAngle(eulerAngles.y);
            rotation.y = ClampAngle(eulerAngles.x);
        }

        private IEnumerator DoLerpRotation(Vector2 target, Action onLerpComplete, float duration, bool keepLookLocked = false)
        {
            blockLook = true;

            target = new Vector2(rotation.x + target.x, rotation.y + target.y);
            Vector2 current = rotation;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                rotation.x = Mathf.LerpAngle(current.x, target.x, t);
                rotation.y = Mathf.LerpAngle(current.y, target.y, t);

                yield return null;
            }

            rotation = target;
            onLerpComplete?.Invoke();

            blockLook = keepLookLocked;
        }

        private IEnumerator DoLerpClampRotation(float newX, Vector2 vLimit, Vector2 hLimit, float duration, bool keepLookLocked = false)
        {
            blockLook = true;

            float newY = rotation.y < vLimit.x
                ? vLimit.x : rotation.y > vLimit.y
                ? vLimit.y : rotation.y;

            Vector2 target = new Vector2(newX, newY);
            Vector2 current = rotation;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

                rotation.x = Mathf.LerpAngle(current.x, target.x, t);
                rotation.y = Mathf.LerpAngle(current.y, target.y, t);

                yield return null;
            }

            rotation = target;
            horizontalLimits = hLimit;
            verticalLimits = vLimit;

            blockLook = keepLookLocked;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            float newAngle = angle.FixAngle();
            return Mathf.Clamp(newAngle, min, max);
        }

        private float ClampAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }

        private float FixDiff(float angleDiff)
        {
            if (angleDiff > 180f)
            {
                angleDiff -= 360f;
            }
            else if (angleDiff < -180f)
            {
                angleDiff += 360f;
            }

            return angleDiff;
        }
    }
}