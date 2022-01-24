using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman
{
    public class BombermanCameraController : MonoBehaviour
    {
        private Transform target;
        private Vector3 distToTarget;

        #region UNITY

        // Use this for initialization
        void Start()
        {

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
            distToTarget = target.position - transform.position;
        }
    }
}