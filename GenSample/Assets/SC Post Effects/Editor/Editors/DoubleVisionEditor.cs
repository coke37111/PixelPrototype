using UnityEditor.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcessEditor(typeof(DoubleVision))]
    public sealed class DoubleVisionEditor : PostProcessEffectEditor<DoubleVision>
    {
        SerializedParameterOverride mode;
        SerializedParameterOverride intensity;
        SerializedParameterOverride amount;

        public override void OnEnable()
        {
            mode = FindParameterOverride(x => x.mode);
            intensity = FindParameterOverride(x => x.intensity);
        }
        
        public override string GetDisplayTitle()
        {
            return base.GetDisplayTitle() + SCPE_GUI.ModeTitle(mode);
        }
        
        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("double-vision");

            SCPE_GUI.DisplaySetupWarning<DoubleVisionRenderer>();

            PropertyField(mode);
            PropertyField(intensity);
        }
    }
}