using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanCameraController : MonoBehaviour
    {
        [Header("- target과의 거리설정값")]
        public float dist = 20f;
        public float height = 20f;

        private Transform target;
        private Vector3 distToTarget;

        #region UNITY

        // Use this for initialization
        void Start()
        {
            distToTarget = new Vector3(0f, -height, dist);
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null)
                return;

            transform.position = target.position - distToTarget;
        }

        #endregion

        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}