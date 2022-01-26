using Assets.Scripts.Feature.Main.Cube;
using Assets.Scripts.Feature.Sandbox;
using Assets.Scripts.Settings;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public class CreateSO
    {
        public static void CreateSandboxData(List<CubeRoot> cubes)
        {
            SandboxMapDataSO asset = ScriptableObject.CreateInstance<SandboxMapDataSO>();

            asset.SetData(cubes);

            AssetDatabase.CreateAsset(asset, $"Assets/Resources/Data/SandboxMap/SandboxMapData_{DateTime.Now.ToString("yyyyMMddHHmmss")}.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

        public static void CreateSandboxData(List<Cube> cubes)
        {
            SandboxMapDataSO asset = ScriptableObject.CreateInstance<SandboxMapDataSO>();

            asset.SetData(cubes);

            AssetDatabase.CreateAsset(asset, $"Assets/Resources/Data/SandboxMap/SandboxMapData_{DateTime.Now.ToString("yyyyMMddHHmmss")}.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
    }
}