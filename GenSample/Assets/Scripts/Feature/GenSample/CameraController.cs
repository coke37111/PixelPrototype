using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Assets.Scripts.Feature.GenSample
{
    public class CameraController : MonoBehaviour
    {
        private static CameraController _instance;
        private UnitLocalPlayer _unitLocalPlayer;

        private void Start()
        {
            _instance = this;
        }

        private void Update()
        {

        }

        public void SetOwner(UnitLocalPlayer unitLocalPlayer)
        {
            if (_unitLocalPlayer != null)
                _unitLocalPlayer.OnChnagePosition -= UnitController_OnChnagePosition;

            _unitLocalPlayer = unitLocalPlayer;

            _unitLocalPlayer.OnChnagePosition += UnitController_OnChnagePosition;
        }

        private void UnitController_OnChnagePosition(Vector3 position)
        {
            transform.position = position;
        }

        public static CameraController Instance => _instance;
    }
}