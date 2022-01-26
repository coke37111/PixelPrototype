using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cube
{
    public class CubeContainer : MonoBehaviour
    {
        private readonly string cubePath = "Prefab/Main/Cube/Cube";

        public List<Cube> GetAllCubes()
        {
            List<Cube> results = new List<Cube>();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<Cube>())
                {
                    results.Add(child.GetComponent<Cube>());
                }
            }

            return results;
        }

        public void DestroyAllCubes()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<Cube>())
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void GenerateCubes(SandboxMapDataSO mapData)
        {
            foreach (CubeData cubeData in mapData.cubeData)
            {
                GameObject pfCube = ResourceManager.LoadAsset<GameObject>(cubePath);
                GameObject goCube = Instantiate(pfCube, cubeData.pos, Quaternion.identity, transform);
                Cube cube = goCube.GetComponent<Cube>();
                cube.Build(cubeData.prefabName);
            }
        }
    }
}