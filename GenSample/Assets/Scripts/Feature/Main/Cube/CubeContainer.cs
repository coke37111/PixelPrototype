using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cube
{
    public class CubeContainer : MonoBehaviour
    {
        public List<EditCube> GetAllCubes()
        {
            List<EditCube> results = new List<EditCube>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<EditCube>())
                {
                    results.Add(child.GetComponent<EditCube>());
                }
            }

            return results;
        }

        public void DestroyAllCubes()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<EditCube>())
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void GenerateCubes(SandboxMapDataSO mapData)
        {
            foreach (CubeData cubeData in mapData.cubeData)
            {
                GameObject pfCube = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                GameObject goCube = Instantiate(pfCube, cubeData.pos, Quaternion.identity, transform);
                EditCube cube = goCube.GetComponent<EditCube>();
                cube.Build(cubeData.prefabName);
            }
        }
    }
}