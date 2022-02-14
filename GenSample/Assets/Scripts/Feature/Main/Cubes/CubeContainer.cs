using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class CubeContainer : MonoBehaviour
    {
        private List<Vector3> spawnPosList = new List<Vector3>();
        private List<CubeData> normalCubeData = new List<CubeData>();

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

            spawnPosList.Clear();
            normalCubeData.Clear();
        }

        public void GenerateCubes(SandboxMapDataSO mapData, bool isEdit = false)
        {
            foreach (CubeData cubeData in mapData.cubeData)
            {
                if (cubeData.prefabName == "NormalCube")
                {
                    normalCubeData.Add(cubeData);
                    continue;
                }

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

        public void GenerateNormalCube()
        {
            if (normalCubeData == null)
                return;

            if (PlayerSettings.IsConnectNetwork())
            {
                foreach (CubeData cubeData in normalCubeData)
                {
                    var data = new List<object>();
                    data.Add(cubeData.prefabName);

                    PhotonNetwork.Instantiate(PrefabPath.EditCubePath, cubeData.pos, Quaternion.identity, 0, data.ToArray());
                }
            }
            else
            {
                foreach (CubeData cubeData in normalCubeData)
                {
                    GameObject pfCube = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                    GameObject goCube = Instantiate(pfCube, cubeData.pos, Quaternion.identity, transform);
                    EditCube cube = goCube.GetComponent<EditCube>();
                    cube.Build(cubeData.prefabName);
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

        public Vector3 GetRandomSpawnPosEdit()
        {
            List<Vector3> spawnPosListEdit = new List<Vector3>();
            List<EditCube> cubes = GetAllCubes();
            foreach (EditCube eCube in cubes)
            {
                SpawnCube spawnCube = eCube.GetComponentInChildren<SpawnCube>();
                if (spawnCube != null)
                {
                    spawnPosListEdit.Add(spawnCube.transform.position);
                }
            }

            if (spawnPosListEdit.Count <= 0)
                return Vector3.zero;

            int idx = Random.Range(0, spawnPosListEdit.Count);
            return spawnPosListEdit[idx];
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