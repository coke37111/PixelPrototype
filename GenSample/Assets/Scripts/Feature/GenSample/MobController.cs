using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Managers.GenSampleManager;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Assets.Scripts.Feature.GenSample
{
    public class MobController : MonoBehaviour, IOnEventCallback
    {
        public HpBar hpbar;

        public float hp = 10000;
        private float curHp;

        private GameObject hitEffect;
        private GameObject hitEffect2;
        private GameObject critHitEffect;

        #region UNITY


        // Use this for initialization
        void Start()
        {
            curHp = hp;
            SetGauge();
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

        #region PUN_CALLBACKS

        public void OnEvent(EventData photonEvent)
        {
            EventCodeType eventCodeType = (EventCodeType)photonEvent.Code;
            object[] data = (photonEvent.CustomData != null) ? photonEvent.CustomData as object[] : null;

            switch (eventCodeType)
            {
                case EventCodeType.MobAttackBy:
                    {
                        float damage = (float)data[0];

                        curHp -= damage;
                        if (curHp <= 0f)
                        {
                            curHp = 0f;

                            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                            SendOptions sendOptions = new SendOptions { Reliability = true };
                            PhotonNetwork.RaiseEvent((byte)EventCodeType.MobDie, null, raiseEventOptions, sendOptions);

                            Die();
                        }
                        SetGauge();

                        MakeHitEffect();
                        break;
                    }
            }
        }

        #endregion

        public void AttackBy(UnitController unit)
        {
            if (PhotonNetwork.IsConnected)
            {
                List<object> content = new List<object>() { unit.atk };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)EventCodeType.MobAttackBy, content.ToArray(), raiseEventOptions, sendOptions);
            }
            else
            {
                curHp -= unit.atk;
                if (curHp <= 0f)
                    curHp = 0f;

                SetGauge();
                MakeHitEffect();
            }            
        }

        private void SetGauge()
        {
            float hpRatio = curHp / hp;
            hpbar.SetGauge(hpRatio);
        }

        private void Die()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }

        public void MakeHitEffect()
        {
            string effPath = $"Prefab/Effect/";

            bool isCrit = Random.Range(0f, 1f) >= .7f;
            if (isCrit)
            {
                effPath += "damage_critical";

                if(critHitEffect == null)
                {
                    GameObject pfCritHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                    critHitEffect = Instantiate(pfCritHitEff, transform);                    
                }

                critHitEffect.transform.localPosition = new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), 0f);
                critHitEffect.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                if(Random.Range(0f, 1f) > .5f)
                {
                    effPath += "damage_001";

                    if (hitEffect == null)
                    {
                        GameObject pfHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                        hitEffect = Instantiate(pfHitEff, transform);
                    }

                    hitEffect.transform.localPosition = new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), 0f);
                    hitEffect.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    effPath += "damage_002";

                    if (hitEffect2 == null)
                    {
                        GameObject pfHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                        hitEffect2 = Instantiate(pfHitEff, transform);
                    }

                    hitEffect2.transform.localPosition = new Vector3(Random.Range(-.2f, .2f), Random.Range(-.2f, .2f), 0f);
                    hitEffect2.GetComponent<ParticleSystem>().Play();
                }
            }
        }
    }
}