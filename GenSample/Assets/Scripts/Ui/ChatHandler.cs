using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Chat.Demo;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatHandler : MonoBehaviourPunCallbacks, IChatClientListener
{
    private static ChatHandler _instance;
    private ChatClient _chatClient;
    protected internal ChatAppSettings _chatAppSettings;

    public static ChatHandler Instance => _instance;
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        if (_chatClient == null)
        {
#if PHOTON_UNITY_NETWORKING
            _chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
#endif

            _chatClient = new ChatClient(this);
#if !UNITY_WEBGL
            _chatClient.UseBackgroundWorkerForSending = true;
#endif
            //_chatClient.AuthValues = new Photon.Chat.AuthenticationValues(My.Nickname);

            if (_chatClient.ConnectUsingSettings(_chatAppSettings) == false)
                throw new Exception("Chat Connect Failed");
        }
    }
    public IEnumerable<string> GetChat(string channelName)
    {
        ChatChannel chatChannel = null;
        if (_chatClient.TryGetChannel(channelName, out chatChannel))
        {
            for (int i = 0; i < chatChannel.MessageCount; ++i)
            {
                var sender = chatChannel.Senders[i];
                var msg = chatChannel.Messages[i];

                yield return $"{sender}:{msg}";
            }
        }
    }

    public void OnDestroy()
    {
        if (_chatClient != null)
        {
            _chatClient.Disconnect();
        }
    }

    public void OnApplicationQuit()
    {
        //PhotonNetwork.LeaveRoom();
        //PhotonNetwork.LeaveLobby();

        if (_chatClient != null)
            _chatClient.Disconnect();

        //PhotonNetwork.Disconnect();
    }

    public void SendChat(string channel, string chat)
    {
        if (_chatClient == null)
            return;

        if (_chatClient.PublishMessage(channel, chat) == false)
            throw new Exception("chat send failed");
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnChatStateChange(ChatState state)
    {
    }

    public void OnDisconnected()
    {
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
    }

    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }

    private void Update()
    {
        if (_chatClient != null)
            _chatClient.Service();
    }
}
