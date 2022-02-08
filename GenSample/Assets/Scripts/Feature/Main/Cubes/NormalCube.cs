using Assets.Scripts.Settings;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class NormalCube : Cube
    {
        private Animator anim;
        private bool isExplosion;
        private bool isInitialized = false;

        #region UNITY

        private void Start()
        {
            Init();
        }

        #endregion

        private void Init()
        {
            anim = GetComponent<Animator>();
            isExplosion = false;
            isInitialized = true;
        }

        public override void Explosion()
        {
            base.Explosion();

            if (!isInitialized)
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
                //PhotonEventManager.RaiseEvent(PlayerSettings.EventCodeType.DestroyNormalBlock, ReceiverGroup.All, new object[]
                //{
                //    photonView.ViewID
                //});
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}