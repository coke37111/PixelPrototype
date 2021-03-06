using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.Camera;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;
using PunHashtable = ExitGames.Client.Photon.Hashtable;
using PunPlayer = Photon.Realtime.Player;

namespace Assets.Scripts.Feature.Main
{
    public class GameModeManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        #region ENUM

        public enum GameState
        {
            Init,
            ConnectNet,
            Play,
            Clear,
            Fail,
            End,
        }

        public enum ProcState
        {
            Lock,
            Proc,
        }

        #endregion

        private GameState gameState;
        private ProcState procState;

        public GameModeSettingSO setting;
        private GameSettingSO gameSetting;
        public TMPro.TextMeshProUGUI textTime;

        private CubeContainer cubeContainer;
        private CameraViewController camViewController;

        private PlayerController localPlayer;
        private List<PlayerController> dummyPlayerList;

        private float curTime;
        private float curLeaveTime;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            SetGameState(GameState.Init);
            SetProcState(ProcState.Proc);            
        }

        // Update is called once per frame
        void Update()
        {
            switch (gameState)
            {
                case GameState.Init when procState == ProcState.Proc:
                    {
                        SetProcState(ProcState.Lock);

                        // Setting Camera
                        UnityEngine.Camera mainCam = UnityEngine.Camera.main;
                        if(mainCam == null)
                        {
                            Log.Error($"Scene에 MainCamera가 존재하지 않습니다");
                            return;
                        }

                        camViewController = mainCam.GetComponent<CameraViewController>();
                        if(camViewController == null)
                        {
                            Log.Error($"MainCamera에 CameraViewController가 붙어있지 않아 동적으로 붙입니다.");
                            mainCam.gameObject.AddComponent<CameraViewController>();
                            camViewController = mainCam.GetComponent<CameraViewController>();
                        }
                        camViewController.Init(setting.cameraViewSetting);

                        // Setting Map
                        cubeContainer = FindObjectOfType<CubeContainer>();
                        if(cubeContainer == null)
                        {
                            Log.Error($"Scene에 CubeContainer가 존재하지 않아 동적으로 생성합니다.");

                            GameObject go = new GameObject("CubeContainer", typeof(CubeContainer));
                            cubeContainer = go.GetComponent<CubeContainer>();
                        }
                        cubeContainer.GenerateCubes(setting.mapData);

                        // Set Value
                        textTime.text = "";
                        curTime = 0f;
                        curLeaveTime = 0f;

                        gameSetting = ResourceManager.LoadAsset<GameSettingSO>(GameSettingSO.path);

                        SetGameState(GameState.ConnectNet);
                        SetProcState(ProcState.Proc);
                        break;
                    }
                case GameState.Init when procState == ProcState.Lock: return;
                case GameState.ConnectNet when procState == ProcState.Proc:
                    {
                        SetProcState(ProcState.Lock);

                        if (PlayerSettings.IsConnectNetwork())
                        {
                            PunHashtable props = new PunHashtable();
                            props.Add(PlayerSettings.PLAYER_LOADED_LEVEL, true);

                            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                        }
                        else
                        {
                            GenerateObjectByMaster();
                            SpawnLocalPlayer();

                            SetGameState(GameState.Play);
                            SetProcState(ProcState.Proc);
                        }
                        break;
                    }
                case GameState.ConnectNet when procState == ProcState.Lock: return;
                case GameState.Play when procState == ProcState.Proc:
                    {
                        if(setting.limitTime > 0)
                        {
                            if (curTime >= setting.limitTime)
                            {
                                curTime = 0f;
                                FailGame();
                            }
                            else
                            {
                                curTime += Time.deltaTime;
                                SetTextTime();
                            }
                        }

                        if (setting.canFallDie)
                        {
                            if (localPlayer == null)
                                return;

                            if(localPlayer.transform.position.y <= setting.dieHeight)
                            {
                                localPlayer.FallDie();

                                if (!PlayerSettings.IsConnectNetwork())
                                {
                                    SetGameState(GameState.Fail);
                                    SetProcState(ProcState.Proc);
                                }
                            }
                        }
                        break;
                    }
                case GameState.Play when procState == ProcState.Lock: return;
                case GameState.Clear when procState == ProcState.Proc:
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            StopAllCoroutines();
                        }

                        SetGameState(GameState.End);
                        SetProcState(ProcState.Proc);
                        break;
                    }
                case GameState.Fail when procState == ProcState.Proc:
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            StopAllCoroutines();
                        }

                        SetGameState(GameState.End);
                        SetProcState(ProcState.Proc);
                        break;
                    }
                case GameState.End when procState == ProcState.Proc:
                    {
                        if(curLeaveTime >= gameSetting.endDelay)
                        {
                            LeaveRoom();

                            SetProcState(ProcState.Lock);
                        }
                        else
                        {
                            curLeaveTime += Time.deltaTime;

                            float remain = gameSetting.endDelay - curLeaveTime;
                            textTime.text = string.Format("Leave Room in {0} seconds", remain.ToString("n0"));
                        }
                        break;
                    }
                case GameState.End when procState == ProcState.Lock: return;
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

        #region PUN_CALLBACK

        public override void OnLeftRoom()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PhotonLobbySample");
        }

        public override void OnPlayerLeftRoom(PunPlayer otherPlayer)
        {
            Log.Print($"Player {otherPlayer.ActorNumber} Left Room");

            CheckEndOfGame();
        }

        public override void OnMasterClientSwitched(PunPlayer newMasterClient)
        {
            Log.Print($"Switch MasterClient {newMasterClient.ActorNumber}");
        }

        public override void OnPlayerPropertiesUpdate(PunPlayer targetPlayer, PunHashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerSettings.PLAYER_LOADED_LEVEL))
            {                
                if (PhotonNetwork.LocalPlayer.ActorNumber == targetPlayer.ActorNumber)
                {
                    SpawnPlayerNetwork();
                }

                if (PhotonNetwork.IsMasterClient)
                {
                    if (!CheckAllPlayerLoadedLevel())
                    {
                        // not all players loaded yet. wait:
                        Log.Print("Waiting for other players...");
                    }
                    else
                    {
                        // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
                        int startTimestamp;
                        bool startTimeIsSet = GenCountdownTimer.TryGetStartTime(out startTimestamp);

                        Log.Print("Players Loaded Complete!");
                        if (!startTimeIsSet)
                        {
                            GenCountdownTimer.SetStartTime();
                            GenerateObjectByMaster();
                        }
                    }
                }
            }

            if (changedProps.ContainsKey(PlayerSettings.PLAYER_DIE))
            {
                CheckEndOfGame();
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
                        PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.CheckCoopEvent, Photon.Realtime.ReceiverGroup.All, CooperateClearEvent.MobDie, false);
                        break;
                    }
                case EventCodeType.CheckCoopEvent:
                    {
                        CooperateClearEvent coopClearEvent = (CooperateClearEvent)data[0];
                        bool goToClear = (bool)data[1];

                        if (setting.IsClear(coopClearEvent))
                        {
                            PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.PlayerControllable, Photon.Realtime.ReceiverGroup.All, false);
                            SetProcState(ProcState.Lock);

                            if (goToClear)
                            {
                                PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.Clear, Photon.Realtime.ReceiverGroup.All);
                            }
                        }
                        break;
                    }
                case EventCodeType.Clear:
                    {
                        if (setting.IsAllClear())
                        {
                            SetGameState(GameState.Clear);
                            SetProcState(ProcState.Proc);
                        }
                        break;
                    }
            }
        }

        #endregion

        #region PUN_METHOD

        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (PunPlayer p in PhotonNetwork.PlayerList)
            {
                object playerLoadedLevel;

                if (p.CustomProperties.TryGetValue(PlayerSettings.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
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
            PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.GameStart, Photon.Realtime.ReceiverGroup.All);

            SetGameState(GameState.Play);
            SetProcState(ProcState.Proc);
        }

        private void CheckEndOfGame()
        {
            int allLivePlayerCnt = 0;
            Dictionary<int, int> liveTeamPlayerCnt = new Dictionary<int, int>(); // teamNum, cnt
            foreach (PunPlayer p in PhotonNetwork.PlayerList)
            {
                if (IsPlayerDie(p))
                    continue;

                allLivePlayerCnt++;

                if (p.CustomProperties.TryGetValue(PlayerSettings.PLAYER_TEAM, out object playerTeamNum))
                {
                    int teamNum = (int)playerTeamNum;
                    if (liveTeamPlayerCnt.ContainsKey(teamNum))
                    {
                        liveTeamPlayerCnt[teamNum]++;
                    }
                    else
                    {
                        liveTeamPlayerCnt.Add(teamNum, 1);
                    }
                }
            }

            switch (setting.gameMode)
            {
                case GameMode.Survival:
                    {
                        if(setting.matchType == MatchType.Free)
                        {
                            if (allLivePlayerCnt <= setting.survivalCount)
                            {
                                SetGameState(GameState.Clear);
                                SetProcState(ProcState.Proc);
                            }
                        }
                        else if(setting.matchType == MatchType.Team)
                        {
                            int liveTeamCnt = 0;
                            foreach(int teamNum in liveTeamPlayerCnt.Keys)
                            {
                                if(liveTeamPlayerCnt[teamNum] > 0)
                                {
                                    liveTeamCnt++;
                                }
                            }

                            if(liveTeamCnt <= 1)
                            {
                                SetGameState(GameState.Clear);
                                SetProcState(ProcState.Proc);
                            }
                        }
                        break;
                    }
                case GameMode.Cooperate:
                case GameMode.Sandbox:
                    {
                        if (allLivePlayerCnt <= 0)
                        {
                            SetGameState(GameState.Clear);
                            SetProcState(ProcState.Proc);
                        }
                        break;
                    }
            }
        }

        private bool IsPlayerDie(PunPlayer player)
        {
            object isDie;
            if (player.CustomProperties.TryGetValue(PlayerSettings.PLAYER_DIE, out isDie))
            {
                return (bool)isDie;
            }

            return false;
        }

        #endregion

        private void SetGameState(GameState gameState)
        {
            this.gameState = gameState;
        }

        private void SetProcState(ProcState procState)
        {
            this.procState = procState;
        }

        private void SpawnLocalPlayer()
        {
            Vector3 spawnPos = cubeContainer.GetRandomSpawnPos();

            GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
            GameObject goPlayer = Instantiate(pfPlayer, spawnPos, Quaternion.identity, null);
            PlayerController player = goPlayer.GetComponent<PlayerController>();
            player.SetPlayerUnitSetting(setting.playerUnitSetting.name);
            player.Init();
            player.MakeSpine(setting.playerUnitSetting.GetSpinePath());
            player.SetAtkType(Random.Range(0, 2));
            player.SetControllable(true);
            localPlayer = player;

            camViewController.SetTarget(player.transform);
        }

        public void SpawnDummyPlayer()
        {
            if (!Application.isPlaying)
            {
                Log.Error($"게임을 실행해주세요!");
                return;
            }

            if (PlayerSettings.IsConnectNetwork())
                return;

            Vector3 spawnPos = cubeContainer.GetRandomSpawnPos();

            GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
            GameObject goPlayer = Instantiate(pfPlayer, spawnPos, Quaternion.identity, null);
            PlayerController player = goPlayer.GetComponent<PlayerController>();
            player.SetPlayerUnitSetting(setting.playerUnitSetting.name);
            player.Init();
            player.MakeSpine(setting.playerUnitSetting.GetSpinePath());
            player.SetAtkType(Random.Range(0, 2));
            player.SetControllable(true);

            goPlayer.name = "Dummy" + goPlayer.name;
            player.SetDummy(PlayerController.DummyType.Fix);
            player.SetTargetPlayer(localPlayer);

            if (dummyPlayerList == null)
                dummyPlayerList = new List<PlayerController>();

            dummyPlayerList.Add(player);
        }

        public void DestroyAllDummyPlayer()
        {
            if (!Application.isPlaying)
            {
                Log.Error($"게임을 실행해주세요!");
                return;
            }

            if (dummyPlayerList == null)
                return;

            int dummyCount = dummyPlayerList.Count;
            for(int i = 0; i < dummyCount; i++)
            {
                Destroy(dummyPlayerList[i].gameObject);
            }
            dummyPlayerList.Clear();
        }

        private void SpawnPlayerNetwork()
        {
            Vector3 spawnPos = cubeContainer.GetRandomSpawnPos();

            var data = new List<object>();
            data.Add(setting.playerUnitSetting.name);
            data.Add(setting.playerUnitSetting.GetSpinePath());
            data.Add(Random.Range(0, 2));

            GameObject goPlayer = PhotonNetwork.Instantiate(PrefabPath.PlayerPath, spawnPos, Quaternion.identity, 0, data.ToArray());
            localPlayer = goPlayer.GetComponent<PlayerController>();

            camViewController.SetTarget(goPlayer.transform);
        }

        private void SetTextTime()
        {
            var remain = setting.limitTime - curTime;

            if (remain > 3)
                textTime.text = $"{(remain):n0}";
            else
                textTime.text = $"{(remain):f1}";
        }

        private void FailGame()
        {
            if (PlayerSettings.IsConnectNetwork())
            {
                PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.Fail, Photon.Realtime.ReceiverGroup.All);
            }
            else
            {
                if (localPlayer != null)
                    localPlayer.SetControllable(false);
            }

            SetGameState(GameState.Fail);
            SetProcState(ProcState.Proc);
        }

        private void LeaveRoom()
        {
            if (!PlayerSettings.IsConnectNetwork())
                return;

            RoomSettings.roomName = PhotonNetwork.CurrentRoom.Name;
            RoomSettings.isMaster = PhotonNetwork.IsMasterClient;

            PhotonNetwork.LeaveRoom();
        }

        private void GenerateObjectByMaster()
        {
            cubeContainer.GenerateNormalCube();
            cubeContainer.GenerateMonster();
        }
    }
}