using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.System;
using Assets.Scripts.Util;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GenSampleManager : MonoBehaviourPunCallbacks
    {
        private readonly List<string> unitPartList = new List<string>
        {
            "cos",
            "hair1",
            "hair2",
            "skin",
            "hat",
            "wp_1",
            "wp_shild",
        };

        private readonly List<string> unitTypeList = new List<string>
        {
            "human_m",
            "human_s",
            "human_l",
        };

        public GameObject ground;

        [Header("- For View Test"), Space(10)]
        [Range(1, 100)]
        public int unitGenCount = 1;

        private Vector2 spawnAreaX; // min, max
        private Vector2 spawnAreaZ; // min, max
        [Range(0f, 1f)]
        public float spawnRange;

        private List<Unit> unitList = new List<Unit>();
        private Transform unitContainer;
        private bool isConnect = false;

        // Use this for initialization
        void Start()
        {
            isConnect = PhotonNetwork.IsConnected;

            if (isConnect)
            {
                SpawnPlayer();
            }
            else
            {
                unitContainer = FindObjectOfType<UnitContainer>().transform;

                SetSpawnArea();

                DestroyAllMob();
                GenerateMob();
                ResetMobPos();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isConnect)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    EndOfGame();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    ResetMobPos();
                }

                if (Input.GetKeyDown(KeyCode.D))
                {
                    DestroyAllMob();
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    GenerateMob();
                    ResetMobPos();
                }
            }
        }

        #region CONNECT_NETWORK


        public void SpawnPlayer()
        {
            var data = new List<object>();            
            data.Add(GetSelectUnitPartDict());

            PhotonNetwork.Instantiate(Path.Combine("Prefab", "Player"), new Vector3(0, 0, 0), Quaternion.identity, 0, data.ToArray());
        }

        private void EndOfGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
            }

            RoomSettings.room = PhotonNetwork.CurrentRoom;
            RoomSettings.isMater = PhotonNetwork.IsMasterClient;
            
            PhotonNetwork.LeaveRoom();
        }

        #endregion

        #region PUN_CALLBACKS

        public override void OnLeftRoom()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PhotonLobbySample");
        }

        //public override void OnDisconnected(DisconnectCause cause)
        //{
        //    PhotonNetwork.Disconnect();
        //}

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Log.Print("MaterClientSwitched");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Log.Print($"{otherPlayer.ActorNumber} Left Room");
        }

        #endregion

        #region NOT_CONNTECT_NETWORK

        private void SetSpawnArea()
        {
            Vector2 spawnArea = GetSpawnArea();
            Log.Print(spawnArea);

            spawnAreaX = new Vector2(-spawnArea.x, spawnArea.x);
            spawnAreaZ = new Vector2(-spawnArea.y, spawnArea.y);
        }

        public Vector2 GetSpawnArea()
        {
            Vector3 groundScale = ground.transform.localScale;

            // x-z 좌표계
            Vector2 spawnArea = new Vector2(Mathf.Abs(groundScale.x), Mathf.Abs(groundScale.z))* .5f * spawnRange;
            return spawnArea;
        }

        public List<Unit> GetUnitList()
        {
            return unitList;
        }

        private void GenerateMob()
        {
            GameObject pfUnit = ResourceManager.LoadAsset<GameObject>("Prefab/Unit");
            Unit unitComp;

            for (int i = 0; i < unitGenCount; i++)
            {
                if (unitList.Count <= i)
                {
                    GameObject goUnit = Instantiate(pfUnit, unitContainer);
                    unitComp = goUnit.GetComponent<Unit>();

                    if (unitComp != null)
                        unitList.Add(unitComp);
                }
                else
                {
                    unitComp = unitList[i];
                }

                if (unitComp == null)
                {
                    Log.Error($"Unit Component cannot find!");
                    return;
                }

                unitComp.Init();
                unitComp.SetSprite(GetSelectUnitPartDict());
            }
        }

        private void DestroyAllMob()
        {
            foreach (Unit unit in unitList)
            {
                Destroy(unit.gameObject);
            }
            unitList.Clear();
        }

        private void ResetMobPos()
        {
            foreach (Unit unit in unitList)
            {
                float spawnX = UnityEngine.Random.Range(spawnAreaX.x, spawnAreaX.y);
                float spawnZ = UnityEngine.Random.Range(spawnAreaZ.x, spawnAreaZ.y);

                unit.ResetSpawnPos(spawnX, spawnZ);
            }
        }

        #endregion

        private Dictionary<string, string> GetSelectUnitPartDict()
        {
            Dictionary<string, string> unitPartData = new Dictionary<string, string>();

            string unitType = "";
            for (int i = 0; i < unitTypeList.Count; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) >= .3f || i == unitTypeList.Count - 1)
                {
                    unitType = unitTypeList[i];
                    break;
                }
            }

            foreach (string unitPartName in unitPartList)
            {

                string dirPath = $"Image/Unit/{unitType}/imgs/{unitPartName}";
                string spriteName = GetRandomSprite(dirPath).name;
                if (string.IsNullOrEmpty(spriteName))
                {
                    Log.Error($"Cannot SpawnPlayer! : Cannot find sprite {dirPath}!");
                    continue;
                }

                string resPath = Path.Combine(dirPath, spriteName);

                unitPartData.Add(unitPartName, resPath);
            }

            return unitPartData;
        }

        private Sprite GetRandomSprite(string path)
        {
            Sprite resultSprite = null;

            Sprite[] loadedSprite = ResourceManager.LoadAssets<Sprite>(path);

            for (int i = 0; i < loadedSprite.Length; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) > .5f || i == loadedSprite.Length - 1)
                {
                    resultSprite = loadedSprite[i];
                    break;
                }
            }

            return resultSprite;
        }
    }
}