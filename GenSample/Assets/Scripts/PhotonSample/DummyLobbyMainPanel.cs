using Assets.Scripts.PhotonSample;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids
{
    public class DummyLobbyMainPanel : MonoBehaviourPunCallbacks
    {
        [Header("Login Panel")]
        public GameObject LoginPanel;

        public TMPro.TMP_InputField PlayerNameInput;

        [Header("Selection Panel")]
        public GameObject SelectionPanel;

        [Header("Create Room Panel")]
        public GameObject CreateRoomPanel;

        public TMPro.TMP_InputField RoomNameInputField;
        public TMPro.TMP_InputField MaxPlayersInputField;

        [Header("Join Random Room Panel")]
        public GameObject JoinRandomRoomPanel;

        [Header("Room List Panel")]
        public GameObject RoomListPanel;

        public GameObject RoomListContent;
        public GameObject RoomListEntryPrefab;

        [Header("Inside Room Panel")]
        public GameObject InsideRoomPanel;

        public Button StartGameButton;
        public GameObject PlayerListEntryPrefab;
        public TMPro.TextMeshProUGUI RoomTypeText;

        private Dictionary<string, RoomInfo> cachedRoomList;
        private Dictionary<string, GameObject> roomListEntries;
        private Dictionary<int, GameObject> playerListEntries;


        public string GameSceneName;

        #region UNITY

        public void Awake()
        {
            cachedRoomList = new Dictionary<string, RoomInfo>();
            roomListEntries = new Dictionary<string, GameObject>();

            if (PhotonNetwork.IsConnected)
            {
                return;
            }                

            PhotonNetwork.AutomaticallySyncScene = true;
            
            PlayerNameInput.text = PlayerPrefs.GetString("PlayerName", "Player " + Random.Range(1000, 10000));

            Log.Print($"DummyLobbyMainPanel Awake!");
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnConnectedToMaster()
        {
            Log.Print($"OnConnectedToMaster!");

            PlayerSettings.ConnectNetwork();

            if (RoomSettings.ExistPrevRoom())
            {
                string roomName = RoomSettings.roomName;
                Log.Print($"isMaster = {RoomSettings.isMaster}");

                if (RoomSettings.isMaster)
                {
                    RoomOptions options = new RoomOptions { MaxPlayers = RoomSettings.maxPlayers, PlayerTtl = 10000 };
                    PhotonNetwork.CreateRoom(roomName, options, null);
                }
                else
                {
                    PhotonNetwork.JoinRoom(roomName);
                }
                return;
            }
            
            this.SetActivePanel(SelectionPanel.name);
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            ClearRoomListView();

            UpdateCachedRoomList(roomList);
            UpdateRoomListView();
        }

        public override void OnJoinedLobby()
        {
            // whenever this joins a new lobby, clear any previous room lists
            cachedRoomList.Clear();
            ClearRoomListView();
        }

        // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
        public override void OnLeftLobby()
        {
            cachedRoomList.Clear();
            ClearRoomListView();
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            if (RoomSettings.ExistPrevRoom() && RoomSettings.isMaster)
            {
                string roomName = RoomSettings.roomName;
                RoomOptions options = new RoomOptions { MaxPlayers = RoomSettings.maxPlayers, PlayerTtl = 10000 };
                PhotonNetwork.CreateRoom(roomName, options, null);
                return;
            }

            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {            
            if (RoomSettings.ExistPrevRoom() && !RoomSettings.isMaster)
            {
                string roomName = RoomSettings.roomName;
                PhotonNetwork.JoinRoom(roomName);
                return;
            }

            SetActivePanel(SelectionPanel.name);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            string roomName = "Room " + Random.Range(1000, 10000);

            RoomOptions options = new RoomOptions {MaxPlayers = 8};

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public override void OnJoinedRoom()
        {
            RoomSettings.roomName = null;
            RoomSettings.isMaster = false;

            // joining (or entering) a room invalidates any cached lobby room list (even if LeaveLobby was not called due to just joining a room)
            cachedRoomList.Clear();


            SetActivePanel(InsideRoomPanel.name);

            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerListEntryPrefab);
                entry.transform.SetParent(InsideRoomPanel.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<DummyPlayerListEntry>().Initialize(p.ActorNumber, p.NickName);

                object isPlayerReady;
                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out isPlayerReady))
                {
                    entry.GetComponent<DummyPlayerListEntry>().SetPlayerReady((bool) isPlayerReady);
                }

                playerListEntries.Add(p.ActorNumber, entry);
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());

            Hashtable props = new Hashtable
            {
                {PlayerSettings.PLAYER_LOADED_LEVEL, false },
                {PlayerSettings.PLAYER_DIE, false },
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);

            object roomType;
            if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomSettings.RoomTypeKey, out roomType))
            {
                RoomSettings.roomType = (RoomSettings.ROOM_TYPE)roomType;
                RoomTypeText.text = RoomSettings.roomType.ToString();
            }
            else
            {
                Hashtable roomProps = new Hashtable
                {
                    {RoomSettings.RoomTypeKey, RoomSettings.roomType}
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
            }
        }

        public override void OnLeftRoom()
        {
            SetActivePanel(SelectionPanel.name);

            foreach (GameObject entry in playerListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            playerListEntries.Clear();
            playerListEntries = null;
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(InsideRoomPanel.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<DummyPlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

            playerListEntries.Add(newPlayer.ActorNumber, entry);

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
            playerListEntries.Remove(otherPlayer.ActorNumber);

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
            {
                StartGameButton.gameObject.SetActive(CheckPlayersReady());
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            GameObject entry;
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
            {
                object isPlayerReady;
                if (changedProps.TryGetValue(AsteroidsGame.PLAYER_READY, out isPlayerReady))
                {
                    entry.GetComponent<DummyPlayerListEntry>().SetPlayerReady((bool) isPlayerReady);
                }
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            object roomType;
            if(propertiesThatChanged.TryGetValue(RoomSettings.RoomTypeKey, out roomType))
            {
                RoomSettings.roomType = (RoomSettings.ROOM_TYPE)roomType;
                RoomTypeText.text = RoomSettings.roomType.ToString();
            }
        }

        #endregion

        #region UI CALLBACKS

        public void OnBackButtonClicked()
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            SetActivePanel(SelectionPanel.name);
        }

        public void OnCreateRoomButtonClicked()
        {
            string roomName = RoomNameInputField.text;
            roomName = (roomName.Equals(string.Empty)) ? "Room " + Random.Range(1000, 10000) : roomName;

            byte maxPlayers;
            byte.TryParse(MaxPlayersInputField.text, out maxPlayers);
            maxPlayers = (byte) Mathf.Clamp(maxPlayers, 2, 8);

            RoomOptions options = new RoomOptions {MaxPlayers = maxPlayers, PlayerTtl = 10000 };

            PhotonNetwork.CreateRoom(roomName, options, null);
        }

        public void OnJoinRandomRoomButtonClicked()
        {
            SetActivePanel(JoinRandomRoomPanel.name);

            PhotonNetwork.JoinRandomRoom();            
        }

        public void OnLeaveGameButtonClicked()
        {
            PhotonNetwork.LeaveRoom();
        }

        public void OnLoginButtonClicked()
        {
            string playerName = PlayerNameInput.text;

            if (!playerName.Equals(""))
            {
                PlayerPrefs.SetString("PlayerName", playerName);
                PhotonNetwork.LocalPlayer.NickName = playerName;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                Debug.LogError("Player Name is invalid.");
            }
        }

        public void OnRoomListButtonClicked()
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            SetActivePanel(RoomListPanel.name);
        }

        public void OnStartGameButtonClicked()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            switch (RoomSettings.roomType)
            {
                case RoomSettings.ROOM_TYPE.Pvp:
                case RoomSettings.ROOM_TYPE.Raid:
                    {
                        PhotonNetwork.LoadLevel(GameSceneName);
                        break;
                    }
                case RoomSettings.ROOM_TYPE.Sandbox:
                    {
                        string SandBoxSceneName = "SandboxScene";
                        PhotonNetwork.LoadLevel(SandBoxSceneName);
                        break;
                    }
                case RoomSettings.ROOM_TYPE.Bomberman:
                    {
                        PhotonNetwork.LoadLevel("BombermanSceneNew");
                        break;
                    }
            }
        }

        public void ChangeGameTypeButtonClicked()
        {
            // TODO : 리팩토링 전까지 막아두기
            return;

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            int curRoomTypeIdx = (int)RoomSettings.roomType;
            int nextRoomTypeIdx = curRoomTypeIdx + 1;
            if(nextRoomTypeIdx >= System.Enum.GetValues(typeof(RoomSettings.ROOM_TYPE)).Length)
            {
                nextRoomTypeIdx = 0;
            }

            RoomSettings.ROOM_TYPE nextRoomType = (RoomSettings.ROOM_TYPE)nextRoomTypeIdx;

            Hashtable props = new Hashtable() { { RoomSettings.RoomTypeKey, nextRoomType } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }

        #endregion

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                object isPlayerReady;
                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out isPlayerReady))
                {
                    if (!(bool) isPlayerReady)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        
        private void ClearRoomListView()
        {
            foreach (GameObject entry in roomListEntries.Values)
            {
                Destroy(entry.gameObject);
            }

            roomListEntries.Clear();
        }

        public void LocalPlayerPropertiesUpdated()
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        private void SetActivePanel(string activePanel)
        {
            LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
            SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
            CreateRoomPanel.SetActive(activePanel.Equals(CreateRoomPanel.name));
            JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
            RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
        }

        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
                {
                    if (cachedRoomList.ContainsKey(info.Name))
                    {
                        cachedRoomList.Remove(info.Name);
                    }

                    continue;
                }

                // Update cached room info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }

        private void UpdateRoomListView()
        {
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomListEntryPrefab);
                entry.transform.SetParent(RoomListContent.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<DummyRoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

                roomListEntries.Add(info.Name, entry);
            }
        }
    }
}