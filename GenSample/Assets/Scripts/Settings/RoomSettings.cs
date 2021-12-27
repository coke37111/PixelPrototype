using Photon.Realtime;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public class RoomSettings : MonoBehaviour
    {
        public static string roomName;
        public static byte maxPlayers;
        public static bool isMaster;

        public const string GAME_END = "IsGameEnd";
        public const string CountdownStartTime = "GenCountdownStartTime";

        public static bool ExistPrevRoom()
        {
            return !string.IsNullOrEmpty(roomName);
        }
    }
}