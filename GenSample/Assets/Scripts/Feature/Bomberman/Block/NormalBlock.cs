using Assets.Scripts.Feature.GenSample;
using Assets.Scripts.Settings;
using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Bomberman.Block
{
    public class NormalBlock : BomberManBlock, IPunInstantiateMagicCallback
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

        #endregion

        #region PUN_METHOD

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            Init();
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
            mapCtrl.UnregisterBlock(this);

            if (PlayerSettings.IsConnectNetwork())
            {
                if (photonView.IsMine)
                    PhotonNetwork.Destroy(photonView);
            }
            else
                Destroy(gameObject);
        }
    }
}