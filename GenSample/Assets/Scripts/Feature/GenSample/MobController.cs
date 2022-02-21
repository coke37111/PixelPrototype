using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Settings.PlayerSettings;

namespace Assets.Scripts.Feature.GenSample
{
    public class MobController : MonoBehaviour, IOnEventCallback, IPunInstantiateMagicCallback
    {
        public HpBar hpbar;

        private MobSettingSO mobSetting;
        private float maxHp;
        private int enhanceHpCount = 1;

        private float curHp;
        private float curRegenHpDelay;

        private GameObject hitEffect;
        private GameObject hitEffect2;
        private GameObject critHitEffect;

        private float nextAtkDelay;
        private float curAtkDelay = 0f;

        private PhotonView photonView;

        #region UNITY

        void Update()
        {
            if (PlayerSettings.IsConnectNetwork() && !photonView.IsMine)
                return;

            if (mobSetting == null)
                return;

            if(curRegenHpDelay >= mobSetting.regenHpDelay)
            {
                curRegenHpDelay = 0f;

                if (curHp > 0)
                {
                    RegenHp();
                }
            }
            else
            {
                curRegenHpDelay += Time.deltaTime;
            }

            if(curAtkDelay >= nextAtkDelay)
            {
                curAtkDelay -= nextAtkDelay;

                if (PlayerSettings.IsConnectNetwork())
                {
                    photonView.RPC("AttackRPC", RpcTarget.AllViaServer, transform.position);
                }
                else
                {
                    Attack(transform.position);
                }

                nextAtkDelay = GetAtkDelay();                
            }
            else
            {
                curAtkDelay += Time.deltaTime;
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

        #region PUN_CALLBACKS

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            enhanceHpCount = (int)info.photonView.InstantiationData[0];

            Init();
        }

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

                            PhotonEventManager.RaiseEvent(EventCodeType.MobDie, ReceiverGroup.All);

                            GetComponent<Animator>().SetTrigger("mob_die_01");
                            SetGauge();
                            return;
                        }
                        SetGauge();

                        MakeHitEffect();
                        GetComponent<Animator>().SetTrigger("mob_hit_01");
                        break;
                    }
                case EventCodeType.MobRegenHp:
                    {
                        if (curHp >= maxHp)
                        {
                            return;
                        }

                        curHp += maxHp * mobSetting.regenHpRatio;

                        SetGauge();
                        break;
                    }
            }
        }

        [PunRPC]
        public void AttackRPC(Vector3 position, PhotonMessageInfo info)
        {
            float lag = (float)(PhotonNetwork.Time - info.SentServerTime);
            Attack(position);
        }

        #endregion

        public void Init()
        {
            mobSetting = ResourceManager.LoadAsset<MobSettingSO>(MobSettingSO.path);
            maxHp = enhanceHpCount * mobSetting.hp;

            curHp = maxHp;
            SetGauge();

            nextAtkDelay = GetAtkDelay();
            curAtkDelay = 0f;

            photonView = GetComponent<PhotonView>();
        }

        private float GetAtkDelay()
        {
            if (mobSetting == null)
                return 1f;

            return Random.Range(mobSetting.minAtkDelay, mobSetting.maxAtkDelay);
        }

        private void Attack(Vector3 pos)
        {
            Vector3 initPos = pos;
            initPos.x += Random.Range(-mobSetting.atkPos.x, mobSetting.atkPos.x);
            initPos.y = Random.Range(-mobSetting.atkPos.y, mobSetting.atkPos.y);
            initPos.z += Random.Range(-mobSetting.atkPos.z, mobSetting.atkPos.z);

            GameObject pfIndicator = ResourceManager.LoadAsset<GameObject>(PrefabPath.IndicatorPath);
            GameObject goIndicator = Instantiate(pfIndicator, initPos, Quaternion.identity, null);
            Indicator indicator = goIndicator.GetComponent<Indicator>();
            indicator.Init(mobSetting.atkLimitTime, mobSetting.atkScale.x, mobSetting.atkScale.y);
        }

        public void AttackBy(float damage)
        {
            if (curHp <= 0)
                return;

            if (PlayerSettings.IsConnectNetwork())
            {
                List<object> content = new List<object>() { damage };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)EventCodeType.MobAttackBy, content.ToArray(), raiseEventOptions, sendOptions);
            }
            else
            {
                curHp -= damage;
                if (curHp <= 0f)
                    curHp = 0f;

                SetGauge();
                MakeHitEffect();

                GetComponent<Animator>().SetTrigger("mob_hit_01");
            }
        }

        private void SetGauge()
        {
            float hpRatio = curHp / maxHp;
            hpbar.SetGauge(hpRatio);
        }

        // mob의 die anim에서 호출
        public void Die()
        {
            if (PlayerSettings.IsConnectNetwork())
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(gameObject);
                }

                PhotonEventManager.RaiseEvent(EventCodeType.Clear, ReceiverGroup.All);
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

        private void RegenHp()
        {
            if (PhotonNetwork.IsConnected)
            {
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                SendOptions sendOptions = new SendOptions { Reliability = true };
                PhotonNetwork.RaiseEvent((byte)EventCodeType.MobRegenHp, null, raiseEventOptions, sendOptions);
            }
            else
            {
                if (curHp >= maxHp)
                {
                    return;
                }

                curHp += maxHp * mobSetting.regenHpRatio;

                SetGauge();
            }            
        }

        public bool IsDie()
        {
            return curHp <= 0f;
        }

        public void ResetHp()
        {
            curHp = maxHp;
            SetGauge();
        }
    }
}