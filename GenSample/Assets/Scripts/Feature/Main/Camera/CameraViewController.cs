using Assets.Scripts.Settings.SO;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Camera
{
    public class CameraViewController : MonoBehaviour
    {        
        private CameraViewSettingSO setting;
        private Transform target;
        private UnityEngine.Camera viewCamera;

        #region UNITY

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (setting == null)
                return;

            Vector3 newPos = Vector3.zero;
            if (setting.cameraMoveType == CameraMoveType.FollowPlayer)
            {
                if (target == null)
                    return;

                newPos = target.position;
            }

            newPos.y += setting.height;
            newPos.z -= setting.dist;
            transform.position = newPos;

            transform.localRotation = Quaternion.Euler(setting.rotate);
            viewCamera.fieldOfView = setting.fieldOfView;
        }

        #endregion

        public void Init(CameraViewSettingSO setting)
        {
            this.setting = setting;
            viewCamera = GetComponent<UnityEngine.Camera>();
        }

        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}