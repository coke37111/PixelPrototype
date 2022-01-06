using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox
{
    public class ShowCube : MonoBehaviour
    {
        private List<Collider> collObjs = new List<Collider>();

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag != "Cube")
            {
                if (!collObjs.Contains(other))
                    collObjs.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag != "Cube")
            {
                if (collObjs.Contains(other))
                    collObjs.Remove(other);
            }
        }

        #endregion

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void MakeRealCube(Transform parent)
        {
            if (!CanMakeRealCube())
                return;

            GameObject pfRealCube = ResourceManager.LoadAsset<GameObject>("Prefab/Sandbox/RealCube");
            Instantiate(pfRealCube, GetPosition(), Quaternion.identity, parent);
        }

        private bool CanMakeRealCube()
        {
            return collObjs.Count <= 0;
        }
    }
}