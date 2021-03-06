using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public class RoomSettings : MonoBehaviour
    {
        public enum ROOM_TYPE
        {
            Bomberman,
            Raid,
            Pvp,
            //Sandbox,
            Template,
        }

        public static ROOM_TYPE roomType;
        public static string roomName;
        public static byte maxPlayers;
        public static bool isMaster;

        public const string RoomTypeKey = "GenSampleRoomType";
        public const string CountdownStartTime = "GenCountdownStartTime";
        public const string StartRoom = "StartRoom";

        public static bool ExistPrevRoom()
        {
            return !string.IsNullOrEmpty(roomName);
        }
    }
}