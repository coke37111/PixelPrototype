using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.System;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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

        private readonly float MIN_SPAWN_TIME_INDICATOR = 1f;
        private readonly float MAX_SPAWN_TIME_INDICATOR = 3f;

        private List<Unit> unitList = new List<Unit>();
        private bool isConnect = false;

        private UnitController unitCtrl;
        private readonly float initSpawnHeight = 1f;

        #region UNITY
        // Use this for initialization
        void Start()
        {
            isConnect = PhotonNetwork.IsConnected;

            SetSpawnArea();

            if (!isConnect)
            {
                DestroyAllMob();
                GenerateMob();
                ResetMobPos();
                SpawnPlayer(isConnect);
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
                    //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    //RaycastHit hit;
                    //if (Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
                    //{
                    //    MakeIndicator(hit.point);
                    //}
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    ResetMobPos();
                    ResetPlayerPos();
                }

                if (Input.GetKeyDown(KeyCode.F2))
                {
                    DestroyAllMob();
                }

                if (Input.GetKeyDown(KeyCode.F3))
                {
                    GenerateMob();
                    ResetMobPos();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 10000f, ~ignoreClickLayer))
                    {
                        MakeIndicator(hit.point);
                    }
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            GenCountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            GenCountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;
        }

        #endregion

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
                CameraController.Instance.SetOwner(unitCtrl);
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
            if (PhotonNetwork.NetworkClientState == ClientState.Leaving)
                return;

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
                }

                RoomSettings.roomName = PhotonNetwork.CurrentRoom.Name;
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

        private void OnCountdownTimerIsExpired()
        {
            SpawnPlayer(isConnect);

            if (isConnect && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(SpawnIndicator());
            }
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
            PhotonNetwork.DestroyPlayerObjects(otherPlayer);

            CheckEndOfGame();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PLAYER_LIVES))
            {                
                if(PhotonNetwork.LocalPlayer.ActorNumber == targetPlayer.ActorNumber)
                {
                    if(IsPlayerDie(targetPlayer))
                    {
                        unitCtrl.Die();
                    }
                }

                CheckEndOfGame();
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
            int startTimestamp;
            bool startTimeIsSet = GenCountdownTimer.TryGetStartTime(out startTimestamp);

            if (changedProps.ContainsKey(PLAYER_LOADED_LEVEL))
            {
                if (!CheckAllPlayerLoadedLevel())
                {
                    // not all players loaded yet. wait:
                    Log.Print("Waiting for other players...");
                }
                else
                {
                    if (!startTimeIsSet)
                    {
                        GenCountdownTimer.SetStartTime();
                    }
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
                        int viewId = (int)data[0];
                        float centerX = (float)data[1];
                        float centerY = (float)data[2];
                        float centerZ = (float)data[3];

                        if (unitCtrl == null)
                            return;

                        Knockback(new Vector3(centerX, centerY, centerZ));

                        PhotonView targetIndicator = PhotonNetwork.GetPhotonView(viewId);
                        PhotonNetwork.Destroy(targetIndicator);
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

        private void MakeIndicator(Vector3 hitPoint)
        {
            string pfPath = Path.Combine("Prefab", "Indicator");
            Vector3 initPos = new Vector3(hitPoint.x, 0f, hitPoint.z);

            Log.Print($"Indi {initPos}");

            float limitTime = 2f;
            float scaleX = 3f;
            float scaleZ = 3f;

            if (isConnect)
            {
                var data = new List<object>();
                data.Add(limitTime);
                data.Add(scaleX);
                data.Add(scaleZ);

                PhotonNetwork.InstantiateRoomObject(pfPath, initPos, Quaternion.identity, 0, data.ToArray());
            }
            else
            {
                GameObject pfIndicator = ResourceManager.LoadAsset<GameObject>(pfPath);
                GameObject goIndicator = Instantiate(pfIndicator, initPos, Quaternion.identity, unitContainer);
                Indicator indicator = goIndicator.GetComponent<Indicator>();
                indicator.Init(limitTime, scaleX, scaleZ);
                indicator.SetGenSampleManager(Knockback);
            }
        }

        public void Knockback(Vector3 center)
        {
            if (isConnect)
            {
                unitCtrl.Knockback(center.x, center.z);
            }
            else
            {   
                unitCtrl.Knockback(center.x, center.z);
                foreach (Unit unit in unitList)
                {
                    unit.Knockback(center.x, center.z);
                }
            }
        }
        private IEnumerator SpawnIndicator()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(MIN_SPAWN_TIME_INDICATOR, MAX_SPAWN_TIME_INDICATOR));

                float spawnAreaX = Random.Range(this.spawnAreaX.x, this.spawnAreaX.y);
                float spawnAreaZ = Random.Range(this.spawnAreaZ.x, this.spawnAreaZ.y);

                Vector3 spawnPoint = new Vector3(spawnAreaX, 0f, spawnAreaZ);
                MakeIndicator(spawnPoint);
            }
        }

        private IEnumerator EndOfGame(string winner, int score)
        {
            yield return null;
            //float timer = 3.0f;

            //while (timer > 0.0f)
            //{
            //    InfoText.text = string.Format("Player {0} won with {1} points.\n\n\nReturning to login screen in {2} seconds.", winner, score, timer.ToString("n2"));

            //    yield return new WaitForEndOfFrame();

            //    timer -= Time.deltaTime;
            //}

            //PhotonNetwork.LeaveRoom();
        }
    }
}