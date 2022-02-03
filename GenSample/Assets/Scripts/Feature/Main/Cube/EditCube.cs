using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.nsCube
{
    public class EditCube : MonoBehaviour
    {
        private string cubeId;
        private List<Collider> collObjs = new List<Collider>();

        #region UNITY

        private void OnTriggerEnter(Collider other)
        {
            if (!collObjs.Contains(other))
                collObjs.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (collObjs.Contains(other))
                collObjs.Remove(other);
        }

        #endregion

        public void Build(string cubeId)
        {
            this.cubeId = cubeId;

            GameObject pfRealCube = ResourceManager.LoadAsset<GameObject>($"Prefab/Main/Cube/{cubeId}");
            GameObject goRealCube = Instantiate(pfRealCube, transform.position, Quaternion.identity, transform);

            Cube realCube = goRealCube.GetComponent<Cube>();
            realCube.SetDestroyCallback(DestroyCube);
        }

        public string GetCubeId()
        {
            return cubeId;
        }

        public void MakeRealCube()
        {
            if (!CanMakeRealCube())
                return;

            Transform parent = FindObjectOfType<CubeContainer>().transform;
            GameObject pfCubeRoot = ResourceManager.LoadAsset<GameObject>(PrefabPath.EditCubePath);
            GameObject goCubeRoot = Instantiate(pfCubeRoot, GetPosition(), Quaternion.identity, parent);
            EditCube cubeRoot = goCubeRoot.GetComponent<EditCube>();
            cubeRoot.Build(cubeId);
        }

        private bool CanMakeRealCube()
        {
            return collObjs.Count <= 0;
        }

        public void ClearCollObjs()
        {
            collObjs.Clear();
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void DestroyCube()
        {
            Destroy(gameObject);
        }
    }
}