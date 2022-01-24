using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Sandbox;
using Assets.Scripts.Feature.Sandbox.Cube;
using Assets.Scripts.Feature.Sandbox.UI;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class SandboxManager : MonoBehaviourPunCallbacks
    {
        public enum SANDBOX_STATE
        {
            Init,
            Play,
            Idle,
        }
        public enum PLAYER_TYPE
        {
            Designer,
            Player,
        }
        public PLAYER_TYPE playerType;
        private Transform cubeContainer;
        private CubeSlotController cubeSlotController;

        private SandboxCameraController sbCamCtrl;
        private CubeBase objShowCube;
        private SANDBOX_STATE curState;
        private UnitBase unit;
        private GameObject hitCube;

        public string nextCubeName = "GroundCube";
        private string curCubeName;

        private float prevCubeHeight;
        private Vector3 prevMousePos;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            SetState(SANDBOX_STATE.Init);
        }

        // Update is called once per frame
        void Update()
        {
            switch (curState)
            {
                case SANDBOX_STATE.Idle: return;
                case SANDBOX_STATE.Init:
                    {
                        Init();
                        break;
                    }
                case SANDBOX_STATE.Play:
                    {
                        if (PlayerSettings.IsConnectNetwork())
                        {
                            if (Input.GetKeyDown(KeyCode.Escape))
                            {
                                PhotonNetwork.LeaveRoom();
                            }
                        }

                        bool removeCube = false;
                        if (playerType == PLAYER_TYPE.Designer)
                        {
                            cubeSlotController.ShowSlotUI();
                            if (Input.GetKeyDown(KeyCode.F))
                            {
                                sbCamCtrl.LookTarget();
                            }

                            if (Input.GetKey(KeyCode.LeftControl))
                            {
                                removeCube = true;
                                ActiveShowCube(false);
                            }
                        }
                        else
                        {
                            cubeSlotController.HideSlotUI();
                            if (objShowCube != null)
                                ActiveShowCube(false);

                            if (Input.GetKeyDown(KeyCode.R))
                            {
                                unit.ResetSpawnPos(Vector3.up);
                            }
                        }

                        if (Input.GetMouseButtonDown(0))
                        {
                            if (!removeCube && objShowCube != null && objShowCube.gameObject.activeSelf)
                            {
                                objShowCube.MakeRealCube(curCubeName);
                                prevCubeHeight = objShowCube.GetPosition().y;
                                prevMousePos = Input.mousePosition;                                
                            }
                        }

                        if (Input.GetMouseButton(0))
                        {
                            if (removeCube)
                            {
                                if(hitCube != null)
                                {
                                    hitCube.GetComponent<CubeBase>().DestroyCube();
                                }                                    
                            }
                            else
                            {
                                if (objShowCube != null && objShowCube.gameObject.activeSelf)
                                {
                                    float distMousePos = Vector3.Distance(Input.mousePosition, prevMousePos);
                                    if(distMousePos >= .2f && prevCubeHeight == objShowCube.GetPosition().y)
                                        objShowCube.MakeRealCube(curCubeName);
                                }
                            }
                        }

                        unit.SetControllable(playerType == PLAYER_TYPE.Player);
                        break;
                    }
            }            
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
            Log.Print($"Master Switched {newMasterClient.ActorNumber}");
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(PlayerSettings.PLAYER_DIE))
            {
                CheckEndOfGame();
            }

            if (!PhotonNetwork.IsMasterClient)
                return;

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

                    Hashtable props = new Hashtable
                        {
                            {RoomSettings.StartRoom, true}
                        };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(props);
                }
            }
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Debug.Log("OnRoomPropertiesUpdate " + propertiesThatChanged.ToStringFull());
            
            if(propertiesThatChanged.TryGetValue(RoomSettings.StartRoom, out object isStart))
            {
                if((bool)isStart)
                {
                    InitValue();

                    SetState(SANDBOX_STATE.Play);
                }
            }
        }

        #endregion

        #region PUN_METHOD

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
                LeaveRoom();
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

        private void LeaveRoom()
        {
            RoomSettings.roomName = PhotonNetwork.CurrentRoom.Name;
            RoomSettings.isMaster = PhotonNetwork.IsMasterClient;

            Hashtable props = new Hashtable();
            props.Add(PlayerSettings.PLAYER_LOADED_LEVEL, false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            PhotonNetwork.LeaveRoom();
        }

        #endregion

        private void SetState(SANDBOX_STATE state)
        {
            curState = state;
        }

        private void Init()
        {
            cubeContainer = FindObjectOfType<CubeContainer>().transform;
            cubeSlotController = FindObjectOfType<CubeSlotController>();
            cubeSlotController.Build(this);

            if (!PlayerSettings.IsConnectNetwork())
            {
                InitValue();

                SetState(SANDBOX_STATE.Play);
            }
            else
            {
                Hashtable props = new Hashtable();
                props.Add(PlayerSettings.PLAYER_LOADED_LEVEL, true);
                props.Add(PlayerSettings.PLAYER_DIE, false);

                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                SetState(SANDBOX_STATE.Idle);
            }
        }

        private void InitValue()
        {
            sbCamCtrl = FindObjectOfType<SandboxCameraController>();
            if (sbCamCtrl == null)
                Log.Error($"SandboxCameraController를 찾을 수 없습니다!");
            else
            {
                sbCamCtrl.Init(this);
            }

            curCubeName = nextCubeName;
            objShowCube = null;

            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            Vector3 initPos = Vector3.up;

            if (!PlayerSettings.IsConnectNetwork())
            {
                GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>("Prefab/Unit/LocalPlayer");
                GameObject goPlayer = Instantiate(pfPlayer, initPos, Quaternion.identity);
                unit = goPlayer.GetComponent<UnitLocalPlayer>();
                unit.Init();
                unit.SetControllable(playerType == PLAYER_TYPE.Player);
                PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
                if (!UnitSettings.useSpine())
                {
                    Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict(playerUnitSetting.GetUnitType());
                    unit.SetSprite(selectUnitParts);
                }
                else
                    unit.MakeSpine(playerUnitSetting.GetSpinePath());

                sbCamCtrl.SetTarget(unit.transform);
            }
            else
            {
                var data = new List<object>();

                // Set Sprite
                PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
                Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict(playerUnitSetting.GetUnitType());
                data.Add(selectUnitParts);

                // Set AtkType
                int atkTypeIdx = UnityEngine.Random.Range(0, Enum.GetValues(typeof(UnitBase.ATK_TYPE)).Length);
                data.Add(atkTypeIdx);

                // Set Spine
                data.Add(playerUnitSetting.GetSpinePath());
                data.Add(UnitSettings.useSpine());

                // Set SpawnPos
                Vector3 orgSpawnPos = initPos;

                GameObject netGoPlayer = PhotonNetwork.Instantiate(Path.Combine("Prefab", "Unit/NetworkPlayer"), orgSpawnPos, Quaternion.identity, 0, data.ToArray());
                unit = netGoPlayer.GetComponent<UnitNetworkPlayer>();
                unit.SetControllable(playerType == PLAYER_TYPE.Player);

                sbCamCtrl.SetTarget(unit.transform);
            }
        }

        public void ShowCube(Transform hit, Vector3 normal)
        {
            if (objShowCube == null)
            {
                MakeShowCube();
            }

            if (curCubeName != nextCubeName)
            {
                curCubeName = nextCubeName;

                Destroy(objShowCube.gameObject);
                MakeShowCube();
            }

            if (hit == objShowCube.transform) 
                return;

            hitCube = hit.gameObject;

            Vector3 orgPos = hit.position;
            Vector3 showPos = normal * objShowCube.transform.localScale.x;

            objShowCube.ClearCollObjs();
            objShowCube.SetPosition(orgPos + showPos);
        }

        public void ActiveShowCube(bool isActive)
        {
            if (objShowCube == null)
                return;

            objShowCube.gameObject.SetActive(isActive);
        }

        private void MakeShowCube()
        {
            GameObject pfShowCube = ResourceManager.LoadAsset<GameObject>($"Prefab/Sandbox/Cube/{curCubeName}");
            GameObject goShowCube = Instantiate(pfShowCube, cubeContainer);
            objShowCube = goShowCube.GetComponent<CubeBase>();
            objShowCube.SetGuide(true);
        }

        public PLAYER_TYPE GetPlayerType()
        {
            return playerType;
        }

        public void SetNextCube(string cubeName)
        {
            nextCubeName = cubeName;
        }
    }
}