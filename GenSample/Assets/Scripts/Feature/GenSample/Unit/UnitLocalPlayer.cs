using Assets.Scripts.Managers;
using Assets.Scripts.Settings.SO;
using System;
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
        private float curAtkDelay;
        protected string curEffColor;

        private PlayerUnitSettingSO playerUnitSetting;
        private MobController targetMob;
        private GameObject atkEffectL;
        private GameObject atkEffectR;

        #region UNITY

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
                canAtk = false;
                targetMob = null;
            }
        }

        #endregion

        #region abstract override
        protected override void Attack()
        {
            if (canAtk && targetMob != null)
            {
                if (curAtkDelay >= playerUnitSetting.atkDelay)
                {
                    curAtkDelay = 0f;

                    if (targetMob.IsDie())
                    {
                        return;
                    }

                    targetMob.AttackBy(this);
                    CheckAtkEffect();
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
                transform.position += delta;

            OnChnagePosition?.Invoke(transform.position);
        }

        #endregion

        public override void Init()
        {
            base.Init();

            canAtk = false;
            canJump = true;
            controlable = true;

            playerUnitSetting = ResourceManager.LoadAsset<PlayerUnitSettingSO>(PlayerUnitSettingSO.path);
        }

        protected virtual void CheckAtkEffect()
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

        public float GetAtk()
        {
            return playerUnitSetting.atk;
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
    }
}