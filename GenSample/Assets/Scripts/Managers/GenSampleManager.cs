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
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Managers
{
    public class GenSampleManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public enum EventCodeType
        {
            Move,
            Knockback,
            MobAttackBy,
            MobDie,
            MakeAtkEff,
            MobRegenHp,
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
            "char_st_01_human_m",
            "char_st_01_human_s",
        };

        public const string PLAYER_LIVES = "GenPlayerLives";
        private const string PLAYER_LOADED_LEVEL = "GenPlayerLoadedLevel";
        public const string MOB_DIE = "GenMobDie";
        private const string FAIL_GAME = "GenGameFail";

        public Text infoText;
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

        public float limitTime = 30f;
        private float curLimitTime;
        private bool isGameEnd;

        private readonly float MIN_SPAWN_TIME_INDICATOR = 1f;
        private readonly float MAX_SPAWN_TIME_INDICATOR = 3f;

        private List<Unit> unitList = new List<Unit>();
        private bool isConnect = false;

        private UnitController unitCtrl;
        private readonly float initSpawnHeight = 1f;

        private static List<UnitController> unitListenerList = new List<UnitController>();

        #region UNITY
        // Use this for initialization
        void Start()
        {
            isConnect = PhotonNetwork.IsConnected;

            SetSpawnArea();

            if (!isConnect)
            {
                DestroyAllAIUnit();
                GenerateAIUnit();
                ResetAIUnitPos();
                SpawnPlayer(isConnect);
                GenerateMob();
            }
            else
            {
                curLimitTime = 0f;
                isGameEnd = true;

                Hashtable props = new Hashtable
                    {
                        {PLAYER_LOADED_LEVEL, true},
                        { PLAYER_LIVES, 1 },
                    { FAIL_GAME, false }
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

                if (!isGameEnd)
                {
                    if (curLimitTime >= limitTime)
                    {
                        isGameEnd = true;

                        Hashtable props = new Hashtable
                    {
                        { FAIL_GAME, true },
                    };
                        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                    }
                    else
                    {
                        curLimitTime += Time.deltaTime;
                    }
                    infoText.text = $"Remain {(limitTime - curLimitTime):n0} seconds";
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    ResetAIUnitPos();
                    ResetPlayerPos();
                }

                if (Input.GetKeyDown(KeyCode.F2))
                {
                    DestroyAllAIUnit();
                }

                if (Input.GetKeyDown(KeyCode.F3))
                {
                    GenerateAIUnit();
                    ResetAIUnitPos();
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

            bool failClear = true;
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object playerGameFail;
                if (p.CustomProperties.TryGetValue(FAIL_GAME, out playerGameFail))
                {
                    if (!(bool)playerGameFail)
                    {
                        failClear = false;
                        break;
                    }
                }
            }

            if (allDestroyed || failClear)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    StopAllCoroutines();
                }

                StartCoroutine(EndOfGame());
            }
        }

        private void LeaveRoom()
        {
            RoomSettings.roomName = PhotonNetwork.CurrentRoom.Name;
            RoomSettings.isMaster = PhotonNetwork.IsMasterClient;

            PhotonNetwork.LeaveRoom();
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
            isGameEnd = false;
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

            if (changedProps.ContainsKey(FAIL_GAME))
            {
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

                        GenerateMob();
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
                case EventCodeType.MobDie:
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            StopAllCoroutines();
                        }

                        StartCoroutine(EndOfGame());
                        break;
                    }
                case EventCodeType.MakeAtkEff:
                    {
                        int senderViewId = (int)data[0];
                        string effColor = data[1].ToString();

                        foreach(UnitController unit in unitListenerList)
                        {
                            if(unit.photonView.ViewID == senderViewId)
                            {
                                unit.SetAtkEffColor(effColor);
                                unit.MakeAtkEffect();
                                break;
                            }
                        }
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

        private void GenerateAIUnit()
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

        private void DestroyAllAIUnit()
        {
            foreach (Unit unit in unitList)
            {
                Destroy(unit.gameObject);
            }
            unitList.Clear();
        }

        private void ResetAIUnitPos()
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
                indicator.RegisterKnockbackListener(Knockback);
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

        private IEnumerator EndOfGame()
        {
            isGameEnd = true;

            yield return null;
            float timer = 3.0f;

            while (timer > 0.0f)
            {
                infoText.text = string.Format("Leave Room in {0} seconds", timer.ToString("n0"));
                yield return new WaitForEndOfFrame();

                timer -= Time.deltaTime;
            }

            LeaveRoom();
        }

        private void GenerateMob()
        {
            Vector3 initPos = new Vector3(0f, 1f, 0f);
            string pfMobPath = "Prefab/Mob";

            if (isConnect)
            {
                var data = new List<object>();
                data.Add(PhotonNetwork.PlayerList.Length);

                PhotonNetwork.InstantiateRoomObject(pfMobPath, initPos, Quaternion.identity, 0, data.ToArray());
            }
            else
            {
                GameObject pfMob = ResourceManager.LoadAsset<GameObject>(pfMobPath);
                GameObject goMob = Instantiate(pfMob, initPos, Quaternion.identity);
                goMob.GetComponent<MobController>().Init();
            }
        }

        public static void RegisterUnit(UnitController unit)
        {
            unitListenerList.Add(unit);
        }

        public static void UnRegisterUnit(UnitController unit)
        {
            unitListenerList.Remove(unit);
        }
    }
}