using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public class RoomSettings : MonoBehaviour
    {
        public enum ROOM_TYPE
        {
            Raid,
            Pvp,
        }

        public static ROOM_TYPE roomType;
        public static string roomName;
        public static byte maxPlayers;
        public static bool isMaster;

        public const string RoomTypeKey = "GenSampleRoomType";
        public const string CountdownStartTime = "GenCountdownStartTime";

        public static bool ExistPrevRoom()
        {
            return !string.IsNullOrEmpty(roomName);
        }
    }
}