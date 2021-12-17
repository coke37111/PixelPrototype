using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.System;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class GenSampleManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public enum EventCodeType
        {
            Move,
            Knockback,
        }

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
            "char_st_01",
        };

        public const string PLAYER_LIVES = "GenPlayerLives";
        private const string PLAYER_LOADED_LEVEL = "GenPlayerLoadedLevel";

        public GameObject collGround;
        public LayerMask ignoreClickLayer;

        [Header("- For Gen Test"), Space(10)]
        public Transform unitContainer;
        [Range(1, 100)]
        public int unitGenCount = 1;

        private Vector2 spawnAreaX; // min, max
        private Vector2 spawnAreaZ; // min, max
        [Range(0f, 1f)]
        public float spawnRange;

        private List<Unit> unitList = new List<Unit>();
        private bool isConnect = false;

        private UnitController unitCtrl;
        private readonly float initSpawnHeight = 1f;

        // Use this for initialization
        void Start()
        {
            isConnect = PhotonNetwork.IsConnected;

            if (!isConnect)
            {
                SetSpawnArea();
                DestroyAllMob();
                GenerateMob();
                ResetMobPos();
            }
            else
            {
                Hashtable props = new Hashtable
                    {
                        {PLAYER_LOADED_LEVEL, true},
                        { PLAYER_LIVES, 1 }
                    };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }

            SpawnPlayer(isConnect);
        }

        // Update is called once per frame
        void Update()
        {
            if (isConnect)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PhotonNetwork.LeaveRoom();
                }

                if (PhotonNetwork.IsMasterClient && Input.GetMouseButtonUp(0))
                {
                    KnockBack();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    ResetMobPos();
                    ResetPlayerPos();
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

                if (Input.GetMouseButtonUp(0))
                {
                    KnockBack();
                }
            }
        }

        #region CONNECT_NETWORK


        public void SpawnPlayer(bool isConnect)
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);
            Dictionary<string, string> selectUnitParts = GetSelectUnitPartDict();

            if (isConnect)
            {
                var data = new List<object>();
                data.Add(selectUnitParts);

                GameObject netGoPlayer = PhotonNetwork.Instantiate(Path.Combine("Prefab", "Player"), initPos, Quaternion.identity, 0, data.ToArray());
                unitCtrl = netGoPlayer.GetComponent<UnitController>();
            }
            else{
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>("Prefab/Player");
                GameObject goPlayer = Instantiate(pfPlayer, initPos, Quaternion.identity, unitContainer);
                unitCtrl = goPlayer.GetComponent<UnitController>();
                unitCtrl.SetSprite(selectUnitParts);
                unitCtrl.Init(false);
            }
        }

        private void CheckEndOfGame()
        {
            bool allDestroyed = true;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (!IsPlayerDie(p))
                {
                    allDestroyed = false;
                    break;
                }
            }

            if (allDestroyed)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    StopAllCoroutines();
                    if (PhotonNetwork.PlayerListOthers.Length > 0)
                        return;
                }

                RoomSettings.room = PhotonNetwork.CurrentRoom;
                RoomSettings.isMaster = PhotonNetwork.IsMasterClient;

                PhotonNetwork.LeaveRoom();
            }
        }

        private bool IsPlayerDie(Player player)
        {
            object lives;
            if (player.CustomProperties.TryGetValue(PLAYER_LIVES, out lives))
            {
                return (int)lives <= 0;
            }

            return false;
        }

        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(PLAYER_LOADED_LEVEL, out playerLoadedLevel))
                {
                    if ((bool)playerLoadedLevel)
                    {
                        continue;
                    }
                }

                return false;
            }

            return true;
        }

        #endregion

        #region PUN_CALLBACKS

        public override void OnLeftRoom()
        {            
            UnityEngine.SceneManagement.SceneManager.LoadScene("PhotonLobbySample");
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Log.Print("MaterClientSwitched");
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Log.Print($"Player {otherPlayer.ActorNumber} Left Room");

            CheckEndOfGame();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PLAYER_LIVES))
            {                
                CheckEndOfGame();

                if(PhotonNetwork.LocalPlayer.ActorNumber == targetPlayer.ActorNumber)
                {
                    if(IsPlayerDie(targetPlayer))
                        unitCtrl.Die();
                }
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (changedProps.ContainsKey(PLAYER_LOADED_LEVEL))
            {
                if (!CheckAllPlayerLoadedLevel())
                {
                    // not all players loaded yet. wait:
                    Log.Print("Waiting for other players...");
                }
                else
                {
                    Log.Print("Loaded All Players Complete!");
                }
            }
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;

            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[] : null;
            switch (eventCodeType)
            {
                case EventCodeType.Knockback:
                    {
                        float hitX = (float)data[0];
                        float hitZ = (float)data[1];

                        if (unitCtrl == null)
                            return;

                        unitCtrl.Knockback(hitX, hitZ);
                        break;
                    }
            }
        }

        #endregion

        #region NOT_CONNTECT_NETWORK

        private void SetSpawnArea()
        {   
            Vector3 spawnArea = GetSpawnArea();

            Vector3 center = collGround.transform.localPosition;
            spawnAreaX = new Vector2(-spawnArea.x, spawnArea.x) + Vector2.one * center.x;
            spawnAreaZ = new Vector2(-spawnArea.z, spawnArea.z) + Vector2.one * center.z;
        }

        public Vector3 GetSpawnArea()
        {
            Vector3 groundScale = collGround.transform.localScale;
            
            // x-z 좌표계
            Vector3 spawnArea = new Vector3(Mathf.Abs(groundScale.x), Mathf.Abs(groundScale.y), Mathf.Abs(groundScale.z)) * .5f * spawnRange;
            return spawnArea;
        }

        private void GenerateMob()
        {
            GameObject pfUnit = ResourceManager.LoadAsset<GameObject>("Prefab/AIUnit");
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

                unit.ResetSpawnPos(spawnX, initSpawnHeight, spawnZ);
            }
        }

        private void ResetPlayerPos()
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);
            unitCtrl.ResetSpawnPos(initPos);
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
                Sprite selectSprite = GetRandomSprite(dirPath);
                if (selectSprite == null)
                {
                    Log.Error($"Cannot SpawnPlayer! : Cannot find sprite {dirPath}!");
                    continue;
                }
                string spriteName = GetRandomSprite(dirPath).name;

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

        private void KnockBack()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
            {
                if (isConnect)
                {
                    List<object> content = new List<object>() { hit.point.x, hit.point.z };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    SendOptions sendOptions = new SendOptions { Reliability = true };
                    PhotonNetwork.RaiseEvent((byte)EventCodeType.Knockback, content.ToArray(), raiseEventOptions, sendOptions);
                }
                else
                {
                    unitCtrl.Knockback(hit.point.x, hit.point.z);
                    foreach(Unit unit in unitList)
                    {
                        unit.Knockback(hit.point.x, hit.point.z);
                    }
                }                
            }
        }
    }
}