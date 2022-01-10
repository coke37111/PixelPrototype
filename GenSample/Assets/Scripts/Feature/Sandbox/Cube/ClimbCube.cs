using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class ClimbCube : CubeBase
    {
        public GameObject roofColl;
        public GameObject bottomColl;
        public GameObject floorColl;

        [SerializeField]
        private LayerMask cubeLayer;

        protected override CUBE_TYPE cubeType => CUBE_TYPE.Climb;

        #region UNITY

        private void Start()
        {
            Vector3 rayOrg = transform.position + GetComponent<BoxCollider>().center;
            Vector3 rayDir = Vector3.down;
            float rayDist = .2f;

            RaycastHit hit;
            if (Physics.Raycast(rayOrg, rayDir, out hit, rayDist, cubeLayer))
            {
                if (hit.collider.tag == "Cube")
                {
                    CubeBase collCube = hit.collider.GetComponent<CubeBase>();
                    if(collCube.GetCubeType() == CUBE_TYPE.Climb)
                    {
                        collCube.GetComponent<ClimbCube>().roofColl.SetActive(false);
                        bottomColl.SetActive(false);
                        floorColl.SetActive(false);
                    }
                }
            }
        }

        #endregion
    }
}