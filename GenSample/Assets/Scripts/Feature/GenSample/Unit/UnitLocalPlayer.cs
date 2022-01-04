using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Settings.SO;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Feature.GenSample
{
    public class UnitLocalPlayer : UnitBase
    {
        public readonly string[] effColorList = new string[] {
            "blue",
            "green",
            "purple",
            "red",
            "yellow"
        };

        public event Action<Vector3> OnChnagePosition;
        public Transform effContainerL;
        public Transform effContainerR;

        private bool canAtk;
        private bool canJump;
        protected string curEffColor;

        protected Vector3 moveDir; // misile 발사 방향을 위한 변수
        private float curFireDelay;
        private bool canFire;

        private MobController targetMob;
        private GameObject atkEffectL;
        private GameObject atkEffectR;
        private List<UnitBase> targetUnitList;

        #region UNITY

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (RoomSettings.roomType != RoomSettings.ROOM_TYPE.Pvp)
                return;

            if (other.gameObject.tag == "Player")
            {
                if (targetUnitList == null)
                    targetUnitList = new List<UnitBase>();

                UnitBase targetUnit = other.gameObject.GetComponent<UnitBase>();
                if (IsSameTeam(targetUnit.teamNum))
                    return;

                canAtk = true;
                if (targetUnitList.Contains(targetUnit))
                    return;
                targetUnitList.Add(targetUnit);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (RoomSettings.roomType != RoomSettings.ROOM_TYPE.Pvp)
                return;

            if (other.gameObject.tag == "Player")
            {
                if (targetUnitList == null)
                    return;

                UnitBase targetUnit = other.gameObject.GetComponent<UnitBase>();
                if (targetUnitList.Contains(targetUnit))
                    targetUnitList.Remove(targetUnit);
            }

            CheckCanAtk();
        }

        protected override void OnCollisionEnter(Collision coll)
        {
            base.OnCollisionEnter(coll);

            if (coll.gameObject.tag == "Ground")
            {
                canJump = true;
            }
            else if (coll.gameObject.tag == "Mob")
            {
                canAtk = true;
                targetMob = coll.gameObject.GetComponent<MobController>();
            }
        }

        protected override void OnCollisionExit(Collision coll)
        {
            base.OnCollisionExit(coll);

            if (coll.gameObject.tag == "Mob")
            {
                targetMob = null;
            }

            CheckCanAtk();
        }

        #endregion

        #region abstract override
        protected override void Attack()
        {
            if (canAtk)
            {
                if (curAtkDelay >= playerUnitSetting.atkDelay)
                {
                    curAtkDelay = 0f;

                    bool showAtkEff = false;
                    if(targetMob != null)
                    {
                        if (targetMob.IsDie())
                        {
                            return;
                        }

                        targetMob.AttackBy(this);
                        showAtkEff = true;
                    }

                    if(targetUnitList != null)
                    {
                        List<UnitBase> removePlayer = null;
                        foreach(UnitBase unitPlayer in targetUnitList)
                        {
                            if (unitPlayer.IsDie())
                            {
                                if (removePlayer == null)
                                    removePlayer = new List<UnitBase>();
                                removePlayer.Add(unitPlayer);
                                continue;
                            }

                            unitPlayer.AttackBy(this);
                            showAtkEff = true;
                        }

                        if(removePlayer != null)
                        {
                            foreach(UnitBase removeUnit in removePlayer)
                            {
                                if (targetUnitList.Contains(removeUnit))
                                    targetUnitList.Remove(removeUnit);
                            }

                            CheckCanAtk();
                        }
                    }
                    
                    if(showAtkEff)
                        ShowAtkEff();
                }
                else
                {
                    curAtkDelay += Time.deltaTime;
                }
            }
        }

        protected override void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                canJump = false;
                rb.AddForce(Vector3.up * 150f);
            }
        }

        protected override void Move()
        {
            Vector3 delta = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
                delta.z += (playerUnitSetting.speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.S))
                delta.z -= (playerUnitSetting.speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.A))
                delta.x -= (playerUnitSetting.speed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D))
                delta.x += (playerUnitSetting.speed * Time.deltaTime);

            if (delta.x != 0)
                isLeftDir = delta.x < 0;

            if (delta != Vector3.zero)
            {
                transform.position += delta;

                moveDir = delta.normalized;
            }

            isMove = delta != Vector3.zero;

            OnChnagePosition?.Invoke(transform.position);
        }

        #endregion

        public override void Init()
        {
            base.Init();

            canAtk = false;
            canJump = true;
            controlable = true;

            moveDir = isLeftDir ? Vector3.left : Vector3.right;
            curFireDelay = 0f;
            canFire = true;
        }

        protected virtual void ShowAtkEff()
        {
            SetAtkEffColor();
            MakeAtkEffect();
        }

        public void SetAtkEffColor()
        {
            if (!string.IsNullOrEmpty(curEffColor))
                return;

            int colorIdx = UnityEngine.Random.Range(0, effColorList.Length);
            curEffColor = effColorList[colorIdx];
        }

        public void MakeAtkEffect()
        {
            if (isLeftDir)
            {
                if (atkEffectL == null)
                {
                    GameObject pfAtkEff =
                        ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_{curEffColor}_L");
                    atkEffectL = Instantiate(pfAtkEff, effContainerL);
                    atkEffectL.transform.localPosition = Vector3.zero;
                }

                atkEffectL.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                if (atkEffectR == null)
                {
                    GameObject pfAtkEff =
                        ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_{curEffColor}_R");
                    atkEffectR = Instantiate(pfAtkEff, effContainerR);
                    atkEffectR.transform.localPosition = Vector3.zero;
                }

                atkEffectR.GetComponent<ParticleSystem>().Play();
            }
        }

        public override void ResetSpawnPos(Vector3 pos)
        {
            base.ResetSpawnPos(pos);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = pos;
        }
        
        public override void Knockback(float centerX, float centerZ)
        {
            var distance = Vector3.Distance(new Vector3(centerX, transform.position.y, centerZ), transform.position);

            if (canJump && distance <= 1.3f)
            {
                Vector3 diffPos = transform.position - new Vector3(centerX, transform.position.y, centerZ);
                Vector3 dir = diffPos.normalized;
                rb.AddForce(dir * 300f + Vector3.up * 150f);
            }
        }

        private void CheckCanAtk()
        {
            if (targetMob == null &&
                            (targetUnitList == null || targetUnitList.Count <= 0))
            {
                canAtk = false;
                curAtkDelay = 0f;
            }
        }

        protected override void Fire()
        {
            if (canFire)
            {
                if (Input.GetKeyDown(KeyCode.CapsLock))
                {
                    MakeMissile();

                    canFire = false;
                }
            }
            else
            {
                if(curFireDelay >= playerUnitSetting.fireDelay)
                {
                    canFire = true;
                    curFireDelay = 0f;
                }
                else
                {
                    curFireDelay += Time.deltaTime;
                }
            }
        }

        protected virtual void MakeMissile()
        {
            Vector3 initPos = transform.position + (moveDir + Vector3.up) * .25f;
            GameObject pfBullet = ResourceManager.LoadAsset<GameObject>("Prefab/Missile");
            GameObject goBullet = Instantiate(pfBullet, initPos, Quaternion.identity, transform);
            Missile bullet = goBullet.GetComponent<Missile>();
            bullet.InitializeBullet(this, moveDir, 0f);
        }
    }
}