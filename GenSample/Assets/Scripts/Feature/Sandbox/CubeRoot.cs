using Assets.Scripts.Managers;
using UnityEngine;
using static Assets.Scripts.Feature.Sandbox.Cube.CubeBase;

namespace Assets.Scripts.Feature.Sandbox
{
    public class CubeRoot : MonoBehaviour
    {
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

        public virtual void Init(CUBE_TYPE cubeType)
        {
            GameObject pfRealCube = ResourceManager.LoadAsset<GameObject>($"Prefab/Sandbox/Cube/{cubeType}");
            GameObject goRealCube = Instantiate(pfRealCube, transform.position, Quaternion.identity, transform);
            goRealCube.GetComponent<BoxCollider>().isTrigger = false;
        }
    }
}