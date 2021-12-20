using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    public sealed class KaleidoscopeRenderer : PostProcessEffectRenderer<Kaleidoscope>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.Kaleidoscope);
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetFloat("_Splits", Mathf.PI * 2 / Mathf.Max(1, settings.splits.value));
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}