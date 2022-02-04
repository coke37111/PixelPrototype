using Assets.Scripts.Util;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Sandbox.Cube
{
    public class ClimbCube : MonoBehaviour
    {
        //private readonly string FloorObjName = "Floor";
        //private readonly string BottomObjName = "Bottom";
        //private readonly string RoofObjName = "Roof";

        //private GameObject roofColl;
        //private GameObject bottomColl;
        //private GameObject floorColl;

        //public LayerMask cubeLayer;

        //protected override CUBE_TYPE cubeType => CUBE_TYPE.ClimbCube;

        //#region UNITY

        //private void Start()
        //{
        //    floorColl = transform.Find(FloorObjName).gameObject;
        //    bottomColl = transform.Find(BottomObjName).gameObject;
        //    roofColl = transform.Find(RoofObjName).gameObject;

        //    Vector3 rayOrg = transform.position + GetComponent<BoxCollider>().center;
        //    Vector3 rayDir = Vector3.down;
        //    float rayDist = .2f;

        //    Debug.DrawRay(rayOrg, rayDir * rayDist, Color.red);
        //    RaycastHit hit;
        //    if (Physics.Raycast(rayOrg, rayDir, out hit, rayDist, cubeLayer))
        //    {
        //        if (hit.collider.tag == "Cube")
        //        {
        //            CubeBase collCube = hit.collider.GetComponent<CubeBase>();
        //            if(collCube.GetCubeType() == CUBE_TYPE.ClimbCube)
        //            {
        //                collCube.GetComponent<ClimbCube>().roofColl.SetActive(false);
        //                bottomColl.SetActive(false);
        //                floorColl.SetActive(false);
        //            }
        //        }
        //    }
        //}

        //#endregion

        //public override void SetGuide(bool flag)
        //{
        //    base.SetGuide(flag);

        //    if (flag)
        //    {
        //        bottomColl = transform.Find(BottomObjName).gameObject;
        //        bottomColl.SetActive(false);
        //    }
        //}
    }
}