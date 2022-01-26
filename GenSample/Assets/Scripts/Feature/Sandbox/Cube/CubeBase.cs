using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public abstract class CubeBase : MonoBehaviour
    {
        public enum CUBE_TYPE
        {
            ClimbCube,
            DamageCube,
            GroundCube,
            IceCube,
            JumpCube,
            BreakCube,
            BombCube,
        }

        protected abstract CUBE_TYPE cubeType
        {
            get;
        }

        private bool isGuide;
        private List<Collider> collObjs = new List<Collider>();

        protected CubeRoot cubeRoot;

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

        public virtual void SetGuide(bool flag)
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

        public void MakeRealCube(string cubeName)
        {
            if (!CanMakeRealCube())
                return;

            if (PlayerSettings.IsConnectNetwork())
            {
                PhotonNetwork.Instantiate($"Prefab/Sandbox/NetworkCube", GetPosition(), Quaternion.identity, 0, new object[] { cubeName });
            }
            else
            {
                Transform parent = FindObjectOfType<CubeContainer>().transform;
                GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>($"Prefab/Sandbox/LocalCube");
                GameObject goCubeRoot = Instantiate(pfCubeRoot, GetPosition(), Quaternion.identity, parent);
                cubeRoot = goCubeRoot.GetComponent<CubeRoot>();
                cubeRoot.Init(cubeName);
            }
        }

        private bool CanMakeRealCube()
        {
            return collObjs.Count <= 0;
        }

        public CUBE_TYPE GetCubeType()
        {
            return cubeType;
        }

        public void ClearCollObjs()
        {
            collObjs.Clear();
        }

        public void SetCubeRoot(CubeRoot cubeRoot)
        {
            this.cubeRoot = cubeRoot;
        }
        
        public void DestroyCube()
        {
            if (cubeRoot == null)
                return;

            cubeRoot.DestroyCube();
        }
    }
}