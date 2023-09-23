using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System;

namespace UHFPS.Rendering
{
    [Serializable, VolumeComponentMenu("UHFPS PostProcess/Blood Disortion")]
    public class BloodDisortion : VolumeComponent, IPostProcessComponent
    {
        public ColorParameter BlendColor = new(Color.white);
        public ColorParameter OverlayColor = new(Color.white);
        public Texture2DParameter BlendTexture = new(null);
        public Texture2DParameter BumpTexture = new(null);

        public ClampedFloatParameter BloodAmount = new(0f, 0f, 1f);
        public ClampedFloatParameter MinBloodAmount = new(0f, 0f, 1f);
        public ClampedFloatParameter MaxBloodAmount = new(1f, 0f, 1f);
        public ClampedFloatParameter EdgeSharpness = new(0.5f, 0f, 1f);
        public ClampedFloatParameter Distortion = new(0.5f, 0f, 1f);

        public bool IsActive() => BloodAmount.value > 0;
        public bool IsTileCompatible() => false;

        public override void Override(VolumeComponent state, float interpFactor)
        {
            if (state is BloodDisortion _state)
            {
                _state.BlendColor.value = BlendColor.value;
                _state.OverlayColor.value = OverlayColor.value;
                _state.BlendTexture.value = BlendTexture.value;
                _state.BumpTexture.value = BumpTexture.value;

                _state.BloodAmount.Interp(_state.BloodAmount.value, BloodAmount.value, interpFactor);
                _state.MinBloodAmount.value = MinBloodAmount.value;
                _state.MaxBloodAmount.value = MaxBloodAmount.value;
                _state.EdgeSharpness.value = EdgeSharpness.value;
                _state.Distortion.value = Distortion.value;
            }
        }
    }
}