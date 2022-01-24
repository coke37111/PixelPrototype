using Assets.Scripts.Feature.Bomberman.Unit;
using Assets.Scripts.Managers;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanManager : MonoBehaviour
    {
        private BombermanCameraController camCtrl;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            camCtrl = FindObjectOfType<BombermanCameraController>();
            if (camCtrl != null)
                camCtrl.SetTarget(FindObjectOfType<PlayerController>().transform);
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion
    }
}