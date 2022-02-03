﻿using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.Camera;
using Assets.Scripts.Feature.Main.nsCube;
using Assets.Scripts.Feature.Main.Player;
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

namespace Assets.Scripts.Feature.BombermanNew
{
    public class BombermanNewManager : MonoBehaviourPunCallbacks
    {
        public enum MANAGER_STATE
        {
            Init,
            Play,
            End,
        }
        private MANAGER_STATE managerState;

        public enum PROC_STATE
        {
            Start,
            Proc,
            Complete,
        }
        private PROC_STATE procState;

        public SandboxMapDataSO mapData;
        public TMPro.TextMeshProUGUI infoText;

        private CubeContainer cubeContainer;
        private TopViewCamCtrl camCtrl;

        private PlayerController player;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            SetState(MANAGER_STATE.Init, PROC_STATE.Start);
        }

        // Update is called once per frame
        void Update()
        {
            switch (managerState)
            {
                case MANAGER_STATE.Init when procState == PROC_STATE.Start:
                    {
                        InitValue();
                        GenerateMap();
                        ConnectNetwork();
                        break;
                    }
                case MANAGER_STATE.Init when procState == PROC_STATE.Proc: break;
                case MANAGER_STATE.Init when procState == PROC_STATE.Complete:
                    {
                        if(player != null)
                            player.SetControllable(true);

                        SetState(MANAGER_STATE.Play, PROC_STATE.Start);
                        break;
                    }
                case MANAGER_STATE.Play:
                    {
                        break;
                    }
                case MANAGER_STATE.End when procState == PROC_STATE.Start:
                    {
                        StartCoroutine(EndOfGame());
                        SetState(MANAGER_STATE.End, PROC_STATE.Proc);
                        break;
                    }
                case MANAGER_STATE.End when procState == PROC_STATE.Proc: break;
                case MANAGER_STATE.End when procState == PROC_STATE.Complete:
                    {
                        LeaveRoom();
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
            Log.Print($"Switch MasterClient {newMasterClient.ActorNumber}");
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, PunHashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerSettings.PLAYER_DIE))
            {
                CheckEndOfGame();
            }

            if (changedProps.ContainsKey(PlayerSettings.PLAYER_LOADED_LEVEL))
            {
                if(PhotonNetwork.LocalPlayer.ActorNumber == targetPlayer.ActorNumber &&
                    targetPlayer.CustomProperties.TryGetValue(PlayerSettings.PLAYER_DIE, out object isDie))
                {
                    if(!(bool)isDie)
                        SpawnPlayer();
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
                        }
                    }
                }
            }
        }

        #endregion

        private void SetState(MANAGER_STATE state, PROC_STATE procState)
        {
            managerState = state;
            this.procState = procState;
        }

        private void InitValue()
        {
            cubeContainer = FindObjectOfType<CubeContainer>();
            camCtrl = FindObjectOfType<TopViewCamCtrl>();
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

        private void ConnectNetwork()
        {
            if (PlayerSettings.IsConnectNetwork())
            {
                PunHashtable props = new PunHashtable();
                props.Add(PlayerSettings.PLAYER_LOADED_LEVEL, true);
                props.Add(PlayerSettings.PLAYER_DIE, false);

                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                SetState(MANAGER_STATE.Init, PROC_STATE.Proc);
            }
            else
            {
                RoomSettings.roomType = RoomSettings.ROOM_TYPE.Bomberman;
                SpawnPlayer();

                SetState(MANAGER_STATE.Init, PROC_STATE.Complete);
            }
        }

        private void SpawnPlayer()
        {
            Vector3 spawnPosTo3 = Vector3.zero;

            if (PlayerSettings.IsConnectNetwork())
            {
                var data = new List<object>();

                // Set Spine
                PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
                string spinePath = playerUnitSetting.GetSpinePath();
                data.Add(spinePath);

                GameObject goPlayer = PhotonNetwork.Instantiate(PrefabPath.PlayerPath, spawnPosTo3, Quaternion.identity, 0, data.ToArray());
                player = goPlayer.GetComponent<PlayerController>();
                player.SetControllable(false);

                camCtrl.SetTarget(goPlayer.transform);
            }
            else
            {
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
                GameObject goPlayer = Instantiate(pfPlayer, spawnPosTo3, Quaternion.identity, null);
                player = goPlayer.GetComponent<PlayerController>();
                player.SetControllable(false);

                camCtrl.SetTarget(goPlayer.transform);
            }
        }

        private void CheckEndOfGame()
        {
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

                SetState(MANAGER_STATE.End, PROC_STATE.Start);
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
            SetState(MANAGER_STATE.Init, PROC_STATE.Complete);
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

            SetState(MANAGER_STATE.End, PROC_STATE.Complete);
        }
    }
}