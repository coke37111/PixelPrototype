using Assets.Scripts.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public abstract class CubeBase : MonoBehaviour
    {
        public enum CUBE_TYPE
        {
            None,
            Ground,
            Ice,
            Damage,
        }

        [SerializeField]
        private CUBE_TYPE cubeType = CUBE_TYPE.None;

        private bool isGuide;
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

        public void SetGuide(bool flag)
        {
            isGuide = flag;
            GetComponent<BoxCollider>().isTrigger = isGuide;
        }

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

            GameObject pfRealCube = ResourceManager.LoadAsset<GameObject>($"Prefab/Sandbox/Cube/{cubeType}Cube");
            GameObject goRealCube = Instantiate(pfRealCube, GetPosition(), Quaternion.identity, parent);
            goRealCube.GetComponent<BoxCollider>().isTrigger = false;
        }

        private bool CanMakeRealCube()
        {
            return collObjs.Count <= 0;
        }

        public CUBE_TYPE GetCubeType()
        {
            return cubeType;
        }
    }
}