using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System;
using System.Collections.Generic;
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
                if(manager.CanSaveMap())
                    SaveData(manager.GetCubes());
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Load Map", style))
            {
                if(EditorUtility.DisplayDialog("Load Map", "맵을 로드하시겠습니까?", "로드", "취소"))
                {
                    manager.LoadMap();
                }
            }            
        }

        private void SaveData(List<Main.nsCube.EditCube> cubes)
        {
            SandboxMapDataSO asset = CreateInstance<SandboxMapDataSO>();

            asset.SetData(cubes);

            AssetDatabase.CreateAsset(asset, $"Assets/Resources/Data/SandboxMap/SandboxMapData_{DateTime.Now.ToString("yyyyMMddHHmmss")}.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}