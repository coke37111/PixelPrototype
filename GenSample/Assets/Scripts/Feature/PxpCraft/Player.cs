using Assets.Scripts.Managers;
using Assets.Scripts.Util;
using Spine.Unity;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Feature.PxpCraft
{
    public class Player : MonoBehaviour
    {
        private Transform trSpine;
        private Animator skelAnim;
        private Rigidbody2D rBody;
        private Transform effectContainerL;
        private Transform effectContainerR;
        private CollisionEventListener collEventListener;

        private GameObject effL;
        private GameObject effR;
        private bool isLeft;

        public float jumpPower = 100f;
        public float speed = 5f;

        private bool isAttacked;

        // Use this for initialization
        void Start()
        {
            trSpine = GetComponentInChildren<SkeletonMecanim>()
                .transform;
            skelAnim = GetComponentInChildren<SkeletonMecanim>()
                .GetComponent<Animator>();
            rBody = GetComponent<Rigidbody2D>();
            effectContainerL = transform.Find("Effect/L");
            effectContainerR = transform.Find("Effect/R");
            collEventListener = GetComponentInChildren<CollisionEventListener>();

            collEventListener.RegisterListner("Attack", AttackBy);

            isLeft = trSpine.localScale.x < 0f;
            isAttacked = false;
        }

        // Update is called once per frame
        void Update()
        {
            Move();
            Jump();
            Attack();
        }

        private void Move()
        {
            float axisX = Input.GetAxis("Horizontal");

            transform.Translate(Vector3.right * axisX * speed * Time.deltaTime);

            skelAnim.SetBool("isMove", axisX != 0f);

            Vector3 trScale = trSpine.localScale;
            if(axisX > 0f)
            {
                trScale.x = -Mathf.Abs(trScale.x);
                isLeft = false;
            }else if(axisX < 0f)
            {
                trScale.x = Mathf.Abs(trScale.x);
                isLeft = true;
            }
            trSpine.localScale = trScale;
        }

        private void Jump()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rBody.AddForce(Vector3.up * jumpPower);
            }
        }

        private void Attack()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                skelAnim.SetTrigger("isAtk");

                if (isLeft)
                {
                    if(effL == null)
                    {
                        GameObject pfAtkEff =
                       ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_red_L");
                        effL = Instantiate(pfAtkEff, effectContainerL);
                    }
                    effL.transform.localPosition = Vector3.zero;
                    effL.GetComponent<ParticleSystem>().Play();
                }
                else
                {
                    if(effR == null)
                    {
                        GameObject pfAtkEff =
                          ResourceManager.LoadAsset<GameObject>($"Prefab/Effect/attack_slash_eff_red_R");
                        effR = Instantiate(pfAtkEff, effectContainerR);
                    }
                    effR.transform.localPosition = Vector3.zero;
                    effR.GetComponent<ParticleSystem>().Play();
                }
            }
        }

        public void AttackBy(params object[] param)
        {
            if (isAttacked)
                return;
            isAttacked = true;

            Monster monster = (Monster)param[0];

            Log.Print($"Attacked monster {monster.name}!");
        }
    }
}