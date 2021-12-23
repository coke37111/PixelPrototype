using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.Settings.PlayerSettings;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Managers
{
    public class GenSampleManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public enum GenSampleState
        {
            Init,
            Idle,
            PlayNetwork,
            PlayAI,
            Clear,
            End,
        }
        private GenSampleState genSampleState;

        public TMPro.TextMeshProUGUI infoText;
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

        private readonly float MIN_SPAWN_TIME_INDICATOR = 1f;
        private readonly float MAX_SPAWN_TIME_INDICATOR = 3f;

        private List<Unit> unitList = new List<Unit>();

        private UnitController unitCtrl;
        private readonly float initSpawnHeight = 1f;

        private static List<UnitController> unitListenerList = new List<UnitController>();

        #region UNITY
        // Use this for initialization
        void Start()
        {
            // TODO : 훨씬 더 앞 프로세스에 있어야하긴 함
            if(PhotonNetwork.IsConnected)
                PlayerSettings.ConnectNetwork();

            SetGenSampleState(GenSampleState.Init);
        }

        // Update is called once per frame
        void Update()
        {
            switch (genSampleState)
            {
                case GenSampleState.Idle: return;
                case GenSampleState.Init:
                    {
                        InitProc();
                        break;
                    }
                case GenSampleState.PlayNetwork:
                    {
                        PlayProcNetwork();
                        break;
                    }
                case GenSampleState.PlayAI:
                    {
                        PlayProcAI();
                        break;
                    }
                case GenSampleState.Clear:
                    {
                        // TODO mob의 die 애니메이션에서 call 대기
                        break;
                    }
                case GenSampleState.End:
                    {
                        StartCoroutine(EndOfGame());

                        SetGenSampleState(GenSampleState.Idle);
                        break;
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
            if (changedProps.ContainsKey(PLAYER_DIE) ||
                changedProps.ContainsKey(FAIL_GAME))
            {           
                CheckEndOfGame();
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (PhotonNetwork.LocalPlayer.ActorNumber != targetPlayer.ActorNumber)
                return;

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

                        if (PhotonNetwork.IsMasterClient)
                        {
                            PhotonView targetIndicator = PhotonNetwork.GetPhotonView(viewId);
                            PhotonNetwork.Destroy(targetIndicator);
                        }
                        break;
                    }
                case EventCodeType.MobDie:
                    {
                        Hashtable roomProps = new Hashtable();
                        roomProps[RoomSettings.GAME_END] = true;
                        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                        if (PhotonNetwork.IsMasterClient)
                        {
                            StopAllCoroutines();
                        }
                        SetGenSampleState(GenSampleState.Clear);
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
                case EventCodeType.Clear:
                    {
                        SetGenSampleState(GenSampleState.End);
                        break;
                    }
            }
        }

        #endregion

        #region CONNECT_NETWORK


        public void SpawnPlayer()
        {
            Vector3 initPos = new Vector3(0, initSpawnHeight, -1f);
            Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict();

            if (PlayerSettings.IsConnectNetwork())
            {
                var data = new List<object>();
                data.Add(selectUnitParts);

                GameObject netGoPlayer = PhotonNetwork.Instantiate(Path.Combine("Prefab", "Player"), initPos, Quaternion.identity, 0, data.ToArray());
                unitCtrl = netGoPlayer.GetComponent<UnitController>();
                CameraController.Instance.SetOwner(unitCtrl);
            }
            else
            {
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
                SetGenSampleState(GenSampleState.End);
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
            object isDie;
            if (player.CustomProperties.TryGetValue(PLAYER_DIE, out isDie))
            {
                return (bool)isDie;
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
            Hashtable roomProps = new Hashtable();
            roomProps[RoomSettings.GAME_END] = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

            SpawnPlayer();

            if (PlayerSettings.IsConnectNetwork() && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(SpawnIndicator());
            }

            SetGenSampleState(GenSampleState.PlayNetwork);
        }

        private IEnumerator EndOfGame()
        {
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

        #endregion

        #region NOT_CONNTECT_NETWORK

        private void InitSpawnArea()
        {   
            Vector3 groundCenter = collGround.transform.localPosition;
            Vector3 groundScale = collGround.transform.localScale;

            Vector3 spawnRadius = groundScale * .5f * spawnRange;

            spawnAreaX = new Vector2(-spawnRadius.x, spawnRadius.x) + Vector2.one * groundCenter.x;
            spawnAreaZ = new Vector2(-spawnRadius.z, spawnRadius.z) + Vector2.one * groundCenter.z;
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
                unitComp.SetSprite(UnitSettings.GetSelectUnitPartDict());
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

        private void MakeIndicator(Vector3 hitPoint)
        {
            string pfPath = Path.Combine("Prefab", "Indicator");
            Vector3 initPos = new Vector3(hitPoint.x, 0f, hitPoint.z);

            float limitTime = 2f;
            float scaleX = 3f;
            float scaleZ = 3f;

            if (PlayerSettings.IsConnectNetwork())
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
            if (PlayerSettings.IsConnectNetwork())
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

        private void GenerateMob()
        {
            Vector3 initPos = new Vector3(0f, 1f, 0f);
            string pfMobPath = "Prefab/Mob";

            if (PlayerSettings.IsConnectNetwork())
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

        private void SetGenSampleState(GenSampleState state)
        {
            this.genSampleState = state;
        }
        private void PlayProcNetwork()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.LeaveRoom();
            }

            if (curLimitTime >= limitTime)
            {
                Hashtable roomProps = new Hashtable();
                roomProps[RoomSettings.GAME_END] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                Hashtable props = new Hashtable
                    {
                        { FAIL_GAME, true },
                    };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                SetGenSampleState(GenSampleState.Idle);
            }
            else
            {
                curLimitTime += Time.deltaTime;
            }

            SetTextLimitTime();
        }

        private void SetTextLimitTime()
        {
            var remain = limitTime - curLimitTime;

            if (remain > 3)
                infoText.text = $"{(limitTime - curLimitTime):n0}";
            else
                infoText.text = $"{(limitTime - curLimitTime):f1}";
        }

        private void PlayProcAI()
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

        private void InitProc()
        {
            InitSpawnArea();

            if (!PlayerSettings.IsConnectNetwork())
            {
                DestroyAllAIUnit();
                GenerateAIUnit();
                ResetAIUnitPos();

                SpawnPlayer();
                GenerateMob();

                SetGenSampleState(GenSampleState.PlayAI);
            }
            else
            {
                curLimitTime = 0f;

                Hashtable roomProps = new Hashtable();
                roomProps[RoomSettings.GAME_END] = true;
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                Hashtable props = new Hashtable();
                props.Add(PLAYER_LOADED_LEVEL, true);
                props.Add(PLAYER_DIE, false);
                props.Add(FAIL_GAME, false);
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                SetGenSampleState(GenSampleState.Idle);
            }
        }
    }
}