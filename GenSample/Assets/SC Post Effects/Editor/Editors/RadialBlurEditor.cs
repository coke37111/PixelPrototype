using UnityEditor.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcessEditor(typeof(RadialBlur))]
    public sealed class RadialBlurEditor : PostProcessEffectEditor<RadialBlur>
    {
        SerializedParameterOverride amount;
        SerializedParameterOverride iterations;

        public override void OnEnable()
        {
            amount = FindParameterOverride(x => x.amount);
            iterations = FindParameterOverride(x => x.iterations);
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("radial-blur");

            SCPE_GUI.DisplaySetupWarning<RadialBlurRenderer>();

            PropertyField(amount);
            PropertyField(iterations);
        }
    }
}