using UnityEditor.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcessEditor(typeof(Kaleidoscope))]
    public sealed class KaleidoscopeEditor : PostProcessEffectEditor<Kaleidoscope>
    {
        SerializedParameterOverride splits;

        public override void OnEnable()
        {
            splits = FindParameterOverride(x => x.splits);
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("kaleidoscope");

            SCPE_GUI.DisplaySetupWarning<KaleidoscopeRenderer>();

            PropertyField(splits);
        }
    }
}