﻿using Assets.Scripts.Feature.Sandbox.Cube;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox
{
    public class CubeContainer : MonoBehaviour
    {
        public List<CubeRoot> GetAllCubes()
        {
            List<CubeRoot> results = new List<CubeRoot>();

            for(int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<CubeRoot>())
                {
                    results.Add(child.GetComponent<CubeRoot>());
                }
            }

            return results;
        }

        public void DestroyAllCubes()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<CubeRoot>())
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void GenerateCubes(SandboxMapDataSO mapData)
        {
            foreach (CubeData cubeData in mapData.cubeData)
            {
                GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>($"Prefab/Sandbox/LocalCube");
                GameObject goCubeRoot = Instantiate(pfCubeRoot, cubeData.pos, Quaternion.identity, transform);
                CubeRoot cubeRoot = goCubeRoot.GetComponent<CubeRoot>();
                cubeRoot.Init(cubeData.prefabName);
            }
        }
    }
}