using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    public sealed class RadialBlurRenderer : PostProcessEffectRenderer<RadialBlur>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.RadialBlur);
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            sheet.properties.SetFloat("_Amount", settings.amount.value / 50);
            sheet.properties.SetFloat("_Iterations", settings.iterations.value);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}