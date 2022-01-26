﻿using Assets.Scripts.Feature.Sandbox.Cube;
using Assets.Scripts.Managers;
using UnityEngine;
using static Assets.Scripts.Feature.Sandbox.Cube.CubeBase;

namespace Assets.Scripts.Feature.Sandbox
{
    public class CubeRoot : MonoBehaviour
    {
        private CubeBase cubeBase;

        #region UNITY

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        public virtual void Init(string cubeName)
        {
            GameObject pfRealCube = ResourceManager.LoadAsset<GameObject>($"Prefab/Sandbox/Cube/{cubeName}");
            GameObject goRealCube = Instantiate(pfRealCube, transform.position, Quaternion.identity, transform);
            goRealCube.GetComponent<BoxCollider>().isTrigger = false;

            cubeBase = goRealCube.GetComponent<CubeBase>();
            cubeBase.SetCubeRoot(this);
        }

        public virtual void DestroyCube()
        {
            Destroy(gameObject);
        }

        public CubeBase GetCubeBase()
        {
            return cubeBase;
        }
    }
}