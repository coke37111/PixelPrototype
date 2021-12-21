using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectF
{
    public class PopupChatting : UiBase
    {
        public TMPro.TextMeshProUGUI _chatField;
        public Button _closeButton;
        public TMPro.TMP_InputField _inputField;
        public Button _sendButton;

        public override void OnOpen(object parameter)
        {
            Instance_OnUpdateChat(PhotonNetwork.CurrentRoom.Name, ChatHandler.Instance.GetChat(PhotonNetwork.CurrentRoom.Name));
        }

        private void OnEnable()
        {
            AddClickEvent(_closeButton, OnClickClose);
            AddClickEvent(_sendButton, OnClickSend);
        }

        protected override void OnClose()
        {
        }

        private void Awake()
        {
            ChatHandler.Instance.OnUpdateChat += Instance_OnUpdateChat;
        }

        private void Instance_OnUpdateChat(string channelName, IEnumerable<string> chats)
        {
            if (chats.Any() == false)
                return;

            string temp = "";

            foreach (var chat in chats)
            {
                temp += chat;
                temp += "\n";
            }

            _chatField.text = temp;

            _inputField.Select();
            _inputField.ActivateInputField();
        }

        public void OnClickClose()
        {
            this.gameObject.SetActive(false);
            //UiManager.Instance.Close<PopupChatting>();
        }

        public void OnClickSend()
        {
            ChatHandler.Instance.SendChat(PhotonNetwork.CurrentRoom.Name, _inputField.text);
            _inputField.text = "";
        }

        private void Update()
        {
            if ((Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) && (string.IsNullOrEmpty(_inputField.text) == false))
            {
                OnClickSend();
            }

            if (Input.GetKey(KeyCode.Escape))
                OnClickClose();
        }
    }
}