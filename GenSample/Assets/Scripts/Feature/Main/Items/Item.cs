using Assets.Scripts.Feature.Main.Player;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.Main.Items
{
    public class Item : MonoBehaviour, IPunInstantiateMagicCallback, IOnEventCallback
    {
        public enum ITEM_TYPE
        {
            power,
            range
        }
        public ITEM_TYPE itemType = ITEM_TYPE.power;

        private PhotonView photonView;

        private bool isInitialized = false;
        private bool isDestroying = false;        

        #region UNITY

        // Use this for initialization
        void Start()
        {
            if (PlayerSettings.IsConnectNetwork())
                return;

            Init();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized)
                return;

            if(other.tag == "Player")
            {
                switch (itemType)
                {
                    case ITEM_TYPE.power:
                        {
                            other.GetComponent<PlayerController>().BombPowerLevelup(1);
                            break;
                        }
                    case ITEM_TYPE.range:
                        {
                            other.GetComponent<PlayerController>().BombRangeLevelup(1);
                            break;
                        }
                }

                DestroyItem();
            }
        }

        public void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        #endregion

        #region PUN_METHOD

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Init();
        }

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;
            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[] : null;

            switch (eventCodeType)
            {
                case EventCodeType.DestroyItem:
                    {
                        int senderViewId = (int)data[0];
                        if (photonView.ViewID != senderViewId)
                            return;

                        if (!photonView.IsMine)
                            return;

                        PhotonNetwork.Destroy(photonView);
                        break;
                    }
            }
        }

        #endregion

        private void Init()
        {
            photonView = GetComponent<PhotonView>();
            isInitialized = true;
            isDestroying = false;
        }

        private void DestroyItem()
        {
            if (isDestroying)
                return;
            isDestroying = true;

            if (PlayerSettings.IsConnectNetwork())
            {
                PhotonEventManager.RaiseEvent(EventCodeType.DestroyItem, Photon.Realtime.ReceiverGroup.All, new object[]
                {
                    photonView.ViewID
                });
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}