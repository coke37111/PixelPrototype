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
            string cubeName = info.photonView.InstantiationData[0].ToString();

            base.Init(cubeName);
        }

        #endregion
    }
}