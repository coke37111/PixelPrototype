using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.Camera;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using PunHashtable = ExitGames.Client.Photon.Hashtable;
using PunPlayer = Photon.Realtime.Player;

namespace Assets.Scripts.Feature.Main
{
    public class GameModeManager : MonoBehaviourPunCallbacks
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

        private CubeContainer cubeContainer;
        private CameraViewController camViewController;

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


                        cubeContainer = FindObjectOfType<CubeContainer>();
                        if(cubeContainer == null)
                        {
                            Log.Error($"Scene에 CubeContainer가 존재하지 않아 동적으로 생성합니다.");

                            GameObject go = new GameObject("CubeContainer", typeof(CubeContainer));
                            cubeContainer = go.GetComponent<CubeContainer>();
                        }
                        cubeContainer.GenerateCubes(setting.mapData);


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
                            cubeContainer.GenerateNormalCube();
                            SpawnLocalPlayer();

                            SetGameState(GameState.Play);
                            SetProcState(ProcState.Proc);
                        }
                        break;
                    }
                case GameState.ConnectNet when procState == ProcState.Lock: return;
                case GameState.Play when procState == ProcState.Proc:
                    {
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

        #region PUN_CALLBACK

        public override void OnLeftRoom()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PhotonLobbySample");
        }

        public override void OnPlayerLeftRoom(PunPlayer otherPlayer)
        {
            Log.Print($"Player {otherPlayer.ActorNumber} Left Room");
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
                            cubeContainer.GenerateNormalCube();
                        }
                    }
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

            camViewController.SetTarget(player.transform);
        }

        private void SpawnPlayerNetwork()
        {
            Vector3 spawnPos = cubeContainer.GetRandomSpawnPos();

            var data = new List<object>();
            data.Add(setting.playerUnitSetting.name);
            data.Add(setting.playerUnitSetting.GetSpinePath());
            data.Add(Random.Range(0, 2));

            GameObject goPlayer = PhotonNetwork.Instantiate(PrefabPath.PlayerPath, spawnPos, Quaternion.identity, 0, data.ToArray());

            camViewController.SetTarget(goPlayer.transform);
        }
    }
}