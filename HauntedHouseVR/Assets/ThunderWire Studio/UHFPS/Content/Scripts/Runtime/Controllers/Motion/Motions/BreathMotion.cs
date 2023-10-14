using System;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    public class BreathMotion : SimpleMotionModule
    {
        public override string Name => "General/Breath Motion";

        [Header("General Settings")]
        public AnimationCurve breathingPattern = new(new(0, 1), new(1, 1));
        public float breathingRate;
        public float breathingIntensity;

        // Current time in the breathing cycle
        private float currentBreathingCycleTime;

        public override void MotionUpdate(float deltaTime)
        {
            // If not updatable, reset to initial conditions
            if (!IsUpdatable)
            {
                SetTargetPosition(Vector3.zero);
                currentBreathingCycleTime = 0f;
                return;
            }

            // Check if we've completed the breathing cycle, if so, reset the cycle
            if (currentBreathingCycleTime > breathingPattern[breathingPattern.length - 1].time)
                currentBreathingCycleTime = 0f;

            // Advance the breathing cycle
            currentBreathingCycleTime += Time.deltaTime * breathingRate;
            float evaluatedBreathingValue = breathingPattern.Evaluate(currentBreathingCycleTime) * breathingIntensity;

            // Create the breathing motion vector
            Vector3 breathingMotion = new Vector3(0, evaluatedBreathingValue, 0);
            SetTargetPosition(breathingMotion);
        }

        public override void Reset()
        {
            currentBreathingCycleTime = 0f;
        }
    }
}