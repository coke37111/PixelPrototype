using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.PhotonSample
{
    public class DummyRoomListEntry : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI RoomNameText;
        public TMPro.TextMeshProUGUI RoomPlayersText;
        public Button JoinRoomButton;

        private string roomName;

        public void Start()
        {
            JoinRoomButton.onClick.AddListener(() =>
            {
                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.LeaveLobby();
                }

                PhotonNetwork.JoinRoom(roomName);
            });
        }

        public void Initialize(string name, byte currentPlayers, byte maxPlayers)
        {
            roomName = name;

            RoomNameText.text = name;
            RoomPlayersText.text = currentPlayers + " / " + maxPlayers;
        }
    }
}