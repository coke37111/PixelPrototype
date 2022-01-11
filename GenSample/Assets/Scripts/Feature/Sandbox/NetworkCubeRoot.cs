using Photon.Pun;
using System.Collections;
using UnityEngine;
using static Assets.Scripts.Feature.Sandbox.Cube.CubeBase;

namespace Assets.Scripts.Feature.Sandbox
{
    public class NetworkCubeRoot : CubeRoot, IPunInstantiateMagicCallback
    {
        #region PUN_CALLBACKS

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            CUBE_TYPE cubeType = (CUBE_TYPE)info.photonView.InstantiationData[0];

            base.Init(cubeType);
        }

        #endregion
    }
}