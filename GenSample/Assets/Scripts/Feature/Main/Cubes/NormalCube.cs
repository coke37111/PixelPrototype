using Assets.Scripts.Settings;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.Main.Cubes
{
    public class NormalCube : Cube
    {
        public float hp = 10f;

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

        public override void Hit(float damage)
        {
            hp -= damage;
            if(hp <= 0f)
            {
                hp = 0f;
                Explosion();
            }    
        }

        public override void Explosion()
        {
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