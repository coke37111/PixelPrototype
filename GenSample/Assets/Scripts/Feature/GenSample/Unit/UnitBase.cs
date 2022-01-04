using Assets.Scripts.Managers;
using Assets.Scripts.Settings.SO;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class UnitBase : MonoBehaviour
    {
        protected HpBar hpbar;
        protected AtkTypeSlot atkTypeSlot;
        protected Rigidbody rb;
        private GameObject hitEffect;
        private GameObject hitEffect2;
        private GameObject critHitEffect;

        protected float curAtkDelay;
        private float maxHp;

        private float oldCurHp;
        protected float curHp {
            get => oldCurHp;
            set
            {
                if(oldCurHp != value)
                {
                    oldCurHp = value;
                    SetGauge();
                }
            }
        }

        private UnitParts unitParts;
        protected PlayerUnitSettingSO playerUnitSetting;

        private bool oldLeftDir;
        protected bool isLeftDir
        {
            get => oldLeftDir;
            set
            {
                if (oldLeftDir != value)
                {
                    oldLeftDir = value;
                    OnChangeDir(value);
                }
            }
        }        

        protected bool controlable;
        
        public enum ATK_TYPE
        {
            melee,
            missile
        }
        protected ATK_TYPE atkType;
        public int teamNum 
        {
            get;
            set;
        }

        #region UNITY

        protected virtual void Update()
        {
            if (!controlable)
                return;

            Move();
            Jump();

            switch (atkType)
            {
                case ATK_TYPE.melee:
                    {
                        Attack();
                        break;
                    }
                case ATK_TYPE.missile:
                    {
                        Fire();
                        break;
                    }
            }
        }

        protected virtual void OnCollisionEnter(Collision coll)
        {
        }

        protected virtual void OnCollisionExit(Collision coll)
        {
        }

        #endregion

        #region abstract method

        protected abstract void Move();
        protected abstract void Jump();
        protected abstract void Attack();
        protected abstract void Fire();
        #endregion

        public virtual void Init()
        {
            rb = GetComponent<Rigidbody>();
            unitParts = transform.GetComponentInChildren<UnitParts>();
            if (unitParts == null)
                Log.Error($"unitParts component가 존재하지 않습니다!");

            hpbar = transform.GetComponentInChildren<HpBar>();
            if(hpbar == null)
                Log.Error($"HpBar component가 존재하지 않습니다!");

            atkTypeSlot = transform.GetComponentInChildren<AtkTypeSlot>();
            if (atkTypeSlot == null)
                Log.Error($"AtkTypeSlot component가 존재하지 않습니다!");

            transform.SetParent(FindObjectOfType<UnitContainer>().transform);

            playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
            maxHp = playerUnitSetting.hp;
            curHp = maxHp;

            atkType = (ATK_TYPE)(UnityEngine.Random.Range(0, Enum.GetValues(typeof(ATK_TYPE)).Length));
            atkTypeSlot.Build((int)atkType);

            teamNum = -1;

            isLeftDir = UnityEngine.Random.Range(0f, 1f) >= .5f;
        }

        protected virtual void OnChangeDir(bool isLeft)
        {
            if (unitParts == null)
                Log.Error($"unitParts component가 존재하지 않습니다!");
            else
                unitParts.FlipX(isLeft);
        }

        public void SetSprite(Dictionary<string, string> unitPartsList)
        {
            if (unitParts == null)
            {
                Log.Error($"unitParts component가 존재하지 않습니다!");
                return;
            }

            foreach (string unitPartName in unitPartsList.Keys)
            {
                string resPath = unitPartsList[unitPartName];
                unitParts.SetSprite(unitPartName, resPath);
            }

            unitParts.RotateSprite();
        }

        public virtual void ResetSpawnPos(Vector3 pos) { }
        public virtual void Knockback(float centerX, float centerZ) { }

        public virtual void AttackBy(UnitLocalPlayer unitLocalPlayer)
        {
            MakeHitEffect();

            curHp -= GetFinalDamage(unitLocalPlayer.GetAtk(), GetDef());
            if (curHp <= 0f)
            {
                Destroy(gameObject);
            }
        }

        public bool IsDie()
        {
            return curHp <= 0f;
        }

        public float GetAtk()
        {
            return playerUnitSetting.atk;
        }

        public float GetDef()
        {
            return playerUnitSetting.def;
        }

        protected float GetFinalDamage(float atk, float def)
        {
            float result = atk - def;
            return result <= 0f ? 0f : result;
        }

        protected void MakeHitEffect()
        {
            string effPath = $"Prefab/Effect/";

            bool isCrit = UnityEngine.Random.Range(0f, 1f) >= .7f;
            if (isCrit)
            {
                effPath += "damage_critical";

                if (critHitEffect == null)
                {
                    GameObject pfCritHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                    critHitEffect = Instantiate(pfCritHitEff, transform);
                }

                critHitEffect.transform.localPosition = new Vector3(UnityEngine.Random.Range(-.2f, .2f), UnityEngine.Random.Range(-.2f, .2f), 0f);
                critHitEffect.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                if (UnityEngine.Random.Range(0f, 1f) > .5f)
                {
                    effPath += "damage_001";

                    if (hitEffect == null)
                    {
                        GameObject pfHitEff = ResourceManager.LoadAsset<GameObject>(effPath);
                        hitEffect = Instantiate(pfHitEff, transform);
                    }

                    hitEffect.transform.localPosition = new Vector3(UnityEngine.Random.Range(-.2f, .2f), UnityEngine.Random.Range(-.2f, .2f), 0f);
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

                    hitEffect2.transform.localPosition = new Vector3(UnityEngine.Random.Range(-.2f, .2f), UnityEngine.Random.Range(-.2f, .2f), 0f);
                    hitEffect2.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        private void SetGauge()
        {
            float hpRatio = curHp / maxHp;
            hpbar.SetGauge(hpRatio);
        }

        public bool IsSameTeam(int teamNum)
        {
            if (this.teamNum == -1)
                return false;

            return this.teamNum == teamNum;
        }
    }
}