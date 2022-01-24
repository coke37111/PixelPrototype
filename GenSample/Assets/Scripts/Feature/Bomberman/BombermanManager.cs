using Assets.Scripts.Feature.Bomberman.Unit;
using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Managers;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanManager : MonoBehaviour
    {
        private BombermanCameraController camCtrl;
        private PlayerController player;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            camCtrl = FindObjectOfType<BombermanCameraController>();

            MakePlayer();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if(player == null)
                {
                    MakePlayer();
                }
            }
        }

        #endregion

        private void MakePlayer()
        {
            GameObject pfPlayer = ResourceManager.LoadAsset<GameObject>($"Prefab/BomberMan/Player");
            if (pfPlayer != null)
            {
                Transform unitContainer = FindObjectOfType<UnitContainer>().transform;
                GameObject goPlayer = Instantiate(pfPlayer, unitContainer);
                player = goPlayer.GetComponent<PlayerController>();

                if (camCtrl != null)
                    camCtrl.SetTarget(goPlayer.transform);
            }
        }
    }
}