using UnityEditor.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcessEditor(typeof(Sharpen))]
    public sealed class SharpenEditor : PostProcessEffectEditor<Sharpen>
    {
        SerializedParameterOverride amount;
        SerializedParameterOverride radius;

        public override void OnEnable()
        {
            amount = FindParameterOverride(x => x.amount);
            radius = FindParameterOverride(x => x.radius);
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("sharpen");

            SCPE_GUI.DisplaySetupWarning<SharpenRenderer>();

            PropertyField(amount);
            PropertyField(radius);
        }
    }
}