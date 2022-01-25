using Assets.Scripts.Feature.Bomberman.Unit;
using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PunHashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.Bomberman
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

        public TMPro.TextMeshProUGUI infoText;

        #region UNITY

        // Use this for initialization
        void Start()
        {
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
                            SetGameState(GameState.Init);
                        }
                        break;
                    }
                case GameState.Init:
                    {
                        camCtrl = FindObjectOfType<BombermanCameraController>();
                        mapCtrl = FindObjectOfType<BombermanMapController>();
                        mapCtrl.Init();

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

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Log.Print($"Player {otherPlayer.ActorNumber} Left Room");

            PhotonNetwork.DestroyPlayerObjects(otherPlayer);
            CheckEndOfGame(true);
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Log.Print($"Switch MasterClient {newMasterClient.ActorNumber}");
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, PunHashtable changedProps)
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
            if (PlayerSettings.IsConnectNetwork())
            {
                var data = new List<object>();

                PhotonNetwork.Instantiate($"Prefab/BomberMan/Player", Vector3.zero, Quaternion.identity, 0, data.ToArray());
            }
            else
            {
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>($"Prefab/BomberMan/Player");
                if (pfPlayer != null)
                {
                    Transform unitContainer = FindObjectOfType<UnitContainer>().transform;
                    GameObject goPlayer = Instantiate(pfPlayer, unitContainer);
                    player = goPlayer.GetComponent<PlayerController>();
                    player.SetBomberManMapController(mapCtrl);

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
            foreach (Player p in PhotonNetwork.PlayerListOthers)
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

        private bool IsPlayerDie(Player player)
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
            foreach (Player p in PhotonNetwork.PlayerList)
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
    }
}