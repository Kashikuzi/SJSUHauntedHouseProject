using UnityEngine;
using UHFPS.Scriptable;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/motion-controller")]
    public class MotionController : PlayerComponent
    {
        public MotionBlender MotionBlender = new();

        public Transform HandsMotionTransform;
        public Transform HeadMotionTransform;
        public MotionPreset MotionPreset;

        public bool MotionSuppress = true;
        public float MotionSuppressSpeed = 2f;
        public float MotionResetSpeed = 2f;

        public float BobWave
        {
            get
            {
                bool flag1 = PlayerStateMachine.IsCurrent(PlayerStateMachine.WALK_STATE);
                bool flag2 = PlayerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE);
                bool flag3 = PlayerStateMachine.IsCurrent(PlayerStateMachine.CROUCH_STATE);

                if ((flag1 || flag2 || flag3)
                    && MotionBlender != null
                    && MotionBlender.Instance.TryGetValue("waveY", out object value))
                    return (float)value;

                return 0f;
            }
        }

        private void Start()
        {
            MotionBlender.Init(MotionPreset, HeadMotionTransform, this);
        }

        private void OnDestroy()
        {
            MotionBlender.Dispose();
        }

        private void Update()
        {
            if (MotionSuppress)
            {
                if (isEnabled && MotionBlender.Weight < 1f)
                {
                    MotionBlender.Weight = Mathf.MoveTowards(MotionBlender.Weight, 1f, Time.deltaTime * MotionResetSpeed);
                }
                else if (!isEnabled && MotionBlender.Weight > 0f)
                {
                    MotionBlender.Weight = Mathf.MoveTowards(MotionBlender.Weight, 0f, Time.deltaTime * MotionSuppressSpeed);
                }
            }

            MotionBlender.BlendMotions(Time.deltaTime, out var position, out var rotation);
            HeadMotionTransform.SetLocalPositionAndRotation(position, rotation);
        }
    }
}