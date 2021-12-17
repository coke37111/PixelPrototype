using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.System
{
    public class RoomSettings : MonoBehaviour
    {
        public static string roomName;
        public static byte maxPlayers;
        public static bool isMaster;

        public static bool ExistPrevRoom()
        {
            return !string.IsNullOrEmpty(roomName);
        }
    }
}