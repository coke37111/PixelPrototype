using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Feature.Sandbox;
using Assets.Scripts.Feature.Sandbox.UI;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using MainCubeContainer = Assets.Scripts.Feature.Main.Cubes.CubeContainer;
using static Assets.Scripts.Settings.PlayerSettings;

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
        public Vector3 DefaultMapSize;
        public GameObject DefaultCube;
        public SandboxMapDataSO LoadMapData;

        private CubeSlotController cubeSlotController;
        private MainCubeContainer cubeContainer;

        private SandboxCameraController sbCamCtrl;
        private EditCube objShowCubeNew;
        private SANDBOX_STATE curState;
        private PlayerController player;
        private GameObject hitCube;

        private string nextCubeName = "GroundCube";
        private string curCubeName;

        private Vector3 prevMousePos;
        private Vector3 prevCubePos;
        private bool makeCubeX, makeCubeY, makeCubeZ = false;

        public Vector3 playerScale = new Vector3(1, 1, 1);

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
                            if (objShowCubeNew != null)
                                ActiveShowCube(false);

                            if (Input.GetKeyDown(KeyCode.R) && player != null)
                            {
                                player.transform.position = Vector3.up;
                            }
                        }

                        if (Input.GetMouseButtonDown(0))
                        {
                            if (!removeCube && objShowCubeNew != null && objShowCubeNew.gameObject.activeSelf)
                            {
                                objShowCubeNew.MakeRealCube();
                                prevMousePos = Input.mousePosition;

                                Vector3 cubePos = objShowCubeNew.GetPosition();
                                prevCubePos = cubePos;
                                Vector3 diffPos = cubePos - hitCube.transform.position;

                                if (Mathf.Abs(diffPos.x) > 0f)
                                {
                                    makeCubeX = true;
                                }
                                else if (Mathf.Abs(diffPos.y) > 0f)
                                {
                                    makeCubeY = true;
                                }
                                else if (Mathf.Abs(diffPos.z) > 0f)
                                {
                                    makeCubeZ = true;
                                }
                                else
                                {
                                    makeCubeX = makeCubeY = makeCubeZ = false;
                                }
                            }
                        }

                        if (Input.GetMouseButton(0))
                        {
                            if (removeCube)
                            {
                                if(hitCube != null)
                                {
                                    float distMousePos = Vector3.Distance(Input.mousePosition, prevMousePos);
                                    if (distMousePos >= .5f)
                                        hitCube.GetComponent<EditCube>().DestroyCube();
                                }                                    
                            }
                            else
                            {
                                if (objShowCubeNew != null && objShowCubeNew.gameObject.activeSelf)
                                {
                                    float distMousePos = Vector3.Distance(Input.mousePosition, prevMousePos);
                                    if(distMousePos >= .5f)
                                    {
                                        prevMousePos = Input.mousePosition;

                                        Vector3 cubePos = objShowCubeNew.GetPosition();
                                        Vector3 diffPos = cubePos - prevCubePos;
                                        float diffX = Mathf.Abs(diffPos.x);
                                        float diffY = Mathf.Abs(diffPos.y);
                                        float diffZ = Mathf.Abs(diffPos.z);

                                        if (makeCubeX)
                                        {
                                            if (diffX > 0f && diffY == 0f && diffZ == 0f)
                                            {
                                                objShowCubeNew.MakeRealCube();
                                                prevCubePos = cubePos;
                                            }
                                        }
                                        else if (makeCubeY)
                                        {
                                            if (diffY > 0f && diffX == 0f && diffZ == 0f)
                                            {
                                                objShowCubeNew.MakeRealCube();
                                                prevCubePos = cubePos;
                                            }
                                        }
                                        else if (makeCubeZ)
                                        {
                                            if (diffZ > 0f && diffX == 0f && diffY == 0f)
                                            {
                                                objShowCubeNew.MakeRealCube();
                                                prevCubePos = cubePos;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (Input.GetMouseButtonUp(0))
                        {
                            makeCubeX = makeCubeY = makeCubeZ = false;
                        }

                        bool isPlayerMode = playerType == PLAYER_TYPE.Player;

                        if (player != null)
                        {
                            if (isPlayerMode)
                            {
                                player.SetPlayMode();
                                player.SetControllable(isPlayerMode);
                            }
                            else
                            {
                                Destroy(player.gameObject);
                                sbCamCtrl.ResetPos();
                            }
                        }
                        else
                        {
                            if (isPlayerMode)
                            {
                                SpawnPlayer();
                            }
                        }
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
            cubeSlotController = FindObjectOfType<CubeSlotController>();
            cubeSlotController.Build(this);
            cubeContainer = FindObjectOfType<MainCubeContainer>();

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
            objShowCubeNew = null;

            if(playerType == PLAYER_TYPE.Designer)
            {
                MakeDefaultMap();
            }

            //SpawnPlayer();
        }

        private void MakeDefaultMap()
        {            
            Vector3 defaultCubeScale = DefaultCube.transform.localScale;
            int ofsX = (int)((DefaultMapSize.x - defaultCubeScale.x) * 0.5f);
            int ofsY = 0;// DefaultMapSize.y - defaultCubeScale.y;
            int ofsZ = (int)((DefaultMapSize.z - defaultCubeScale.z) * 0.5f);
            for (int col = 0; col < DefaultMapSize.x; col++)
            {
                for(int height = 0; height < DefaultMapSize.y; height++)
                {
                    for (int row = 0; row < DefaultMapSize.z; row++)
                    {
                        Vector3 pos = new Vector3(col - ofsX, height - ofsY, row - ofsZ);
                        GameObject pfCube = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
                        GameObject goCube = Instantiate(pfCube, pos, Quaternion.identity, cubeContainer.transform);
                        EditCube cube = goCube.GetComponent<EditCube>();
                        cube.Build(DefaultCube.name);
                    }
                }
            }
        }

        private void SpawnPlayer()
        {
            Vector3 spawnPosTo3 = cubeContainer.GetRandomSpawnPosEdit();
            PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);

            if (PlayerSettings.IsConnectNetwork())
            {
                var data = new List<object>();
                data.Add(playerUnitSetting.name);
                data.Add(playerUnitSetting.GetSpinePath());
                data.Add(Random.Range(0, 2));

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
                    player.SetPlayerUnitSetting(playerUnitSetting.name);
                    player.Init();
                    player.MakeSpine(playerUnitSetting.GetSpinePath());
                    player.SetAtkType(Random.Range(0, 2));
                    player.SetControllable(false);

                    sbCamCtrl.SetTarget(goPlayer.transform);

                    if (playerType == PLAYER_TYPE.Designer)
                    {
                        player.SetEditMode();
                    }
                    else
                    {
                        player.SetPlayMode();
                    }
                }
            }
        }

        public void ShowCube(Transform hit, Vector3 normal)
        {
            if (objShowCubeNew == null)
            {
                MakeShowCube();
            }

            if (curCubeName != nextCubeName)
            {
                curCubeName = nextCubeName;

                Destroy(objShowCubeNew.gameObject);
                MakeShowCube();
            }

            if (hit == objShowCubeNew.transform) 
                return;

            hitCube = hit.gameObject;

            Vector3 orgPos = hit.position;
            float posOffset = 0f;
            if(normal == Vector3.left || normal == Vector3.right)
            {
                posOffset = objShowCubeNew.transform.localScale.x;
            }else if(normal == Vector3.forward || normal == Vector3.back)
            {
                posOffset = objShowCubeNew.transform.localScale.z;
            }
            else if(normal == Vector3.up || normal == Vector3.down)
            {
                posOffset = objShowCubeNew.transform.localScale.y;
            }
            Vector3 showPos = normal * posOffset;

            objShowCubeNew.ClearCollObjs();
            objShowCubeNew.SetPosition(orgPos + showPos);
        }

        public void ActiveShowCube(bool isActive)
        {
            if (objShowCubeNew == null)
                return;

            objShowCubeNew.gameObject.SetActive(isActive);
        }

        private void MakeShowCube()
        {
            GameObject pfShowCube = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
            GameObject goShowCube = Instantiate(pfShowCube);
            objShowCubeNew = goShowCube.GetComponent<EditCube>();
            objShowCubeNew.Build(curCubeName);
        }

        public PLAYER_TYPE GetPlayerType()
        {
            return playerType;
        }

        public void SetNextCube(string cubeName)
        {
            nextCubeName = cubeName;
        }

        public bool CanSaveMap()
        {
            if (!Application.isPlaying)
            {
                Log.Error($"게임을 실행해주세요!");
            }

            return Application.isPlaying;
        }

        public List<EditCube> GetCubes()
        {
            Log.Print("Save");

            MainCubeContainer container = FindObjectOfType<MainCubeContainer>();
            List<EditCube> cubes = container.GetAllCubes();

            return cubes;
        }

        public void LoadMap()
        {
            Log.Print("Load");

            if (!Application.isPlaying)
            {
                Log.Error($"게임을 실행해주세요!");
                return;
            }

            if(LoadMapData == null)
            {
                Log.Error($"LoadMapData가 할당되있지 않습니다!");
                return;
            }

            MainCubeContainer container = FindObjectOfType<MainCubeContainer>();
            container.DestroyAllCubes();

            container.GenerateCubes(LoadMapData, true);
            container.GenerateNormalCube();
        }

        public void ShowGuardCube(bool flag)
        {
            if (cubeContainer == null)
                return;

            if (flag)
            {
                cubeContainer.ShowGuardCube();
            }
            else
            {
                cubeContainer.HideGuardCube();
            }
        }
    }
}