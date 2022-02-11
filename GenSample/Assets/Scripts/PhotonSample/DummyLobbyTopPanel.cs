using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.PhotonSample
{
    public class DummyLobbyTopPanel : MonoBehaviour
    {

        private readonly string connectionStatusMessage = "Connection Status: ";

        [Header("UI References")]
        public TMPro.TextMeshProUGUI ConnectionStatusText;

        #region UNITY

        public void Update()
        {
            ConnectionStatusText.text = connectionStatusMessage + PhotonNetwork.NetworkClientState;
        }

        #endregion
    }
}