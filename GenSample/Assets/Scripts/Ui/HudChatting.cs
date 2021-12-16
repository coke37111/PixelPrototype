using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectF
{
    public class HudChatting : UiBase
    {
        public GameObject _popupChat;
        public Button _areaButton;
        public TMPro.TextMeshProUGUI _chatNew;
        public TMPro.TextMeshProUGUI _chatOld;

        public override void OnOpen(object parameter)
        {
            _chatNew.text = "";
            _chatOld.text = "";
                        
            Instance_OnUpdateChat("chat_test", ChatHandler.Instance.GetChat("chat_test"));
        }

        public void SetText(string text)
        {
            ChatHandler.Instance.SendChat("chat_test", text);
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
            _chatNew.text = "";
            _chatOld.text = "";

            if (chats.Any())
            {
                var newMsg = chats.Last();
                var oldMag = chats.LastOrDefault(t => t != newMsg);

                _chatNew.text = newMsg;
                if (string.IsNullOrEmpty(oldMag) == false)
                    _chatOld.text = oldMag;
            }
        }

        private void OnAreaClick()
        {
            _popupChat.SetActive(true);
        }

        private void Start()
        {
            AddClickEvent(_areaButton, OnAreaClick);
        }
    }
}