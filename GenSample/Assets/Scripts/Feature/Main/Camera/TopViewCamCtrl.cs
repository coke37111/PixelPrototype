using UnityEngine;

namespace Assets.Scripts.Feature.Main.Camera
{
    public class TopViewCamCtrl : MonoBehaviour
    {
        [Header("- target과의 거리설정값")]
        public float dist = 20f;
        public float height = 20f;

        private Transform target;
        private Vector3 distToTarget;

        #region UNITY

        // Update is called once per frame
        void Update()
        {
            if (target == null)
                return;

            transform.position = target.position - new Vector3(0f, -height, dist);
        }

        #endregion

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public void ResetPos()
        {
            target = null;
            transform.position = Vector3.zero - distToTarget;
        }
    }
}