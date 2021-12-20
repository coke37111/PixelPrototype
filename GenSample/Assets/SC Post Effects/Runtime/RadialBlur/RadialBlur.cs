using System;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.Rendering.PostProcessing;
using TextureParameter = UnityEngine.Rendering.PostProcessing.TextureParameter;
using BoolParameter = UnityEngine.Rendering.PostProcessing.BoolParameter;
using FloatParameter = UnityEngine.Rendering.PostProcessing.FloatParameter;
using IntParameter = UnityEngine.Rendering.PostProcessing.IntParameter;
using ColorParameter = UnityEngine.Rendering.PostProcessing.ColorParameter;
using MinAttribute = UnityEngine.Rendering.PostProcessing.MinAttribute;

namespace SCPE
{
    [PostProcess(typeof(RadialBlurRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Blurring/Radial Blur", true)]
    [Serializable]
    public sealed class RadialBlur : PostProcessEffectSettings
    {
        [Range(0f, 1f)]
        public FloatParameter amount = new FloatParameter { value = 0f };
        [Range(3, 12)]
        public IntParameter iterations = new IntParameter { value = 6 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0) { return false; }
                return true;
            }

            return false;
        }
    }
}