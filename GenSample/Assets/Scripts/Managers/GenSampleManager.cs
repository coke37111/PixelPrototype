using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;
using static Assets.Scripts.Settings.RoomSettings;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Managers
{
    public class GenSampleManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public ROOM_TYPE curRoomType = ROOM_TYPE.Raid;

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
       
        private float curLimitTime;

        private GameSettingSO gameSetting;
        private IndicatorSettingSO indicatorSetting;

        public SandboxMapDataSO mapData;
        private CubeContainer cubeContainer;

        public Vector3 playerScale = new Vector3(1, 1, 1);

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
            if (curRoomType == ROOM_TYPE.Raid && PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                StartCoroutine(SpawnIndicator());
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PLAYER_DIE))
            {           
                CheckEndOfGame();
            }

            if (changedProps.ContainsKey(FAIL_GAME))
            {
                if(IsPlayerGameFail(targetPlayer))
                    CheckEndOfGame(true);
            }
            
            if(PhotonNetwork.IsMasterClient)
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

                            if(PhotonNetwork.MasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                            {
                                GenerateMob();
                                cubeContainer.GenerateNormalCube();
                            }
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

            cubeContainer = FindObjectOfType<CubeContainer>();
            GenerateMap();

            if (!PlayerSettings.IsConnectNetwork())
            {
                RoomSettings.roomType = curRoomType;

                SpawnPlayer();
                cubeContainer.GenerateNormalCube();

                GenerateMob();

                if (curRoomType == ROOM_TYPE.Raid)
                {
                    StartCoroutine(SpawnIndicator());
                }

                SetGenSampleState(GenSampleState.PlayAI);
            }
            else
            {
                curRoomType = RoomSettings.roomType;

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

            if (curLimitTime >= gameSetting.GetLimitTime(curRoomType))
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

            if (player != null && player.transform.position.y <= -5f)
            {
                player.FallDie();
            }

            SetTextLimitTime();
        }

        private void SetTextLimitTime()
        {
            float limitTime = gameSetting.GetLimitTime(curRoomType);
            var remain = limitTime - curLimitTime;

            if (remain > 3)
                infoText.text = $"{(limitTime - curLimitTime):n0}";
            else
                infoText.text = $"{(limitTime - curLimitTime):f1}";
        }

        private PlayerController player;
        private void SpawnPlayer()
        {
            Vector3 spawnPosTo3 = cubeContainer.GetRandomSpawnPos();

            //// Set SpawnPos
            //Vector3 orgSpawnPos = initSpawnPos;
            //if (curRoomType == ROOM_TYPE.Pvp)
            //{
            //    int teamNum = -1;
            //    if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PLAYER_TEAM, out object playerTeam))
            //    {
            //        teamNum = (int)playerTeam;
            //    }
            //    data.Add(teamNum);

            //    if (teamNum < 0)
            //    {
            //        float spawnAreaX = UnityEngine.Random.Range(this.spawnAreaX.x, this.spawnAreaX.y);
            //        float spawnAreaZ = UnityEngine.Random.Range(this.spawnAreaZ.x, this.spawnAreaZ.y);

            //        Vector3 spawnPoint = new Vector3(spawnAreaX, 0f, spawnAreaZ);
            //        orgSpawnPos += spawnPoint;
            //    }
            //    else
            //    {
            //        orgSpawnPos = teamNum == 0 ? orgSpawnPos + Vector3.left : orgSpawnPos + Vector3.right;
            //    }
            //}

            PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);

            if (PlayerSettings.IsConnectNetwork())
            {
                var data = new List<object>();
                data.Add(playerUnitSetting.name);
                data.Add(playerUnitSetting.GetSpinePath());
                data.Add(Random.Range(0, 2));

                GameObject goPlayer = PhotonNetwork.Instantiate(PrefabPath.PlayerPath, spawnPosTo3, Quaternion.identity, 0, data.ToArray());
                player = goPlayer.GetComponent<PlayerController>();

                CameraController.Instance.SetOwner(player);
            }
            else
            {
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
                GameObject goPlayer = Instantiate(pfPlayer, spawnPosTo3, Quaternion.identity, null);
                PlayerController player = goPlayer.GetComponent<PlayerController>();
                player.SetPlayerUnitSetting(playerUnitSetting.name);
                player.Init();
                player.MakeSpine(playerUnitSetting.GetSpinePath());
                player.SetAtkType(Random.Range(0, 2));
                player.SetControllable(true);

                CameraController.Instance.SetOwner(player);
            }      
        }

        private void CheckEndOfGame(bool isForce = false)
        {
            if (PhotonNetwork.NetworkClientState == ClientState.Leaving)
                return;

            if (genSampleState != GenSampleState.PlayNetwork && !isForce)
                return;

            bool allDestroyed = true;
            if (curRoomType == ROOM_TYPE.Raid)
            {
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (!IsPlayerDie(p))
                    {
                        allDestroyed = false;
                        break;
                    }
                }
            }
            else if (curRoomType == ROOM_TYPE.Pvp)
            {
                allDestroyed = false;

                Dictionary<int, int> teamCntDict = new Dictionary<int, int>(); // teamNum, cnt
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (!IsPlayerDie(p))
                    {
                        if (p.CustomProperties.TryGetValue(PLAYER_TEAM, out object playerTeamNum))
                        {
                            int teamNum = (int)playerTeamNum;
                            if (!teamCntDict.ContainsKey(teamNum))
                            {
                                teamCntDict.Add(teamNum, 0);
                            }
                            teamCntDict[teamNum]++;
                        }
                    }
                }

                if(teamCntDict.Keys.ToList().Count <= 1)
                {
                    if(teamCntDict.Keys.First() == -1)
                    {
                        allDestroyed = teamCntDict[-1] <= 1 ? true : false;
                    }
                    else
                    {
                        allDestroyed = true;
                    }
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

            Hashtable props = new Hashtable();
            props.Add(PLAYER_LOADED_LEVEL, false);            
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

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
                        
            if (curRoomType == ROOM_TYPE.Raid && PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(SpawnIndicator());
            }

            SetGenSampleState(GenSampleState.PlayNetwork);
            if (curRoomType == ROOM_TYPE.Pvp)
            {
                CheckEndOfGame();
            }
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

            Vector3 spawnRadius = groundScale * 0.5f * spawnRange;

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
            Vector3 initPos = new Vector3(hitPoint.x, 0.5f, hitPoint.z);

            float limitTime = 2f;
            float scaleX = 5f;
            float scaleZ = 5f;

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
                GameObject goIndicator = Instantiate(pfIndicator, initPos, Quaternion.identity, null);
                Indicator indicator = goIndicator.GetComponent<Indicator>();
                indicator.Init(limitTime, scaleX, scaleZ);
                //indicator.RegisterKnockbackListener(player.Knockback);
            }
        }

        private IEnumerator SpawnIndicator()
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(indicatorSetting.minSpawnTime, indicatorSetting.maxSpawnTime));

                float spawnAreaX = UnityEngine.Random.Range(this.spawnAreaX.x, this.spawnAreaX.y);
                float spawnAreaZ = UnityEngine.Random.Range(this.spawnAreaZ.x, this.spawnAreaZ.y);

                Vector3 spawnPoint = new Vector3(spawnAreaX, 0f, spawnAreaZ);
                MakeIndicator(spawnPoint);
            }
        }

        private void GenerateMob()
        {
            if (curRoomType != ROOM_TYPE.Raid)
                return;

            Vector3 initPos = new Vector3(0f, 1.7f, 0f);
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
                MobController mobCtrl = goMob.GetComponent<MobController>();
                mobCtrl.Init();
            }
        }

        private void GenerateMap()
        {
            if (cubeContainer == null)
            {
                Log.Error($"CubeContainer가 씬에 존재하지 않습니다");
            }
            else
            {
                if (mapData != null)
                    cubeContainer.GenerateCubes(mapData);
                else
                    Log.Error($"MapData를 세팅해주세요!");
            }
        }
    }
}