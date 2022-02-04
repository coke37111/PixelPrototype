using Assets.Scripts.Feature.Main.Block;
using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Sandbox;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PunHashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.Main
{
    public class BombermanManager : MonoBehaviourPunCallbacks
    {
        public enum GameState
        {
            Idle,
            ConnectServer,
            Init,
            Play,
            End,
            EndWait,
        }
        private GameState gameState;

        private BombermanCameraController camCtrl;
        private BombermanMapController mapCtrl;
        private PlayerController player;
        private CubeContainer cubeContainer;

        public TMPro.TextMeshProUGUI infoText;
        public SandboxMapDataSO mapData;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            cubeContainer = FindObjectOfType<CubeContainer>();
            cubeContainer.GenerateCubes(mapData);

            camCtrl = FindObjectOfType<BombermanCameraController>();
            mapCtrl = FindObjectOfType<BombermanMapController>();
            mapCtrl.Init();

            gameState = GameState.ConnectServer;
        }

        // Update is called once per frame
        void Update()
        {
            switch (gameState)
            {
                case GameState.Idle: return;
                case GameState.ConnectServer:
                    {
                        if (PlayerSettings.IsConnectNetwork())
                        {
                            PunHashtable props = new PunHashtable();
                            props.Add(PlayerSettings.PLAYER_LOADED_LEVEL, true);
                            props.Add(PlayerSettings.PLAYER_DIE, false);
                            
                            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                            SetGameState(GameState.Idle);
                        }
                        else
                        {
                            RoomSettings.roomType = RoomSettings.ROOM_TYPE.Bomberman;
                            SetGameState(GameState.Init);
                        }
                        break;
                    }
                case GameState.Init:
                    {
                        if (!PlayerSettings.IsConnectNetwork())
                        {
                            GenerateNormalBlock();
                        }
                        MakePlayer();

                        SetGameState(GameState.Play);
                        break;
                    }
                case GameState.Play:
                    {
                        if (PlayerSettings.IsConnectNetwork())
                        {
                            if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                LeaveRoom();
                            }
                        }
                        else
                        {
                            if (Input.GetKeyDown(KeyCode.F1))
                            {
                                if (player == null)
                                {
                                    MakePlayer();
                                }

                            }
                        }
                        break;
                    }
                case GameState.End:
                    {
                        StartCoroutine(EndOfGame());
                        SetGameState(GameState.EndWait);
                        break;
                    }
                case GameState.EndWait:
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

        #region PUN_CALLBACKS

        public override void OnLeftRoom()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PhotonLobbySample");
        }

        public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
        {
            Log.Print($"Player {otherPlayer.ActorNumber} Left Room");

            PhotonNetwork.DestroyPlayerObjects(otherPlayer);
            CheckEndOfGame(true);
        }

        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            Log.Print($"Switch MasterClient {newMasterClient.ActorNumber}");
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, PunHashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerSettings.PLAYER_DIE))
            {
                CheckEndOfGame();
            }

            if (PhotonNetwork.IsMasterClient)
            {
                // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
                int startTimestamp;
                bool startTimeIsSet = GenCountdownTimer.TryGetStartTime(out startTimestamp);

                if (changedProps.ContainsKey(PlayerSettings.PLAYER_LOADED_LEVEL))
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

                            if (PhotonNetwork.MasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                                GenerateNormalBlock();
                        }
                    }
                }
            }
        }

        #endregion

        private void SetGameState(GameState gameState)
        {
            this.gameState = gameState;
        }

        public bool IsEndGame()
        {
            return gameState >= GameState.End;
        }

        private void MakePlayer()
        {
            Vector2Int spawnPos = mapCtrl.GetRandomSpawnPos();
            Vector3 spawnPosTo3 = new Vector3(spawnPos.x, 0f, spawnPos.y);           

            if (PlayerSettings.IsConnectNetwork())
            {
                var data = new List<object>();

                // Set Spine
                PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
                string spinePath = playerUnitSetting.GetSpinePath();
                data.Add(spinePath);

                PhotonNetwork.Instantiate(PrefabPath.PlayerPath, spawnPosTo3, Quaternion.identity, 0, data.ToArray());
            }
            else
            {
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
                if (pfPlayer != null)
                {
                    Transform unitContainer = FindObjectOfType<UnitContainer>().transform;
                    GameObject goPlayer = Instantiate(pfPlayer, spawnPosTo3, Quaternion.identity, unitContainer);
                    player = goPlayer.GetComponent<PlayerController>();

                    if (camCtrl != null)
                        camCtrl.SetTarget(goPlayer.transform);
                }
            }
        }

        private void CheckEndOfGame(bool isForced = false)
        {
            if (!isForced && gameState != GameState.Play)
                return;

            bool allDestroyed = true;
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerListOthers)
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

                SetGameState(GameState.End);
            }
        }

        private void LeaveRoom()
        {
            RoomSettings.roomName = PhotonNetwork.CurrentRoom.Name;
            RoomSettings.isMaster = PhotonNetwork.IsMasterClient;

            PunHashtable props = new PunHashtable();
            props.Add(PlayerSettings.PLAYER_LOADED_LEVEL, false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            PhotonNetwork.LeaveRoom();
        }

        private bool IsPlayerDie(Photon.Realtime.Player player)
        {
            object isDie;
            if (player.CustomProperties.TryGetValue(PlayerSettings.PLAYER_DIE, out isDie))
            {
                return (bool)isDie;
            }

            return false;
        }

        private bool CheckAllPlayerLoadedLevel()
        {
            foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
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
            SetGameState(GameState.Init);
        }

        private IEnumerator EndOfGame()
        {
            yield return null;
            float timer = 3f;

            while (timer > 0.0f)
            {
                infoText.text = string.Format("Leave Room in {0} seconds", timer.ToString("n0"));
                yield return new WaitForEndOfFrame();

                timer -= Time.deltaTime;
            }

            LeaveRoom();
        }

        private void GenerateNormalBlock()
        {
            string blockPath = $"Prefab/Main/NormalBlock/NormalBlock";
            int mapSize = mapCtrl.GetMapSize();
            for (int col = -mapSize; col <= mapSize; col++)
            {
                if (col % 2 != 0)
                    continue;

                for (int row = -mapSize; row <= mapSize; row++)
                {
                    if (row % 2 == 0)
                        continue;

                    if (mapCtrl.isEndOfMap(new Vector2Int(col, row)))
                        continue;

                    if (Random.Range(0f, 1f) >= .5f)
                        continue;

                    if (PlayerSettings.IsConnectNetwork())
                    {
                        var data = new List<object>();
                        PhotonNetwork.Instantiate(blockPath, new Vector3(col, 0f, row), Quaternion.identity, 0, data.ToArray());
                    }
                    else
                    {
                        GameObject pfNormalBlock = ResourceManager.LoadAsset<GameObject>(blockPath);
                        if (pfNormalBlock == null)
                            break;

                        GameObject goNormalB = Instantiate(pfNormalBlock, new Vector3(col, 0f, row), Quaternion.identity);
                        goNormalB.GetComponent<NormalBlock>().Init();
                    }
                }
            }
        }
    }
}