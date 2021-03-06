using Assets.Scripts.Feature.Sandbox.Cube;
using Assets.Scripts.Managers;
using Assets.Scripts.Settings;
using Assets.Scripts.Util;
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
        private bool isPlayingAtk;
        private bool canJump;
        protected string curEffColor;

        private float curFireDelay;
        private bool canFire;

        private MobController targetMob;
        private GameObject atkEffectL;
        private GameObject atkEffectR;
        private List<UnitBase> targetUnitList;

        #region UNITY

        protected override void OnTriggerEnter(Collider other)
        {
            base.OnTriggerEnter(other);

            if (RoomSettings.roomType == RoomSettings.ROOM_TYPE.Pvp)
            {
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
        }

        protected override void OnTriggerExit(Collider other)
        {
            base.OnTriggerExit(other);

            if (RoomSettings.roomType == RoomSettings.ROOM_TYPE.Pvp)
            {
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
        }

        protected override void OnCollisionEnter(Collision coll)
        {
            base.OnCollisionEnter(coll);

            if (coll.gameObject.tag == "Ground" || coll.gameObject.tag == "Cube")
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

        private void OnDisable()
        {
            if(spineListener != null)
                spineListener.UnregisterAtkListener(AttackReal);
        }

        #endregion

        #region abstract override
        protected override void Attack()
        {
            if(skelAnim == null)
            {
                if (canAtk)
                {
                    if (curAtkDelay >= playerUnitSetting.atkDelay)
                    {
                        curAtkDelay = 0f;

                        AttackReal();
                    }
                    else
                    {
                        curAtkDelay += Time.deltaTime;
                    }
                }
            }
            else
            {
                if (canAtk && !isPlayingAtk)
                {
                    if (curAtkDelay >= playerUnitSetting.atkDelay)
                    {
                        curAtkDelay = 0f;

                        skelAnim.SetTrigger("isAtk");
                        isPlayingAtk = true;
                    }
                    else
                    {
                        curAtkDelay += Time.deltaTime;
                    }
                }
            }
        }

        

        protected override void Jump()
        {
            if (isClimb)
                return;

            float jumpPower = playerUnitSetting.jumpPower;
            //if(belowCube != null && belowCube.GetCubeType() == CubeBase.CUBE_TYPE.JumpCube)
            //{
            //    JumpCube jumpCube = belowCube.GetComponent<JumpCube>();
            //    if (jumpCube == null)
            //        return;

            //    jumpPower *= jumpCube.jumpScale;
            //}

            if (Input.GetKeyDown(KeyCode.Space) && canJump)
            {
                canJump = false;
                rb.AddForce(Vector3.up * jumpPower);
            }
        }

        protected override void Move()
        {
            if (lockMove)
            {
                if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
                    Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    lockMove = false;
                }

                return;
            }

            if (isClimb)
            {
                Vector3 delta = Vector3.zero;

                if (Input.GetKey(KeyCode.W))
                {
                    delta.y += playerUnitSetting.speed;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    delta.y -= playerUnitSetting.speed;
                }

                if (delta != Vector3.zero)
                {
                    transform.position += delta * Time.deltaTime;
                }

                isMove = delta != Vector3.zero;

                OnChnagePosition?.Invoke(transform.position);

            }
            else
            {
                Vector3 delta = Vector3.zero;

                if (Input.GetKey(KeyCode.W))
                {
                    delta.z += playerUnitSetting.speed;
                }
                if (Input.GetKey(KeyCode.S))
                {
                    delta.z -= playerUnitSetting.speed;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    delta.x -= playerUnitSetting.speed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    delta.x += playerUnitSetting.speed;
                }

                //if (belowCube != null && belowCube.GetCubeType() == CubeBase.CUBE_TYPE.IceCube)
                //{
                //    accDelta = Vector3.Lerp(accDelta, delta, belowCube.GetComponent<IceCube>().dampRatio);
                //    transform.position += accDelta * Time.deltaTime;
                //}else if(belowCube == null && accDelta != Vector3.zero)
                //{
                //    transform.position += accDelta * Time.deltaTime;
                //}

                if (delta.x != 0)
                    isLeftDir = delta.x < 0;

                if (delta != Vector3.zero)
                {
                    transform.position += delta * Time.deltaTime;

                    moveDir = delta.normalized;
                }

                isMove = delta != Vector3.zero;

                OnChnagePosition?.Invoke(transform.position);
            }
        }

        #endregion

        public override void Init()
        {
            base.Init();

            canAtk = false;
            canJump = true;
            controlable = true;
            isPlayingAtk = false;

            moveDir = isLeftDir ? Vector3.left : Vector3.right;
            curFireDelay = 0f;
            canFire = true;
        }

        public override void MakeSpine(string spinePath)
        {
            base.MakeSpine(spinePath);

            spineListener.RegisterAtkListener(AttackReal);
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

        protected void AttackReal()
        {
            bool showAtkEff = false;
            if (targetMob != null)
            {
                if (targetMob.IsDie())
                {
                    return;
                }

                //targetMob.AttackBy(this);
                showAtkEff = true;
            }

            if (targetUnitList != null)
            {
                List<UnitBase> removePlayer = null;
                foreach (UnitBase unitPlayer in targetUnitList)
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

                if (removePlayer != null)
                {
                    foreach (UnitBase removeUnit in removePlayer)
                    {
                        if (targetUnitList.Contains(removeUnit))
                            targetUnitList.Remove(removeUnit);
                    }

                    CheckCanAtk();
                }
            }

            if (showAtkEff)
                ShowAtkEff();

            isPlayingAtk = false;
        }
    }
}