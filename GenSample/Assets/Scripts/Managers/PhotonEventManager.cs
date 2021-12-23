using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Managers
{
    public static class PhotonEventManager
    {
        public static void RaiseEvent(EventCodeType eventCodeType, ReceiverGroup receiveGroup, params object[] objs)
        {
            List<object> content = new List<object>();
            content.AddRange(objs);

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = receiveGroup };
            SendOptions sendOptions = new SendOptions { Reliability = true };
            PhotonNetwork.RaiseEvent((byte)eventCodeType, content.ToArray(), raiseEventOptions, sendOptions);
        }
    }
}