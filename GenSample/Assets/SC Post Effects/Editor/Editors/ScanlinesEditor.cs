using UnityEditor.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcessEditor(typeof(Scanlines))]
    public sealed class ScanlinesEditor : PostProcessEffectEditor<Scanlines>
    {
        SerializedParameterOverride intensity;
        SerializedParameterOverride amount;
        SerializedParameterOverride speed;

        public override void OnEnable()
        {
            intensity = FindParameterOverride(x => x.intensity);
            amount = FindParameterOverride(x => x.amount);
            speed = FindParameterOverride(x => x.speed);
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("scanlines");

            SCPE_GUI.DisplaySetupWarning<ScanlinesRenderer>();

            PropertyField(intensity);
            PropertyField(amount);
            PropertyField(speed);
        }
    }
}