using Assets.Scripts.Feature.Main.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Assets.Scripts.Feature.GenSample
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController _instance;
        private PlayerController _playerContoller;

        private void Start()
        {
            _instance = this;
        }

        private void Update()
        {

        }

        public void SetOwner(PlayerController player)
        {
            if (_playerContoller != null)
                _playerContoller.OnChangePosition -= UnitController_OnChnagePosition;

            _playerContoller = player;

            _playerContoller.OnChangePosition += UnitController_OnChnagePosition;
        }

        private void UnitController_OnChnagePosition(Vector3 position)
        {
            transform.position = position;
        }

        public static CameraController Instance => _instance;
    }
}