using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.Main.Block
{
    public class NormalBlock : BomberManBlock, IPunInstantiateMagicCallback, IOnEventCallback
    {
        private BombermanMapController mapCtrl;
        private BombermanManager manager;
        private PhotonView photonView;
        private Animator anim;

        private bool isExplosion = false;

        #region UNITY

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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
                case EventCodeType.DestroyNormalBlock:
                    {
                        int senderViewId = (int)data[0];
                        Log.Print(senderViewId, photonView.ViewID, photonView.IsMine);
                        if (photonView.ViewID != senderViewId)
                            return;

                        mapCtrl.UnregisterBlock(this);

                        if(photonView.IsMine)
                            PhotonNetwork.Destroy(photonView);
                        break;
                    }
            }
        }

        #endregion

        public override void Init()
        {
            base.Init();

            manager = FindObjectOfType<BombermanManager>();
            photonView = GetComponent<PhotonView>();
            anim = GetComponent<Animator>();

            Transform unitContainer = FindObjectOfType<BomberManObjectContainer>().transform;
            transform.SetParent(unitContainer);

            SetMapCtrl(FindObjectOfType<BombermanMapController>());
            mapCtrl.RegisterBlock(this);

            isExplosion = false;
            canExplosion = true;
        }

        public void SetMapCtrl(BombermanMapController mapCtrl)
        {
            this.mapCtrl = mapCtrl;
        }

        public override void Explosion()
        {
            base.Explosion();

            if (manager.IsEndGame())
                return;

            if (isExplosion)
                return;

            isExplosion = true;

            anim.SetTrigger("isExplosion");
        }

        public void EndDestroyAnim()
        {
            if (PlayerSettings.IsConnectNetwork())
            {
                PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.DestroyNormalBlock, ReceiverGroup.All, new object[]
                {
                    photonView.ViewID
                });
            }
            else
            {
                mapCtrl.UnregisterBlock(this);
                Destroy(gameObject);
            }
        }
    }
}