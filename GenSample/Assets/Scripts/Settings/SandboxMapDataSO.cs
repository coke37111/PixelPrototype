using Assets.Scripts.Feature.Sandbox;
using Assets.Scripts.Feature.Sandbox.Cube;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    [CreateAssetMenu(fileName = "SBMapData", menuName = "SO/SandboxMapData")]
    public class SandboxMapDataSO : ScriptableObject
    {
        public List<CubeData> cubeData;

        public void SetData(List<CubeRoot> cubes)
        {
            if (cubeData == null)
                cubeData = new List<CubeData>();

            foreach(CubeRoot cube in cubes)
            {
                CubeBase cubeBase = cube.GetCubeBase();
                CubeData newData = new CubeData(cubeBase.name.Replace("(Clone)", "").Trim(), cubeBase.GetPosition());
                cubeData.Add(newData);
            }            
        }
    }

    [Serializable]
    public class CubeData
    {
        public string prefabName;
        public Vector3 pos;

        public CubeData(string prefabName, Vector3 pos)
        {
            this.prefabName = prefabName;
            this.pos = pos;
        }
    }
}