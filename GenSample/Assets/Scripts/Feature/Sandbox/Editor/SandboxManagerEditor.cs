using Assets.Scripts.Managers;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Editor
{
    [CustomEditor(typeof(SandboxManager))]
    public class SandboxManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SandboxManager manager = (SandboxManager)target;

            var style = new GUIStyle(GUI.skin.button);
            style.fontSize = 25;

            GUILayout.Space(10);
            if (GUILayout.Button("Save Map", style))
            {                
                manager.SaveMap();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Load Map", style))
            {
                if(EditorUtility.DisplayDialog("Load Map", "맵을 로드하시겠습니까? 기영님?ㅋ", "로드", "취소"))
                {
                    manager.LoadMap();
                }
            }            
        }
    }
}