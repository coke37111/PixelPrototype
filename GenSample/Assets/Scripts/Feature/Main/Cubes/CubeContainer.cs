﻿using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class CubeContainer : MonoBehaviour
    {
        private List<Vector3> spawnPosList = new List<Vector3>();

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

        public void GenerateCubes(SandboxMapDataSO mapData, bool isEdit = false)
        {
            foreach (CubeData cubeData in mapData.cubeData)
            {
                GameObject pfCube = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                GameObject goCube = Instantiate(pfCube, cubeData.pos, Quaternion.identity, transform);
                EditCube cube = goCube.GetComponent<EditCube>();
                cube.Build(cubeData.prefabName);

                if(cubeData.prefabName == "SpawnCube")
                {
                    spawnPosList.Add(cubeData.pos);

                    if (!isEdit)
                    {
                        SpawnCube spawnCube = goCube.GetComponentInChildren<SpawnCube>();
                        if (spawnCube != null)
                            spawnCube.HideGuide();
                    }
                }

                if (cubeData.prefabName == "GuardCube")
                {
                    GuardCube guardCube = goCube.GetComponentInChildren<GuardCube>();
                    guardCube.HideCube();
                }
            }
        }

        public Vector3 GetRandomSpawnPos()
        {
            if (spawnPosList.Count <= 0)
                return Vector3.zero;

            int idx = Random.Range(0, spawnPosList.Count);
            return spawnPosList[idx];
        }

        public void ShowGuardCube()
        {
            List<EditCube> cubes = GetAllCubes();
            foreach(EditCube eCube in cubes)
            {
                GuardCube guardCube = eCube.GetComponentInChildren<GuardCube>();
                if (guardCube != null)
                {
                    guardCube.ShowCube();
                }
            }
        }

        public void HideGuardCube()
        {
            List<EditCube> cubes = GetAllCubes();
            foreach (EditCube eCube in cubes)
            {
                GuardCube guardCube = eCube.GetComponentInChildren<GuardCube>();
                if (guardCube != null)
                {
                    guardCube.HideCube();
                }
            }
        }
    }
}