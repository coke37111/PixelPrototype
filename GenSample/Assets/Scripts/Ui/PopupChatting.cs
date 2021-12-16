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

            //Instance_OnUpdateChat("chat_test", NetworkManager.Instance.GetChat("chat_test"));
        }

        private void OnEnable()
        {
            AddClickEvent(_closeButton, OnClickClose);
            AddClickEvent(_sendButton, OnClickSender);
        }

        protected override void OnClose()
        {
        }

        private void Awake()
        {
            //NetworkManager.Instance.OnUpdateChat += Instance_OnUpdateChat;
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

        private void OnClickClose()
        {
            //UiManager.Instance.Close<PopupChatting>();
        }

        private void OnClickSender()
        {
            //NetworkManager.Instance.SendChat("chat_test", _inputField.text);
            _inputField.text = "";
        }

        private void Update()
        {
            if ((Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)) && (string.IsNullOrEmpty(_inputField.text) == false))
            {
                OnClickSender();
            }

            if (Input.GetKey(KeyCode.Escape))
                OnClickClose();
        }
    }
}