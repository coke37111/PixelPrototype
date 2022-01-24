using Assets.Scripts.Feature.Bomberman.Unit;
using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Managers;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanManager : MonoBehaviour
    {
        public enum GameState
        {
            Init,
            Play,

        }
        private GameState gameState;

        private BombermanCameraController camCtrl;
        private BombermanMapController mapCtrl;
        private PlayerController player;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            gameState = GameState.Init;
        }

        // Update is called once per frame
        void Update()
        {
            switch (gameState)
            {
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
                        if (Input.GetKeyDown(KeyCode.F1))
                        {
                            if (player == null)
                            {
                                MakePlayer();
                            }
                        }
                        break;
                    }
            }
        }

        #endregion

        private void SetGameState(GameState gameState)
        {
            this.gameState = gameState;
        }

        private void MakePlayer()
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
}