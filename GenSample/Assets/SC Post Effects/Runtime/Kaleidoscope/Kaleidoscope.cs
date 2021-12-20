using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcess(typeof(KaleidoscopeRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Misc/Kaleidoscope", true)]
    [Serializable]
    public sealed class Kaleidoscope : PostProcessEffectSettings
    {
        [Range(0f, 10f), Tooltip("The number of times the screen is split up")]
        public UnityEngine.Rendering.PostProcessing.IntParameter splits = new UnityEngine.Rendering.PostProcessing.IntParameter { value = 0 };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (splits == 0) { return false; }
                return true;
            }

            return false;
        }
    }
}