using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
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
    /// <summary>
    /// Photon Network 연결된 상태에서의 게임 처리 로직
    /// GenSampleScene 자체 실행 로직은 GenSampleAIManager에서 처리
    /// </summary>
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
        public GameObject collGround; // 각종 오브젝트의 spawn 기준영역

        private Vector2 spawnAreaX; // min, max
        private Vector2 spawnAreaZ; // min, max
        [Range(0f, 1f)]
        public float spawnRange;

        private readonly Vector3 initSpawnPos = new Vector3(0, 1f, -1f);
        private float curLimitTime;

        private GenSampleAIManager aiManager;
        private GameSettingSO gameSetting;
        private IndicatorSettingSO indicatorSetting;

        #region UNITY
        // Use this for initialization
        void Start()
        {
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
                        aiManager.Proc();
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

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Log.Print($"Player {otherPlayer.ActorNumber} Left Room");

            PhotonNetwork.DestroyPlayerObjects(otherPlayer);
            CheckEndOfGame();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                StartCoroutine(SpawnIndicator());
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PLAYER_DIE) ||
                changedProps.ContainsKey(FAIL_GAME))
            {           
                CheckEndOfGame();
            }

            if(PhotonNetwork.IsMasterClient && 
                PhotonNetwork.LocalPlayer.ActorNumber == targetPlayer.ActorNumber)
            {
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
                        Log.Print("Players Loaded Complete!");

                        if (!startTimeIsSet)
                        {
                            GenCountdownTimer.SetStartTime();
                            GenerateMob();
                        }
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
                case EventCodeType.MobDie:
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            StopAllCoroutines();
                        }
                        SetGenSampleState(GenSampleState.Clear);
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

        private void SetGenSampleState(GenSampleState state)
        {
            this.genSampleState = state;
        }

        private void InitProc()
        {
            InitSpawnArea();

            gameSetting = ResourceManager.LoadAsset<GameSettingSO>(GameSettingSO.path);
            indicatorSetting = ResourceManager.LoadAsset<IndicatorSettingSO>(IndicatorSettingSO.path);

            if (!PlayerSettings.IsConnectNetwork())
            {
                GameObject pfAIManager = ResourceManager.LoadAsset<GameObject>("Prefab/Manager/GenSampleAIManager");
                if (pfAIManager == null)
                {
                    SetGenSampleState(GenSampleState.Idle);
                    return;
                }

                GameObject goAIManager = Instantiate(pfAIManager, transform);
                aiManager = goAIManager.GetComponent<GenSampleAIManager>();
                aiManager.Build(this);

                SetGenSampleState(GenSampleState.PlayAI);
            }
            else
            {
                curLimitTime = 0f;

                Hashtable props = new Hashtable();
                props.Add(PLAYER_LOADED_LEVEL, true);
                props.Add(PLAYER_DIE, false);
                props.Add(FAIL_GAME, false);
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                SetGenSampleState(GenSampleState.Idle);
            }
        }

        private void PlayProcNetwork()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.LeaveRoom();
            }

            if (curLimitTime >= gameSetting.limitTime)
            {
                Hashtable props = new Hashtable
                    {
                        { FAIL_GAME, true },
                    };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                PhotonEventManager.RaiseEvent(EventCodeType.Fail, ReceiverGroup.All);

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
            var remain = gameSetting.limitTime - curLimitTime;

            if (remain > 3)
                infoText.text = $"{(gameSetting.limitTime - curLimitTime):n0}";
            else
                infoText.text = $"{(gameSetting.limitTime - curLimitTime):f1}";
        }

        private void SpawnPlayer()
        {
            PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
            Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict(playerUnitSetting.GetUnitType());

            var data = new List<object>();
            data.Add(selectUnitParts);

            GameObject netGoPlayer = PhotonNetwork.Instantiate(Path.Combine("Prefab", "Unit/NetworkPlayer"), initSpawnPos, Quaternion.identity, 0, data.ToArray());
            UnitNetworkPlayer unit = netGoPlayer.GetComponent<UnitNetworkPlayer>();
            CameraController.Instance.SetOwner(unit);
        }

        private void CheckEndOfGame()
        {
            if (PhotonNetwork.NetworkClientState == ClientState.Leaving)
                return;

            if (genSampleState == GenSampleState.Clear)
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
                if (!IsPlayerGameFail(p))
                {
                    failClear = false;
                    break;
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

        private bool IsPlayerGameFail(Player player)
        {
            object playerGameFail;
            if (player.CustomProperties.TryGetValue(FAIL_GAME, out playerGameFail))
            {
                return (bool)playerGameFail;
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
            SpawnPlayer();

            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(SpawnIndicator());
            }

            SetGenSampleState(GenSampleState.PlayNetwork);
        }

        private IEnumerator EndOfGame()
        {
            yield return null;
            float timer = gameSetting.endDelay;

            while (timer > 0.0f)
            {
                infoText.text = string.Format("Leave Room in {0} seconds", timer.ToString("n0"));
                yield return new WaitForEndOfFrame();

                timer -= Time.deltaTime;
            }

            LeaveRoom();
        }

        private void InitSpawnArea()
        {
            Vector3 groundCenter = collGround.transform.localPosition;
            Vector3 groundScale = collGround.transform.localScale;

            Vector3 spawnRadius = groundScale * .5f * spawnRange;

            spawnAreaX = new Vector2(-spawnRadius.x, spawnRadius.x) + Vector2.one * groundCenter.x;
            spawnAreaZ = new Vector2(-spawnRadius.z, spawnRadius.z) + Vector2.one * groundCenter.z;
        }

        public Vector2 GetSpawnAreaX()
        {
            return spawnAreaX;
        }

        public Vector2 GetSpawnAreaZ()
        {
            return spawnAreaZ;
        }

        public void MakeIndicator(Vector3 hitPoint)
        {
            string pfPath = Path.Combine("Prefab", "Indicator");
            Vector3 initPos = new Vector3(hitPoint.x, 0f, hitPoint.z);

            float limitTime = 2f;
            float scaleX = 3f;
            float scaleZ = 3f;

            var data = new List<object>();
            data.Add(limitTime);
            data.Add(scaleX);
            data.Add(scaleZ);

            PhotonNetwork.InstantiateRoomObject(pfPath, initPos, Quaternion.identity, 0, data.ToArray());
        }

        private IEnumerator SpawnIndicator()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(indicatorSetting.minSpawnTime, indicatorSetting.maxSpawnTime));

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

            var data = new List<object>();
            data.Add(PhotonNetwork.PlayerList.Length);

            PhotonNetwork.InstantiateRoomObject(pfMobPath, initPos, Quaternion.identity, 0, data.ToArray());
        }
    }
}