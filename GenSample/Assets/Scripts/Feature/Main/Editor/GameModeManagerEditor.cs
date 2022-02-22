using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Editor
{
    [CustomEditor(typeof(GameModeManager))]
    public class GameModeManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GameModeManager manager = (GameModeManager)target;

            var style = new GUIStyle(GUI.skin.button);
            style.fontSize = 15;

            GUILayout.Space(10);
            if (GUILayout.Button("Make DummyPlayer", style))
            {
                manager.SpawnDummyPlayer();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Destroy All DummyPlayer", style))
            {
                manager.DestroyAllDummyPlayer();
            }
        }
    }
}