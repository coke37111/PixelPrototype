using Assets.Scripts.Feature.Main.Camera;
using Assets.Scripts.Feature.Main.Cubes;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using Photon.Pun;
using System.Collections;
using UnityEngine;

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

                        CameraViewController camViewController = mainCam.GetComponent<CameraViewController>();
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

                        }
                        else
                        {
                            cubeContainer.GenerateNormalCube();


                            Vector3 spawnPos = cubeContainer.GetRandomSpawnPos();

                            //GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>(PrefabPath.PlayerPath);
                            //GameObject goPlayer = Instantiate(pfPlayer, spawnPos, Quaternion.identity, null);
                            //player = goPlayer.GetComponent<PlayerController>();
                            //player.SetControllable(false);
                            //player.SetScale(playerScale);

                            //camCtrl.SetTarget(goPlayer.transform);
                        }
                        break;
                    }
                case GameState.ConnectNet when procState == ProcState.Lock: return;
            }
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
    }
}