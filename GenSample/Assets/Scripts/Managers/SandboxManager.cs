using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Feature.Sandbox;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class SandboxManager : MonoBehaviour
    {
        public enum SANDBOX_STATE
        {
            Init,
            Play,
        }
        public enum PLAYER_TYPE
        {
            Designer,
            Player,
        }
        public PLAYER_TYPE playerType;
        public Transform cubeContainer;

        private SandboxCameraController sbCamCtrl;
        private ShowCube objShowCube;
        private SANDBOX_STATE curState;
        private UnitBase unit;

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
                case SANDBOX_STATE.Init:
                    {
                        Init();

                        SetState(SANDBOX_STATE.Play);
                        break;
                    }
                case SANDBOX_STATE.Play:
                    {
                        if (playerType == PLAYER_TYPE.Designer)
                        {
                            if (Input.GetMouseButtonUp(0))
                            {
                                objShowCube.MakeRealCube(cubeContainer);
                            }
                        }

                        unit.SetControllable(playerType == PLAYER_TYPE.Player);
                        if(objShowCube != null)
                            objShowCube.gameObject.SetActive(playerType == PLAYER_TYPE.Designer);
                        break;
                    }
            }            
        }

        #endregion

        private void SetState(SANDBOX_STATE state)
        {
            curState = state;
        }

        private void Init()
        {
            sbCamCtrl = FindObjectOfType<SandboxCameraController>();
            if (sbCamCtrl == null)
                Log.Error($"SandboxCameraController를 찾을 수 없습니다!");
            else
            {
                sbCamCtrl.Init(this);
            }

            objShowCube = null;

            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>("Prefab/Unit/LocalPlayer");
            GameObject goPlayer = Instantiate(pfPlayer, Vector3.up, Quaternion.identity);
            unit = goPlayer.GetComponent<UnitLocalPlayer>();
            unit.Init();
            unit.SetControllable(playerType == PLAYER_TYPE.Player);
            if (!UnitSettings.useSpine())
            {
                PlayerUnitSettingSO playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
                Dictionary<string, string> selectUnitParts = UnitSettings.GetSelectUnitPartDict(playerUnitSetting.GetUnitType());
                unit.SetSprite(selectUnitParts);
            }
            else
                unit.MakeSpine(PlayerUnitSettingSO.spinePath);

            sbCamCtrl.SetTarget(unit.transform);
        }

        public void ShowCube(Transform hit, Vector3 normal)
        {
            if (objShowCube == null)
            {
                GameObject pfShowCube = ResourceManager.LoadAsset<GameObject>("Prefab/Sandbox/ShowCube");
                GameObject goShowCube = Instantiate(pfShowCube, cubeContainer);
                objShowCube = goShowCube.GetComponent<ShowCube>();
            }

            if (hit == objShowCube.transform) 
                return;

            Vector3 orgPos = hit.position;
            Vector3 showPos = normal * .25f;

            objShowCube.SetPosition(orgPos + showPos);
        }

        public PLAYER_TYPE GetPlayerType()
        {
            return playerType;
        }
    }
}